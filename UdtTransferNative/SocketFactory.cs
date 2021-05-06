using INetTransfer;

namespace  UdtTransferNative
{
    [TransferModel("udt")]
    public class SocketFactory : ISocketFactory
    {
        public ITransfer Create()
        {
            return new UdtDataNative();
        }

        public ITransfer Create(string address, int port)
        {
            var nng = new UdtDataNative();
            return nng;
        }
    }
}
