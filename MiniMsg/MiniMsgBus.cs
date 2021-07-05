using System;

namespace MiniMsg
{
    public  interface IMiniMsgBus
    {
        /// <summary>
        /// 订阅回调
        /// </summary>
        public event Action<string, byte[]> OnCall;
        public void Publish(string topic, byte[] bytes);
        public void Subscribe(string topic);
    }
}
