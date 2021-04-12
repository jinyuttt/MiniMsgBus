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

        private readonly  BlockingCollection<TopicStruct> topicStructs = new BlockingCollection<TopicStruct>();

        private static readonly Lazy<SubMgr> sub = new Lazy<SubMgr>(() => new SubMgr());
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

            Thread queue = new Thread(() =>
            {
                foreach (var p in topicStructs.GetConsumingEnumerable())
                {
                    if (p.Flage == 1)
                    {
                        //注册
                        SubTable.Instance.Add(p.Topic, UTF8Encoding.UTF8.GetString(p.msg));
                    }
                    else
                    {
                        //数据
                        Task.Run(() =>
                        {
                            if (dicSubObj.TryGetValue(p.Topic, out List<NngTopic> lst))
                            {
                                int cout = lst.Count;
                                for (int i = 0; i < cout; i++)
                                {
                                    var obj = lst[i];
                                    obj.Add(p.Topic, p.msg);
                                }
                            }
                        });
                    }
                }
            });
            thread.IsBackground = true;
            thread.Name = "queuesub";
            thread.Start();
        }

        public void Add(TopicStruct topicStruct)
        {
            topicStructs.Add(topicStruct);
        }

        private void Pgm_ReceiveTopic(string obj)
        {
            string[] tmp = obj.Split('_');
            StringBuilder builder = new StringBuilder();
            for(int i=0;i<tmp.Length-1;i++)
            {
                builder.AppendFormat("{0}_", tmp[i]);
            }
            builder.Remove(builder.Length - 1, 1);
            string topic = builder.ToString();

            //保存到全局发布列表
            if(topic== "Global")
            {
                //结果
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
            
            }
            PubTable.Instance.Add(topic, tmp[tmp.Length - 1]);
           var ov= LocalNode.GetLocal(topic);
            if (ov != null)
            {
                //本地已经订阅的发送订阅信息
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
                DataTransfer data = new DataTransfer();
                string addr = topic+"_" + LocalNode.LocalAddress;
                data.Send(topic,UTF8Encoding.UTF8.GetBytes(addr), pub,1);
            }
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
