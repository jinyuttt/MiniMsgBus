using System;
using System.Collections.Concurrent;
namespace MiniMsg
{
    public class InprocMsgBus : IMiniMsgBus
    {
        public event Action<string, byte[]> OnCall;
        private ConcurrentDictionary<string, ISubject> map = new ConcurrentDictionary<string, ISubject>();

        public void Publish(string topic, byte[] bytes)
        {
            ISubject msgSubject = new MsgSubject();
            if (!map.TryGetValue(topic, out msgSubject))
            {
                MsgSubject subject = new MsgSubject();
                map[topic] = subject;
                
            }
            msgSubject.SubjectState = bytes;
            msgSubject.Notify();
        }

        public void Subscribe(string topic)
        {
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
