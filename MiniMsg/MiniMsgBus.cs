using System;

namespace MiniMsg
{
    public  interface IMiniMsgBus
    {

        protected const string defaultTopic = "defaultTopicBus";

        /// <summary>
        /// 订阅回调
        /// </summary>
        public event Action<string, byte[]> OnCall;
        public ulong Publish(string topic, byte[] bytes);
        public void Subscribe(string topic);
    }
}
