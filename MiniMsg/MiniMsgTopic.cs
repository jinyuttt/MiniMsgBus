using System;

namespace MiniMsg
{
    /// <summary>
    /// 订阅发布接口
    /// </summary>
    public class MiniMsgTopic
    {
        /// <summary>
        /// 订阅回调
        /// </summary>
        public event  Action<string, byte[]> OnCall = null;

        /// <summary>
        /// 订阅
        /// </summary>
        /// <param name="topic">订阅主题</param>
        public void Subscribe(string topic)
        {
         
            SubMgr.Instance.SendSub(topic, this);
        }

        /// <summary>
        /// 发布
        /// </summary>
        /// <param name="topic">主题</param>
        /// <param name="msg">数据</param>
        public void Publish(string topic, byte[] msg)
        {
            PubMgr.Instance.Send(topic, msg);
            //
        }

        /// <summary>
        /// 内部数据回传
        /// </summary>
        /// <param name="topic"></param>
        /// <param name="msg"></param>
        internal void Add(string topic, byte[] msg)
        {
            if (OnCall != null)
            {
                OnCall(topic, msg);
            }
        }
    }
}
