namespace MiniMsg
{
    public  class BusFactory
    {
        public static IMiniMsgBus Create(BusType busType)
        {
            IMiniMsgBus msgBus = null;
            switch (busType)
            {
                case BusType.Inpoc:
                    msgBus= new InprocMsgBus();
                    break;
                case BusType.Ipc:
                    msgBus = new IpcMsgBus();
                    break;
                case BusType.tcp:
                    msgBus = new NetMsgBus();
                    break;
                default:
                    msgBus = new NetMsgBus();
                    break;

            }
            return msgBus;
        }
    }
    public class PtpFactory
    {
        public static IPtPNet Create()
        {
            return new PtPNet();
        }
    }

    public enum BusType
    {
       /// <summary>
       /// 进程内
       /// </summary>
        Inpoc,
        
        /// <summary>
        /// 进程间
        /// </summary>
        Ipc,

        /// <summary>
        /// 网络
        /// </summary>
        tcp
    }
}
