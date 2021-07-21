using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniMsg
{
   public interface ISubject
    {
        //添加观察者 送零食的加进来，老板来了通知你
        void Add(Observer observer);
        //删除观察者 不送零食的秘书小妹就不通知了
        void Remove(Observer observer);
        //主题状态
        byte[] SubjectState { get; set; }
        //通知方法
        void Notify();
    }
}
