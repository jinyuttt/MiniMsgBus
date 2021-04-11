using System;
using ZeroMQ;
using System.Collections.Concurrent;
namespace MiniMsg
{

    /// <summary>
    /// 主题传输
    /// </summary>
    public  class TopicPgm
    {

        private string epgmAddres = "";
        public string LocalAddress { get; set; } = "127.0.0.1";

        public string MultAddress { get; set; } = "239.192.1.1:5555";

        private string Address
        {
            get
            {
                if (string.IsNullOrEmpty(epgmAddres)) { epgmAddres = "epgm://" + LocalAddress + ";" + MultAddress; }
                return epgmAddres;
            }
        }

        public event Action<string> ReceiveTopic;

        /// <summary>
        /// 接收发布列表更新
        /// </summary>
        public void PgmSub()
        {
            using (var requester = new ZSocket(ZSocketType.XSUB))
            {
                // Connect
                requester.Connect(Address);

              
                   // string requestText = "Hello";
                    //Console.Write("Sending {0}...", requestText);

                   

                    // Receive
                    using (ZFrame reply = requester.ReceiveFrame())
                    {
                        //Console.WriteLine(" Received: {0} {1}!", requestText, reply.ReadString());
                        if(ReceiveTopic!=null)
                        {
                            ReceiveTopic(reply.ReadString());
                        }
                    }
                
            }
        }

        /// <summary>
        /// 更新发布地址
        /// </summary>
        /// <param name="topic"></param>
        public void PgmPub(string topic)
        {
            using (var requester = new ZSocket(ZSocketType.XPUB))
            {
                // Connect
                requester.Bind(Address);
                requester.Send(new ZFrame(topic+"_"+LocalAddress));
               // for (int n = 0; n < 10; ++n)
               // {
               //  string requestText = "Hello";
               // Console.Write("Sending {0}...", requestText);

                // Send
                //  requester.Send(new ZFrame(requestText));

                // Receive
                // using (ZFrame reply = requester.ReceiveFrame())
                // {
                //     Console.WriteLine(" Received: {0} {1}!", requestText, reply.ReadString());
                // }
                //}
            }
        }
    }
}
