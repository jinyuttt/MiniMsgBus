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
        private readonly ConcurrentDictionary<string, List<MiniMsgTopic>> dicSubObj = new ConcurrentDictionary<string, List<MiniMsgTopic>>();

        /// <summary>
        /// 接收的数据，包括订阅信息
        /// </summary>
        private readonly  BlockingCollection<TopicStruct> topicStructs = new BlockingCollection<TopicStruct>();

        /// <summary>
        /// 过滤消息
        /// </summary>
        private readonly ConcurrentDictionary<string,long> dicMsg = new ConcurrentDictionary<string, long>();

        TopicBroadcast topicBroadcast = new TopicBroadcast();

        private static readonly Lazy<SubMgr> sub = new Lazy<SubMgr>(() => new SubMgr());
        private readonly object lock_obj = new object();

        public static SubMgr Instance
        {
            get { return sub.Value; }
        }

        private SubMgr()
        {
            Init();
        }

        private void Init()
        {
            InitDataRecive();
            InitPgm();
            ProcessSub();
            RemoveFilter();
        }


        /// <summary>
        /// 调用空方法初始化实例
        /// </summary>
        public void OpenChanel()
        {
           
        }

        private (string,string,string) GetRealAddress(string address)
        {
           
            int index = address.IndexOf("//");
            int index1 = address.LastIndexOf(":");
            string protol = "";
            string ip = "";
            string port = "";
            if(index>-1)
            {
                protol = address.Substring(0, index-1);
            }
            ip = address.Substring(index + 2, index1 - index-2);
            port = address.Substring(index1 + 1);
            Console.WriteLine(string.Format("通信协议:{0} 绑定IP:{1} 绑定端口:{2}",protol,ip,port));
            return (protol, ip, port);
        }

        /// <summary>
        /// 初始化发布地址与接收
        /// </summary>
        private void InitPgm()
        {
          
            topicBroadcast.ReceiveTopic += TopicBroadcast_ReceiveTopic;
            topicBroadcast.TopicSub();
            //
            topicBroadcast.PgmPub("Global");
            Thread.Sleep(100);

        }

        private void TopicBroadcast_ReceiveTopic(string topic,string address)
        {
           
           
            //将新发布节点加入本地
            PubTable.Instance.Add(topic, address);
            Console.WriteLine("新加入的发布地址，主题:{0} 地址:{1}", topic, address);
            //查看本节点是否已经订阅过这个主题

            var ov = LocalNode.GetLocal(topic);
            if (ov != null)
            {
                Console.WriteLine("获取本地订阅");
                //本地已经订阅的主题发送订阅信息
                this.SendSub(topic, ov as MiniMsgTopic);
            }
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
                        SubTable.Instance.Add(p.Topic.Trim(), UTF8Encoding.UTF8.GetString(p.Msg).Trim(),p.MsgNode.Trim());
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
                            foreach(var k in dicSubObj.Keys)
                            {
                                if(k.CompareTo(p.Topic)==0)
                                {
                                    p.Topic = k;
                                    break;
                                }
                            }
                            if (dicSubObj.TryGetValue(p.Topic, out List<MiniMsgTopic> lst))
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
        private void InitDataRecive()
        {
            DataNative native = new DataNative();
            string tmp = "";
            if (string.IsNullOrEmpty(LocalNode.LocalAddress))
            {

                if (string.IsNullOrEmpty(LocalNode.protocol))
                {
                    tmp = string.Format("{0}:{1}", "*", LocalNode.LocalPort);
                }
                else
                {
                    tmp = string.Format("{0}://{1}:{2}", LocalNode.protocol, "*", LocalNode.LocalPort);
                }
                LocalNode.Netprotocol = native.Receive(tmp);
            }
            else
            {
                if (string.IsNullOrEmpty(LocalNode.protocol))
                {
                    tmp = string.Format("{0}:{1}", LocalNode.LocalAddress, LocalNode.LocalPort);
                }
                else
                {
                    tmp = string.Format("{0}://{1}:{2}", LocalNode.protocol, LocalNode.LocalAddress, LocalNode.LocalPort);
                }
                LocalNode.Netprotocol = native.Receive("tcp://"+LocalNode.LocalAddress+":"+LocalNode.LocalPort);
            }
            //
            Console.WriteLine("LocalNode.Netprotocol:" + LocalNode.Netprotocol);
            var items = GetRealAddress(LocalNode.Netprotocol);
            LocalNode.LocalAddress = items.Item2;
            LocalNode.LocalPort = int.Parse(items.Item3);
            LocalNode.protocol = items.Item1;
            Console.WriteLine("LocalNode.LocalAddress:" + LocalNode.LocalAddress);
            Console.WriteLine("LocalNode.port:" + LocalNode.LocalPort);
            topicBroadcast.GetLocalAddress();//初始化一次地址
             Thread rec = new Thread(() =>
              {
                  //接收数据
                  while (true)
                  {
                      var buf = native.GetData();
                   
                      var v = Util.Convert(buf);
                  
                      if (v.Flage == 0)
                      {
                         
                          topicStructs.Add(v);
                      }
                      else
                      {
                          try
                          {
                              //订阅地址加入
                              SubTable.Instance.Add(v.Topic, UTF8Encoding.UTF8.GetString(v.Msg), v.MsgNode);
                          }
                          catch(Exception ex)
                          {
                              Console.WriteLine(ex);
                          }
                      }
                  }
              });
            rec.IsBackground = true;
            rec.Name = "InitSub";
            rec.Start();
           
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
                    List<string> lst = new List<string>();
                    try
                    {
                        foreach (var p in dicMsg.Keys)
                        {
                            lst.Add(p);
                        }
                    }
                    catch(Exception ex)
                    {
                        //集合变化继续执行即可
                        Console.WriteLine("集合变化可以忽略," + ex.Message);
                    }
                    
                    foreach(var  p in lst)
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
            Console.WriteLine("recvice  {0}", obj);
           
            string[] tmp = obj.Split('|');//主题与发布地址用|分割
            StringBuilder builder = new StringBuilder();
            for(int i=0;i<tmp.Length-1;i++)
            {
                //防止主题中有|
                builder.AppendFormat("{0}|", tmp[i]);
            }
            builder.Remove(builder.Length - 1, 1);
            string topic = builder.ToString();

          
        
            //将新发布节点加入本地
            PubTable.Instance.Add(topic, tmp[tmp.Length - 1]);
            Console.WriteLine("新加入的发布地址，主题:{0} 地址:{1}", topic, tmp[tmp.Length - 1]);
            //查看本节点是否已经订阅过这个主题
          
            var ov= LocalNode.GetLocal(topic);
            if (ov != null)
            {
                Console.WriteLine("获取本地订阅");
                //本地已经订阅的主题发送订阅信息
                this.SendSub(topic, ov as MiniMsgTopic);
            }

        }

        /// <summary>
        /// 发送订阅信息
        /// </summary>
        /// <param name="topic">主题</param>
        /// <param name="sub">数据</param>
        public void  SendSub(string topic, MiniMsgTopic sub)
        {
             Console.WriteLine("发送订阅主题:{0}",topic);
            var lst = PubTable.Instance.GetAddress(topic);
            if(lst==null||lst.Count==0)
            {
                //没有发布地址，放入本地节点信息
                Console.WriteLine("临时放入本地：" +topic);
                LocalNode.AddLocal(topic,sub);
                return;
            }
            foreach (var  pub in lst)
            {

                //这里是因为InitSub方法先初始化
                //绑定了所有地址，需要将明确的地址发送出去订阅
                //取出真实的端口
                foreach (var p in TopicBroadcast.lstNodeAddress)
                {

                    DataTransfer.Send(topic, Encoding.UTF8.GetBytes(p), pub, 1);
                    Console.WriteLine("注册 topic: {0} addr:{1}", topic, pub);

                }

            }

            //保持本地订阅实例，用于数据回传
            if(dicSubObj.TryGetValue(topic,out List<MiniMsgTopic> lstTopic))
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
                        lstTopic = new List<MiniMsgTopic>();
                        lstTopic.Add(sub);
                        dicSubObj[topic] = lstTopic;
                    }
                }
            }
        }
   
        
    }
}
