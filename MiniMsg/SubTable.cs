using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace MiniMsg
{

    /// <summary>
    /// 本地订阅列表
    /// </summary>
   public class SubTable
    {
        private SubTable() { }
        private static readonly Lazy<SubTable>  sub = new Lazy<SubTable>(() => new SubTable());
        public static SubTable Instance
        {
            get { return sub.Value; }
        }

        /// <summary>
        /// 本节点订阅地址
        /// </summary>
        readonly ConcurrentDictionary<string, SubAddressLst> topicPub = new ConcurrentDictionary<string, SubAddressLst>();

        /// <summary>
        /// 加入订阅方地址
        /// </summary>
        /// <param name="topic">订阅节点需要主题</param>
        /// <param name="address">订阅节点地址</param>
        /// <param name="node">订阅节点节点标识guid</param>
        /// <returns></returns>
        public bool Add(string topic, string address,string node)
        {
            if (topicPub.TryGetValue(topic, out SubAddressLst lst))
            {
                lock (lst)
                {
                   //已经加入过地址
                    if (lst.LstAddress.Contains(address))
                    {
                        return false;
                    }
                    else
                    {
                        lst.LstAddress.Add(address);
                        var sub= lst.SubAddresses.Find(X => X.NodeFlage == node);
                        if (sub == null)
                        {
                            // 没有此节点
                            var p=new SubAddress() { Address = address, AllAddress = new List<string>(), ErrorAddress = new List<string>() };
                            p.AllAddress.Add(address);
                            lst.SubAddresses.Add(p);
                        }
                        else
                        {
                            //直接加入节点地址
                            sub.AllAddress.Add(node);
                        }
                    }
                }
            }
            else
            {
                lst = new SubAddressLst() { LstAddress = new List<string>(), SubAddresses = new List<SubAddress>() };
                lst.LstAddress.Add(address);
                lst.SubAddresses.Add(new SubAddress() { Address = address, AllAddress = new List<string>(), ErrorAddress = new List<string>(), NodeFlage=node });
                //Console.WriteLine("mmmm"+topic);
                topicPub[topic.Trim()] = lst;
                Console.WriteLine("加入：" + topicPub.Count);

            }
            return true;
        }

        /// <summary>
        /// 获取主题订阅节点
        /// </summary>
        /// <param name="topic"></param>
        /// <returns></returns>
        public List<string> GetAddress(string topic)
        {
            List<string> lst = new List<string>();
            SubAddressLst lstTMp = null;
            foreach(var kv in  topicPub.Keys)
            {
              
                if(kv.Trim().CompareTo(topic.Trim())==0)
                {
                    topic = kv;
                    break;
                }
            }
            if (topicPub.TryGetValue(topic.Trim(), out lstTMp))
            {
               
                int num = lstTMp.SubAddresses.Count;
              
                for (int i = 0; i < num; i++)
                {
                    lst.Add(lstTMp.SubAddresses[i].Address);
                }
            }
            return lst;
        }

        /// <summary>
        /// 获取主题地址信息
        /// </summary>
        /// <param name="topic"></param>
        /// <returns></returns>
        public SubAddressLst GetAddressLst(string topic)
        {
            SubAddressLst lstTMp = null;
            topicPub.TryGetValue(topic, out lstTMp);
            return lstTMp;
        }
    }
}
