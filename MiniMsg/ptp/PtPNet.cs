using System;
using System.Collections.Concurrent;
using System.Threading;

namespace MiniMsg
{
    public class PtPNet : IPtPNet
    {

        /// <summary>
        /// 数据接收
        /// </summary>
        public event Action<byte[]> OnRecvice;

        private readonly object lock_obj = new object();

        /// <summary>
        /// 地址
        /// </summary>
        public string Address { get; set; }

        /// <summary>
        /// 端口
        /// </summary>
        public int Port { get; set; }

        /// <summary>
        /// 发送地址
        /// </summary>
        private string sendAddress = null;

        bool isStart = false;

        /// <summary>
        /// 接收的数据
        /// </summary>
        private readonly BlockingCollection<byte[]> topicStructs = new BlockingCollection<byte[]>();

        private (string, string, string) GetRealAddress(string address)
        {

            int index = address.IndexOf("//");
            int index1 = address.LastIndexOf(":");
            string protol = "";
            string ip = "";
            string port = "";
            if (index > -1)
            {
                protol = address.Substring(0, index - 1);
            }
            ip = address.Substring(index + 2, index1 - index - 2);
            port = address.Substring(index1 + 1);
            Console.WriteLine(string.Format("通信协议:{0} 绑定IP:{1} 绑定端口:{2}", protol, ip, port));
            return (protol, ip, port);
        }

        /// <summary>
        /// 处理数据
        /// </summary>
        private void ProcessData()
        {
            Thread queue = new Thread(() =>
            {
                foreach (var p in topicStructs.GetConsumingEnumerable())
                {
                    if (OnRecvice != null)
                    {
                        OnRecvice(p);
                    }

                }
            });
            queue.IsBackground = true;
            queue.Name = "queuesub";
            queue.Start();
        }

        private void InitDataRecive(string localIP, int port)
        {
            DataNative native = new DataNative();
            string tmp = "";

            if (string.IsNullOrEmpty(LocalNode.protocol))
            {
                tmp = string.Format("{0}:{1}", localIP, port);
            }
            else
            {
                tmp = string.Format("{0}://{1}:{2}", LocalNode.protocol, localIP, port);
            }
            LocalNode.Netprotocol = native.Receive(tmp);
            //
            Console.WriteLine("LocalNode.Netprotocol:" + LocalNode.Netprotocol);
            var items = GetRealAddress(LocalNode.Netprotocol);


            Thread rec = new Thread(() =>
            {
                //接收数据
                while (true)
                {
                    var buf = native.GetData();

                }
            });
            rec.IsBackground = true;
            rec.Name = "InitSub";
            rec.Start();

        }

        /// <summary>
        /// 启动
        /// </summary>
        public void Start()
        {
            if (isStart)
            {
                return;
            }
            isStart = true;
            ProcessData();
            InitDataRecive(Address, Port);
        }

        /// <summary>
        /// 发送数据
        /// </summary>
        /// <param name="data"></param>
        public void Send(byte[] data)
        {
            DataNative native = new DataNative();
            if (string.IsNullOrEmpty(sendAddress))
            {
                if (string.IsNullOrEmpty(LocalNode.protocol))
                {
                    sendAddress = string.Format("{0}:{1}", Address, Port);
                }
                else
                {
                    sendAddress = string.Format("{0}://{1}:{2}", LocalNode.protocol, Address, Port);
                }
            }
            native.Send(sendAddress, data);
        }

        /// <summary>
        /// 发送数据
        /// </summary>
        /// <param name="addr">地址</param>
        /// <param name="data">数据</param>
        public void Send(string addr, byte[] data)
        {
            DataNative native = new DataNative();
            if (string.IsNullOrEmpty(sendAddress))
            {
                if (string.IsNullOrEmpty(LocalNode.protocol))
                {
                    sendAddress = string.Format("{0}:{1}", Address, Port);
                }
                else
                {
                    sendAddress = string.Format("{0}://{1}:{2}", LocalNode.protocol, Address, Port);
                }
            }
            native.Send(sendAddress, data);
        }

    }
}
