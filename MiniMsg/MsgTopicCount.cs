using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Concurrent;
namespace MiniMsg
{

    /// <summary>
    /// 获取发布信息情况
    /// </summary>
  public  class MsgTopicCount
    {
        private static readonly Lazy<MsgTopicCount> pub = new Lazy<MsgTopicCount>(() => new MsgTopicCount());
        private readonly ConcurrentDictionary<ulong, PubRecords> dic = new ConcurrentDictionary<ulong, PubRecords>(); 
        public static MsgTopicCount Instance
        {
            get { return pub.Value; }
        }

        public event Action<PubRecords> OnCall = null;

        private MsgTopicCount()
        {
          
        }
        public void AddTemp(PubRecords records)
        {
            dic[records.MsgId] = records;
        }

        public void Add(PubRecords records)
        {
            PubRecords tmp = null;
            if (dic.TryRemove(records.MsgId,out tmp))
            {
                records.SucessNum += tmp.SucessNum;
                records.FaildNum += tmp.FaildNum;
            }
            if(OnCall!=null)
            {
                OnCall(records);
            }
        }
    }
}
