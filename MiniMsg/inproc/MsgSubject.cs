using System.Collections.Generic;

namespace MiniMsg
{
    public class MsgSubject : ISubject
    {
       
        private readonly IList<Observer> observers = new List<Observer>();
        public byte[] SubjectState { get; set; }

        public void Add(Observer observer)
        {
            observers.Add(observer);
        }
        public void Remove(Observer observer)
        {
            observers.Remove(observer);
        }
       
        public void Notify()
        {
            foreach (Observer o in observers)
            {
                o.Update();
            }
        }
    }
}
