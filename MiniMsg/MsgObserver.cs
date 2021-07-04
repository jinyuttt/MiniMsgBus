using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniMsg
{
    public class MsgObserver : Observer
    {
        public MsgObserver(string name, ISubject sub) : base(name, sub) { }
        public override void Update()
        {
            Console.WriteLine($"通知内容：{sub.SubjectState},反应：{name}关闭股票行情，继续工作！");
        }
       
    }
}
