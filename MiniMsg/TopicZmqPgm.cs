using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using ZeroMQ;

namespace MiniMsg
{

    /// <summary>
    /// 组播订阅发布
    /// </summary>
    public class TopicZmqPgm
    {
        public string MultAddress { get; set; } = "239.192.1.1:5555";
        public event Action<string,byte[]> ReceiveTopic;

        readonly ZSocket subSocket = new ZSocket(ZSocketType.SUB);
        readonly ZSocket pubSocket = new ZSocket(ZSocketType.PUB);

        bool isStart = false;
        bool isBind = true;
        bool isSubBind = true;
        public List<string> LocalAddres { get; set; }
         
        /// <summary>
        /// 启动线程接收订阅
        /// </summary>
        private void ReceiveData()
        {
            if (isStart)
            {
                return;
            }
            isStart = true;
             Thread rectopic = new Thread(() =>
             {
                 while(true)
                 {
                     using (var reply = subSocket.ReceiveMessage())
                     {
                         try
                         {
                           
                             string topic = reply.Pop().ReadString(Encoding.UTF8);
                             var zf = reply.Pop();
                             var buf = zf.Read();
                             if (ReceiveTopic != null)
                             {
                                 ReceiveTopic(topic, buf); ;
                             }
                         }
                         catch (Exception ex)
                         {
                             Console.WriteLine(ex);
                         }
                     }
                 }
             });
            rectopic.IsBackground = true;
            rectopic.Name = "recpgm";
            rectopic.Start();
        }

        /// <summary>
        /// 订阅数据
        /// </summary>
        /// <param name="topic"></param>
        public void Subscribe(string topic)
        {
            if (isSubBind)
            {
                if (LocalAddres != null)
                {
                    foreach (var p in LocalAddres)
                    {
                        string addr = "epgm://" + p + ";" + MultAddress;
                        subSocket.Connect(addr);
                    }

                }
                isSubBind = false;
            }
            //
            if (string.IsNullOrEmpty(topic))
            {
                subSocket.SubscribeAll();
            }
            else
            {
                subSocket.Subscribe(topic);
            }
            if(isStart)
            {
                return;
            }
            lock (subSocket)
            {
                ReceiveData();
            }
        }
    
        /// <summary>
        /// 发布数据
        /// </summary>
        /// <param name="topic"></param>
        /// <param name="buf"></param>
        public void Publish(string topic,byte[]buf)
        {
            if (isBind)
            {
                if (LocalAddres != null)
                {
                    foreach (var p in LocalAddres)
                    {
                        string addr = "epgm://" + p + ";" + MultAddress;
                        pubSocket.Bind(addr);
                    }
                }
                isBind = false;
            }
            using (var message = new ZMessage())
            {

                message.Add(new ZFrame(topic,Encoding.UTF8));//这里定一个主题
                message.Add(new ZFrame(buf));//主题数据，只使用数据
                pubSocket.Send(message);

            }
        }
    
    }
}
