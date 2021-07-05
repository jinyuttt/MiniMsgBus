using System;
using System.Buffers;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

namespace MiniMsg
{

    /// <summary>
    /// 内存共享
    /// </summary>
    public class TopicIpcNative
    {
        public event Action<string, byte[]> ReceiveTopic;
        bool isStart = false;
       
        private object lock_obj=new object();
        readonly ArrayPool<byte> pool = ArrayPool<byte>.Shared;

       /// <summary>
       /// 处理数据
       /// </summary>
        private void ReceiveData()
        {
            if (isStart)
            {
                return;
            }
            isStart = true;
            Thread recTopic = new Thread(() =>
            {
                int bufLen = 0;
               
                while (true)
                {

                      var buf= IpcNativeMethods.Rec(ref bufLen);
                      byte[] dst = pool.Rent(bufLen);
                      Marshal.Copy(buf, dst, 0, bufLen);
                  
                    using (MemoryStream stream = new MemoryStream(dst))
                    {
                        var tpLen = pool.Rent(4);
                        
                        stream.Read(tpLen,0,4);
                        int len = BitConverter.ToInt32(tpLen);
                        var topic= pool.Rent(len);
                        stream.Read(topic,0,len);
                        string strtopic = Encoding.UTF8.GetString(topic,0,len);
                       
                        var buffer = new byte[bufLen - 4 - len];
                        stream.Read(buffer);
                      
                        if (ReceiveTopic != null)
                        {
                            ReceiveTopic(strtopic, buffer);
                        }
                        pool.Return(tpLen);
                        pool.Return(topic);
                        pool.Return(dst);
                    }
                    
                }
            });
            recTopic.IsBackground = true;
            recTopic.Name = "ipcrec";
            recTopic.Start();
        }
       
        /// <summary>
        /// 启动接收数据
        /// </summary>
        public void IpcRecv()
        {
            try
            {
                if (isStart)
                {
                    return;
                }
                lock (lock_obj)
                {

                    ReceiveData();

                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }


        /// <summary>
        /// 发布数据
        /// </summary>
        /// <param name="topic"></param>
        /// <param name="buf"></param>
        public void IpcSend(string topic, byte[] buf)
        {
            try
            {

                var tp = Encoding.UTF8.GetBytes(topic);
                byte[] len = BitConverter.GetBytes(tp.Length);
                byte[] buffer = new byte[4 + tp.Length + buf.Length];
                Array.Copy(len, buffer, 4);
                Array.Copy(tp,0, buffer, 4,tp.Length);
                Array.Copy(buf, 0, buffer, 4+tp.Length,buf.Length);
                IpcNativeMethods.Send(buffer, buffer.Length);



            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

    }
}
