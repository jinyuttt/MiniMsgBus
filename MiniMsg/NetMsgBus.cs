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
          return  msgTopic.Publish(topic, bytes);
        }

        public void Subscribe(string topic)
        {
            if(!IsInit)
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
