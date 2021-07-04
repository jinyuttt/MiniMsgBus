using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniMsg
{
    public class MsgSubject : ISubject
    {
       
        private readonly IList<Observer> observers = new List<Observer>();
        public string SubjectState { get; set; }

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
