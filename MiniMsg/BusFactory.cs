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

    public enum BusType
    {
        Inpoc,
        Ipc,
        tcp
    }
}
