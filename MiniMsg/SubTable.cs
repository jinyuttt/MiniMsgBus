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
        readonly ConcurrentDictionary<string, List<string>> topicPub = new ConcurrentDictionary<string, List<string>>();

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

        public List<string> GetAddress(string topic)
        {
            List<string> lst = null;
            topicPub.TryGetValue(topic, out lst);
            return lst;
        }
    }
}
