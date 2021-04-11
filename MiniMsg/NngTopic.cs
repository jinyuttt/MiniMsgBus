using System;

namespace MiniMsg
{
    /// <summary>
    /// 订阅发布接口
    /// </summary>
    public class NngTopic
    {
        Func<string, byte[]> OnCall = null;
        public void Subscribe(string topic, Func<string, byte[]> call)
        {
            OnCall = call;
            SubMgr.Instance.SendSub(topic, this);
        }

        public void Publish(string topic, byte[]msg)
        {
           
            //
        }
    }
}
