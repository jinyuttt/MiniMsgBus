using System;

namespace MiniMsg
{
    /// <summary>
    /// 点对点发送接收数据
    /// </summary>
    public interface IPtPNet
    {
        /// <summary>
        /// 发送或者接收地址
        /// </summary>
        string Address { get; set; }

        /// <summary>
        /// 端口
        /// </summary>
        int Port { get; set; }

        /// <summary>
        /// 接收数据
        /// </summary>
        event Action<byte[]> OnRecvice;

        /// <summary>
        /// 发送数据
        /// </summary>
        /// <param name="data"></param>
        void Send(byte[] data);

        /// <summary>
        ///发送数据临时地址
        /// </summary>
        /// <param name="addr"></param>
        /// <param name="data"></param>
        void Send(string addr, byte[] data);

        /// <summary>
        /// 启动接收
        /// </summary>
        void Start();
    }
}