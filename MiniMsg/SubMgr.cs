using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniMsg
{

   /// <summary>
   /// 订阅管理
   /// </summary>
  public  class SubMgr
    {
        private SubMgr() { }
        private static readonly Lazy<SubMgr> sub = new Lazy<SubMgr>(() => new SubMgr());
        public static SubMgr Instance
        {
            get { return sub.Value; }
        }

        public void  SendSub(string topic, NngTopic sub)
        {
            var lst = PubTable.Instance.GetAddress(topic);
            TopicPgm topicPgm = new TopicPgm();
            topicPgm.PgmPub();
        }
    }
}
