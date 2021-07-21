namespace MiniMsg
{
    public abstract class Observer
    {
        //名字
        protected string name;
        //观察者要知道自己订阅了那个主题
        protected ISubject sub;
        public Observer(string name, ISubject sub)
        {
            this.name = name;
            this.sub = sub;
        }
        //接受到通知后的更新方法
        public abstract void Update();
    }
}