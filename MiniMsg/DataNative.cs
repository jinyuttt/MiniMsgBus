using nng;
using nng.Native;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Text;
using System.Threading;

namespace MiniMsg
{

    /// <summary>
    /// 数据底层网络传输
    /// </summary>
    public class DataNative
    {
        static IAPIFactory<INngMsg> factory = null;
        static  INngMsg nodeMsg = null;
        private readonly BlockingCollection<byte[]> queue = new BlockingCollection<byte[]>();


        private static IAPIFactory<INngMsg> GetFactory()
        {
            //  FactoryExt
            if (factory == null)
            {
                var path = Path.GetDirectoryName(typeof(DataNative).Assembly.Location);
                var ctx = new NngLoadContext(path);
                factory = NngLoadContext.Init(ctx);
            }
            return factory;
        }

        
        public static string GetDialUrl(INngListener listener, string url)
        {
            if (url.EndsWith(":0", StringComparison.OrdinalIgnoreCase)
            && (url.StartsWith("tcp", StringComparison.OrdinalIgnoreCase) || url.StartsWith("ws", StringComparison.OrdinalIgnoreCase))
            )
            {
                var res = listener.GetOpt(nng.Native.Defines.NNG_OPT_LOCADDR, out nng_sockaddr addr);
                if (res == 0)
                {
                    ushort port = 0;
                    switch (addr.s_family)
                    {
                        case nng.Native.nng_sockaddr_family.NNG_AF_INET:
                            port = (ushort)System.Net.IPAddress.NetworkToHostOrder((short)addr.s_in.sa_port);
                            break;
                        case nng.Native.nng_sockaddr_family.NNG_AF_INET6:
                            port = (ushort)System.Net.IPAddress.NetworkToHostOrder((short)addr.s_in6.sa_port);
                            break;
                        default:
                          
                            break;
                    }
                    url = url.Substring(0, url.Length - 1) + port;
                }
            }
            return url;
        }

        /// <summary>
        /// 回复节点标识
        /// </summary>
        /// <returns></returns>
        public static INngMsg RepMsg()
        {
            if (nodeMsg == null)
            {
                 nodeMsg = factory.CreateMessage();
                var bytes = UTF8Encoding.UTF8.GetBytes(LocalNode.GUID);
                nodeMsg.Append(bytes);

            }
            return nodeMsg.Dup().Unwrap();//复制一次数据发送
        }

        /// <summary>
        /// 发送数据
        /// </summary>
        /// <param name="address">发送地址</param>
        /// <param name="bytes">数据</param>
        /// <returns></returns>
        public  byte[] Send(string address,byte[]bytes)
        {


            using (var reqSocket = GetFactory().RequesterOpen().ThenDial(address).Unwrap())
            {
                reqSocket.SetOpt(nng.Native.Defines.NNG_OPT_SENDTIMEO, new nng_duration { TimeMs = 100 });
                var msg = factory.CreateMessage();
                msg.Append(bytes);
                reqSocket.SendMsg(msg).Unwrap();
                return reqSocket.RecvMsg().Unwrap().AsSpan().ToArray();
            }
        }

        /// <summary>
        /// 获取接收的数据
        /// </summary>
        /// <returns></returns>
        public byte[] GetData()
        {
            return queue.Take();
        }


        /// <summary>
        /// 启动数据接收
        /// </summary>
        /// <param name="address">监听地址</param>
        /// <returns>真实地址</returns>
        public  string  Receive(string address)
        {
            var repSocket = GetFactory().ReplierOpen().ThenListenAs(out var listener, address).Unwrap();
            var str = GetDialUrl(listener, address);
            Thread thread = new Thread(() =>
            {
                while (true)
                {

                    var msg = repSocket.RecvMsg().Unwrap();
                    repSocket.SendMsg(RepMsg()).Unwrap();
                    var bytes = msg.AsSpan().ToArray();
                    queue.Add(bytes);//数据写入队列

                }
            });
            thread.IsBackground = true;
            thread.Name = "sub";
            thread.Start();
            return str;
        }
    }
}
