using System;
using System.Collections.Concurrent;

namespace MiniMsg
{
    public class IpcMsgBus : IMiniMsgBus
    {
        public event Action<string, byte[]> OnCall;
        TopicIpcNative topicIpc = new TopicIpcNative();
       volatile bool isInit = false;
        private ConcurrentDictionary<string, string> map = new ConcurrentDictionary<string, string>();

        public ulong Publish(string topic, byte[] bytes)
        {
            if (string.IsNullOrEmpty(topic))
            {
                topic = IMiniMsgBus.defaultTopic;
            }
            topicIpc.IpcSend(topic, bytes);
            return 0;
        }

        public void Subscribe(string topic)
        {
            if (string.IsNullOrEmpty(topic))
            {
                topic = IMiniMsgBus.defaultTopic;
            }
            if (!isInit)
            {
                topicIpc.ReceiveTopic += TopicIpc_ReceiveTopic;
                isInit = true;
            }
           
            map[topic] = "";
        }

        private void TopicIpc_ReceiveTopic(string arg1, byte[] arg2)
        {
            if(map.ContainsKey(arg1))
            {
                if(OnCall!=null)
                {
                    OnCall(arg1, arg2);
                }
            }
        }
    }
}
