using System;

namespace MiniMsg
{
    public class InprocSubscriber : Observer
    {
        public Action<string, byte[]> OnCall;
        public InprocSubscriber(string name, ISubject sub) : base(name, sub) { }
        public override void Update()
        {
            if(OnCall!=null)
            {
                OnCall(name, sub.SubjectState);
            }
        } 
       
    }
}
