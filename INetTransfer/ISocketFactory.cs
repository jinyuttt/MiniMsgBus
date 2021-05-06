namespace INetTransfer
{

    /// <summary>
    /// 创建通信接口
    /// </summary>
    public interface ISocketFactory
    {
        public ITransfer Create();

        public ITransfer Create(string address, int port);
    }
}
