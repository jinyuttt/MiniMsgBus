using System.Collections.Generic;
using System.Collections.Concurrent;
using System;

namespace MiniMsg
{

    /// <summary>
    /// 全局发布列表
    /// </summary>
    internal class PubTable
    {
        private PubTable() { }

        private static readonly Lazy<PubTable> pub = new Lazy<PubTable>(() => new PubTable());

        public static PubTable Instance
        {
            get {return pub.Value; }
        }

         readonly ConcurrentDictionary<string, List<string>> topicPub = new ConcurrentDictionary<string, List<string>>();

        /// <summary>
        /// 添加主题和地址
        /// </summary>
        /// <param name="topic">主题</param>
        /// <param name="address">发布地址:包括IP，端口，协议</param>
        /// <returns></returns>
        public bool Add(string topic, string address)
        {
            foreach(var k in  topicPub.Keys)
            {
                if(k.CompareTo(topic)==0)
                {
                    topic = k;
                    break;
                }
            }
            if (topicPub.TryGetValue(topic, out List<string> lst))
            {
                if (lst.Contains(address))
                {
                    return false;
                }
                else
                {
                    lst.Add(address);
                }
            }
            else
            {
                lst = new List<string>();
                lst.Add(address);
                topicPub[topic] = lst;
            }
            return true;
        }

        /// <summary>
        /// 获取主题发布地址
        /// </summary>
        /// <param name="topic"></param>
        /// <returns></returns>
        public List<string> GetAddress(string topic)
        {
          
            List<string> lst = null;
            foreach(var k in topicPub.Keys)
            {
                if(k.CompareTo(topic) ==0)
                {
                    topic = k;
                  
                    break;
                }
            }
           // Console.WriteLine("topicPub:"+ topicPub.Count);
            topicPub.TryGetValue(topic,out lst);
           
            return lst;
        }

        /// <summary>
        /// 主题节点
        /// </summary>
        /// <returns></returns>
        public  Dictionary<string,List<string>> GetPairs()
        {
            return new Dictionary<string, List<string>>(topicPub);
        }
    }
}
