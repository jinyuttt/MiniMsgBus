namespace INetTransfer
{
    public interface ITransfer
    {
         public static string NodeGuid { get; set; }
        public byte[] RepMessage();

        public string Receive(string address);

        public byte[] GetData();

        public byte[] Send(byte[] buf, int offset=0, int len = 0);

        public byte[] Send(string address, byte[] bytes);

        public void Close();

    }
}
