using System.Collections.Generic;
using System.Collections.Concurrent;
using System;

namespace MiniMsg
{

    /// <summary>
    /// 全局发布列表
    /// </summary>
    public class PubTable
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
        /// <param name="topic"></param>
        /// <param name="address"></param>
        /// <returns></returns>
        public bool Add(string topic, string address)
        {
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
                var lstTmp = new List<string>();
                lst.Add(address);
                topicPub[topic] = lstTmp;
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
            topicPub.TryGetValue(topic,out lst);
            return lst;
        }
    }
}
