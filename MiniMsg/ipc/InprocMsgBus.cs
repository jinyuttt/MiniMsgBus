using System;
using System.Collections.Concurrent;
namespace MiniMsg
{
    public class InprocMsgBus : IMiniMsgBus
    {
        public event Action<string, byte[]> OnCall;
        private readonly ConcurrentDictionary<string, ISubject> map = new ConcurrentDictionary<string, ISubject>();

        public ulong Publish(string topic, byte[] bytes)
        {
            if(string.IsNullOrEmpty(topic))
            {
                topic = IMiniMsgBus.defaultTopic;
            }
            ISubject msgSubject = new MsgSubject();
            if (!map.TryGetValue(topic, out msgSubject))
            {
                MsgSubject subject = new MsgSubject();
                map[topic] = subject;
                
            }
            msgSubject.SubjectState = bytes;
            msgSubject.Notify();
            return 0;
        }

        public void Subscribe(string topic)
        {
            if (string.IsNullOrEmpty(topic))
            {
                topic = IMiniMsgBus.defaultTopic;
            }
            ISubject subject = null;
            if (map.TryGetValue(topic,out subject))
            {
                InprocSubscriber observer = new InprocSubscriber(topic, subject);
                subject.Add(observer);
                observer.OnCall = OnCall;
            }
           
        }

       
    }
}
