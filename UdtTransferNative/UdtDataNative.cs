using INetTransfer;
using IUdtSocket;
using System;
using System.Buffers;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UdtCSharp;

namespace UdtTransferNative
{

    /// <summary>
    /// 数据底层网络传输
    /// </summary>
    public class UdtDataNative: ITransfer
    {
        
        private readonly BlockingCollection<byte[]> queue = new BlockingCollection<byte[]>();
        private ArrayPool<byte> pool = ArrayPool<byte>.Shared;
        private bool IsStop = false;
        private static byte[] RepMsg = null;
        private IUdtSocket.IUdtSocket lisSocket = null;
        static UdtDataNative()
        {
           var tmp= UTF8Encoding.UTF8.GetBytes(ITransfer.NodeGuid);
            RepMsg = new byte[4 + tmp.Length];
            using(MemoryStream stream=new MemoryStream(RepMsg))
            {
                stream.Write(BitConverter.GetBytes(tmp.Length));
                stream.Write(tmp);
            }
        }
      
       

        public static (string,string,int) GetUrl(string url)
        {
            string pre = url.Substring(0, url.IndexOf("//")+1);
            string address = url.Substring(url.IndexOf("//" + 1, url.LastIndexOf(":")));
            string addport = url.Substring(url.LastIndexOf(":" + 1));
            int port = int.Parse(addport);
            if(!url.Contains("//"))
            {
                pre = "";
            }
            return (pre,address, port);
        }

        public void Close()
        {
            IsStop = true;
            if (lisSocket != null)
            {
                lisSocket.Close();
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

        public string Receive(string address)
        {
            var socket = SocketFactoty.Create();
            var addr = GetUrl(address);
            socket.Bind(addr.Item2, addr.Item3);
            socket.Listen(100);
            lisSocket = socket;
            Thread rec = new Thread(() =>
            {
                while (!IsStop)
                {
                    IPEndPoint remote;
                    if(socket.State== UdtStatus.CLOSED)
                    {
                        break;
                    }
                    var c = socket.Accept(out remote);
                    Task.Run(() =>
                    {
                       
                            var buf = pool.Rent(1024);
                            int r = c.Receive(buf);
                            int len = BitConverter.ToInt32(buf, 0);
                            if (r < 1024)
                            {
                                byte[] data = new byte[len];
                                Array.Copy(buf, 4, data, 0, len);
                                queue.Add(data);
                                pool.Return(buf);
                            }
                            else
                            {

                                //继续全部接收
                                MemoryStream stream = new MemoryStream(len);
                                stream.Write(buf, 4, r - 4);
                                int dlen = len - r + 2;
                                while (dlen > 0)
                                {
                                    buf = pool.Rent(dlen);
                                    r = c.Receive(buf);
                                    stream.Write(buf, 0, r);
                                    pool.Return(buf);
                                    dlen -= r;
                                }
                                queue.Add(stream.ToArray());
                                stream.Close();
                                var tmp = this.RepMessage();
                                c.Send(this.RepMessage());
                            }
                           //不考虑长连接，只是单次接收
                           
                        
                    });

                }
                //socket.cl
            });
            rec.IsBackground = true;
            rec.Start();
            IPEndPoint iPEndPoint=socket.LocalEndPoint;
            return addr.Item1+ iPEndPoint.Address.ToString() + ":" + iPEndPoint.Port;
        }

        public byte[] RepMessage()
        {
            return RepMsg;
        }

        public byte[] Send(byte[] buf, int offset = 0, int len = 0)
        {
            return null;
            
        }

        public byte[] Send(string address, byte[] bytes)
        {
            var socket = SocketFactoty.Create();
            socket.Connect(address, 6666);
            byte[] rep = null;
            var ret = Task.Run(() =>
              {

                  byte[] buf = pool.Rent(65);
                  int r = socket.Receive(buf);
                  rep = new byte[r];
                  Array.Copy(buf, rep, r);
                  pool.Return(buf);
              });
            socket.Send(bytes);
            ret.Wait(2000);
          
            return rep;

        }

       
       
    }
}
