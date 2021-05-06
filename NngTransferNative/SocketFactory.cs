using INetTransfer;

namespace NngTransferNative
{
    [TransferModel("nng")]
    public class SocketFactory : ISocketFactory
    {
        public ITransfer Create()
        {
            return new NngDataNative();
        }

        public ITransfer Create(string address, int port)
        {
            var nng = new NngDataNative();
            return nng;
        }
    }
}
