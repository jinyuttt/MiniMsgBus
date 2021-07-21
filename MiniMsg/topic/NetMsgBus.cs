using System;

namespace MiniMsg
{
    public class NetMsgBus : IMiniMsgBus
    {
        public event Action<string, byte[]> OnCall;
        MiniMsgTopic msgTopic = new MiniMsgTopic();
        volatile bool IsInit = false;
        public ulong Publish(string topic, byte[] bytes)
        {
            if (string.IsNullOrEmpty(topic))
            {
                topic = IMiniMsgBus.defaultTopic;
            }
            return  msgTopic.Publish(topic, bytes);
        }

        public void Subscribe(string topic)
        {
            if (string.IsNullOrEmpty(topic))
            {
                topic = IMiniMsgBus.defaultTopic;
            }
            if (!IsInit)
            {
                msgTopic.OnCall += MsgTopic_OnCall;
                IsInit = true;
            }
            msgTopic.Subscribe(topic);
        }

        private void MsgTopic_OnCall(string arg1, byte[] arg2)
        {
           if(OnCall!=null)
            {
                OnCall(arg1, arg2);
            }
        }
    }
}
