using System;

namespace INetTransfer
{
    public  class MiniMsgProxy
    {
        public static ISocketFactory Factory = null;
        public ITransfer transfer = null;
        public Func<string, byte[], byte[]> MsgSend;
        public Func<string, string> MsgRecvice;
        public Func<byte[]> dataCall = null;
        public Action LisClose = null;
        public MiniMsgProxy()
        {
            this.MsgSend = this.Send;
            this.MsgRecvice= this.Receive;
            this.dataCall = this.GetData;
            this.LisClose = this.Close;
        }
        public byte[] Send(string address, byte[] v)
        {
            return Factory.Create().Send(address, v);
        }

        public string Receive(string v)
        {
            transfer = Factory.Create();
            return transfer.Receive(v);
        }

        public byte[] GetData()
        {
            return transfer.GetData();
        }

        public void Close()
        {
            transfer.Close();
        }
    }
}
