using System;
using ZeroMQ;

namespace MiniMsg
{

    /// <summary>
    /// 主题传输
    /// </summary>
    public  class TopicPgm
    {
        public void PgmSub()
        {
            using (var requester = new ZSocket(ZSocketType.XSUB))
            {
                // Connect
                requester.Connect("epgm://192.168.1.1;239.192.1.1:5555");

                for (int n = 0; n < 10; ++n)
                {
                    string requestText = "Hello";
                    Console.Write("Sending {0}...", requestText);

                   

                    // Receive
                    using (ZFrame reply = requester.ReceiveFrame())
                    {
                        Console.WriteLine(" Received: {0} {1}!", requestText, reply.ReadString());
                    }
                }
            }
        }
        public void PgmPub()
        {
            using (var requester = new ZSocket(ZSocketType.XPUB))
            {
                // Connect
                requester.Bind("pgm://192.168.0.158;239.192.1.1:5555");

                for (int n = 0; n < 10; ++n)
                {
                    string requestText = "Hello";
                    Console.Write("Sending {0}...", requestText);

                    // Send
                    requester.Send(new ZFrame(requestText));

                    // Receive
                    using (ZFrame reply = requester.ReceiveFrame())
                    {
                        Console.WriteLine(" Received: {0} {1}!", requestText, reply.ReadString());
                    }
                }
            }
        }
    }
}
