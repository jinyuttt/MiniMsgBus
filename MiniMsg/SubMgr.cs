using System;
using System.Text;
using System.Threading;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MiniMsg
{

    /// <summary>
    /// 订阅管理
    /// </summary>
    public  class SubMgr
    {
        /// <summary>
        /// 订阅者
        /// </summary>
        private readonly ConcurrentDictionary<string, List<NngTopic>> dicSubObj = new ConcurrentDictionary<string, List<NngTopic>>();

        /// <summary>
        /// 接收的数据，包括订阅信息
        /// </summary>
        private readonly  BlockingCollection<TopicStruct> topicStructs = new BlockingCollection<TopicStruct>();

        /// <summary>
        /// 过滤消息
        /// </summary>
        private readonly ConcurrentDictionary<string,long> dicMsg = new ConcurrentDictionary<string, long>();


        private static readonly Lazy<SubMgr> sub = new Lazy<SubMgr>(() => new SubMgr());

        public static SubMgr Instance
        {
            get { return sub.Value; }
        }

        private SubMgr()
        {
            InitSub();
            InitPgm();
            
            ProcessSub();
            RemoveFilter();
        }

        /// <summary>
        /// 初始化发布地址与接收
        /// </summary>
        private void InitPgm()
        {
            TopicPgm pgm = new TopicPgm();
            pgm.ReceiveTopic += Pgm_ReceiveTopic;
            Thread thread = new Thread(() =>
            {

                pgm.PgmSub();
            });
            thread.IsBackground = true;
            thread.Name = "pgmsub";
            thread.Start();
            pgm.PgmPub("Global");//内部主题，更新一次发布地址
            Thread.Sleep(100);
          


        }

        /// <summary>
        /// 处理数据
        /// </summary>
        private void  ProcessSub()
        {
            Thread queue = new Thread(() =>
            {
                foreach (var p in topicStructs.GetConsumingEnumerable())
                {
                    if (p.Flage == 1)
                    {
                        //订阅地址加入
                        SubTable.Instance.Add(p.Topic, UTF8Encoding.UTF8.GetString(p.Msg),p.MsgNode);
                    }
                    else
                    {
                        if(dicMsg.ContainsKey(p.MsgNode+p.MsgId))
                        {
                            return;
                        }
                        dicMsg[p.MsgNode + p.MsgId] = DateTime.Now.Ticks;
                        //数据处理
                        Task.Run(() =>
                        {
                            if (dicSubObj.TryGetValue(p.Topic, out List<NngTopic> lst))
                            {
                                int cout = lst.Count;
                                for (int i = 0; i < cout; i++)
                                {
                                    var obj = lst[i];
                                    obj.Add(p.Topic, p.Msg);
                                }
                            }
                        });
                    }
                }
            });
            queue.IsBackground = true;
            queue.Name = "queuesub";
            queue.Start();
        }

        
        /// <summary>
        /// 初始化接收数据，准备网络接收数据（订阅的数据和订阅信息）
        /// </summary>
        private void InitSub()
        {
            DataNative native = new DataNative(); 
            if (string.IsNullOrEmpty(LocalNode.LocalAddress))
            {
                LocalNode.GetNetworkInterface();
                LocalNode.LocalAddress = native.Receive("tcp://*:0");

            }
            else
            {
                LocalNode.LocalAddress = native.Receive("tcp://"+LocalNode.LocalAddress+":"+LocalNode.LocalPort);
            }
             Thread rec = new Thread(() =>
              {
                  //接收数据
                  while (true)
                  {
                      var buf = native.GetData();
                      var v = Util.Convert(buf);
                      if (v.Flage == 0)
                      {
                          //数据
                          topicStructs.Add(v);
                      }
                  }
              });
           
        }
        
        /// <summary>
        /// 定时清理重复的Key=节点标识+msgid
        /// </summary>
        private void RemoveFilter()
        {
            Thread thread = new Thread(() =>
            {
                while(true)
                {
                    Thread.Sleep(2000);
                    string[] keys = new string[dicMsg.Count];
                    dicMsg.Keys.CopyTo(keys, 0);
                    foreach(var  p in keys)
                    {
                        if(dicMsg.TryGetValue(p,out long ticks))
                        {
                            //清理超过2秒的数据，认为重复的数据不会超过2秒
                            //重复的原因是节点多卡绑定
                            if (DateTime.Now.Ticks-ticks> 2*1e9)
                            {
                                dicMsg.TryRemove(p, out ticks);
                            }
                             
                        }

                    }
                }
            });
            thread.IsBackground = true;
            thread.Name = "RemoveFilter";
            thread.Start();
        }
       
        /// <summary>
        /// 接收组播发布节点通知
        /// </summary>
        /// <param name="obj"></param>
        private void Pgm_ReceiveTopic(string obj)
        {
            string[] tmp = obj.Split('|');//主题与发布地址用|分割
            StringBuilder builder = new StringBuilder();
            for(int i=0;i<tmp.Length-1;i++)
            {
                //防止主题中有|
                builder.AppendFormat("{0}|", tmp[i]);
            }
            builder.Remove(builder.Length - 1, 1);
            string topic = builder.ToString();

            //保存到全局发布列表
            if(topic== "Global")
            {
                //如果接收到的是Global信息，则把本节点保持的所有发布节点发送出去，让新加入的节点获取
                //发布地址
                TopicPgm pgm = new TopicPgm();
                var dic = PubTable.Instance.GetPairs();
                foreach(var  kv in dic)
                {
                    foreach(var p in kv.Value)
                    {
                        pgm.PgmUpdate(kv.Key,p); 
                    }
                 
                }
                return;
            }
           Console.WriteLine("recvice topic {0}", topic);

            //将新发布节点加入本地
           PubTable.Instance.Add(topic, tmp[tmp.Length - 1]);

            //查看本节点是否已经订阅过这个主题
           var ov= LocalNode.GetLocal(topic);
            if (ov != null)
            {
                //本地已经订阅的主题发送订阅信息
                this.SendSub(topic, ov as NngTopic);
            }

        }

        /// <summary>
        /// 发送订阅信息
        /// </summary>
        /// <param name="topic">主题</param>
        /// <param name="sub">数据</param>
        public void  SendSub(string topic, NngTopic sub)
        {
            var lst = PubTable.Instance.GetAddress(topic);
            if(lst==null)
            {
                //没有发布地址，放入本地节点信息
                LocalNode.AddLocal(topic,sub);
                return;
            }
            foreach (var  pub in lst)
            {
               
                string addr = LocalNode.LocalAddress;
                //这里是因为InitSub方法先初始化
                if (addr.Contains("*"))
                {
                    //绑定了所有地址，需要将明确的地址发送出去订阅
                    //取出真实的端口
                    int index = addr.LastIndexOf(":");
                    int port = int.Parse(addr.Substring(index+1));
                    foreach(var p in LocalNode.LocalAddressFamily)
                    {
                        string subAddres = "tcp://" + p.IPV4 + ":" + port;
                        DataTransfer.Send(topic, Encoding.UTF8.GetBytes(subAddres), pub, 1);
                    }


                }
                else
                {
                    DataTransfer.Send(topic, Encoding.UTF8.GetBytes(addr), pub, 1);
                }
                
            }

            //保持本地订阅实例，用于数据回传
            //
            if(dicSubObj.TryGetValue(topic,out List<NngTopic> lstTopic))
            {
                lock(lstTopic)
                {
                    if (!lstTopic.Contains(sub))
                    {
                        lstTopic.Add(sub);
                    }
                }
            }
            else
            {
                lock(dicSubObj)
                {
                    if (dicSubObj.TryGetValue(topic,out lstTopic))
                    {
                        lock (lstTopic)
                        {
                           
                                if (!lstTopic.Contains(sub))
                                {
                                    lstTopic.Add(sub);
                                }
                            
                        }
                     
                    }
                    else
                    {
                        lstTopic = new List<NngTopic>();
                        lstTopic.Add(sub);
                        dicSubObj[topic] = lstTopic;
                    }
                }
            }
        }
    }
}
