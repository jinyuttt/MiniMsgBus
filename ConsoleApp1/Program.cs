using MiniMsg;
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ZeroMQ;

namespace ConsoleApp1
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine(System.Environment.Is64BitProcess);
            Task.Run(() => {
                PgmClient();
                

            });
            Task.Run(() => {
                // PgmServwr();
               
            });

            Console.Read();
            //try
            //{
            //    LocalNode.LocalAddress = "192.168.0.158";
            //    NngTopic nng = new NngTopic();
            //    nng.Subscribe("AAA");
            //    nng.Publish("BBB", Encoding.UTF8.GetBytes(DateTime.Now.ToString()));
            //}
            //catch(Exception ex)
            //{
            //    Console.WriteLine(ex);
            //}
            //Console.Read();
            //Console.WriteLine("Hello World!");
        }
        static void Send()
        {
            while(true)
            {
                DataNative dataNative = new DataNative();
                dataNative.Send("tcp://127.0.0.1:5556", System.Text.UTF8Encoding.UTF8.GetBytes(DateTime.Now.ToString()));
                Thread.Sleep(1000);
            }
          
        }
        static void Rec()
        {
            DataNative dataNative = new DataNative();
            dataNative.Receive("tcp://127.0.0.1:5556");
            Task.Run(() => {
                while (true)
                {
                    var ss = dataNative.GetData();
                    Console.WriteLine(UTF8Encoding.UTF8.GetString(ss));
                }
            });
          
            
        }

        static void SendPgm()
        {
            TopicPgm topicPgm = new TopicPgm();
           
            while (true)
            {
                topicPgm.PgmPub(DateTime.Now.ToString());
            }

        }
        static void RecPgm()
        {
            TopicPgm topicPgm = new TopicPgm();
         
            Task.Run(() => {
                while (true)
                {
                     topicPgm.PgmSub();
                }
            });


        }

        public static void PgmClient()
        {

            using (var requester = new ZSocket(ZSocketType.SUB))
            {
                // Connect192.168.0.158
                requester.Connect("epgm://192.168.0.158;239.192.1.1:5555");
                requester.SubscribeAll();
                string requestText = "Hello";
                Console.Write("Sending {0}...", requestText);
                // requester.Connect("tcp://127.0.0.1:6666");
                while (true)
                {


                    // Send
                    // requester.Send(new ZFrame(requestText));

                    // Receive
                    using (ZFrame reply = requester.ReceiveFrame())
                    {
                        Console.WriteLine(" Received: {0} {1}!", requestText, reply.ReadString());
                    }
                }
            }
        }
        public static void PgmServwr()
        {

            using (var requester = new ZSocket(ZSocketType.PUB))
            {
                // Connect
                requester.Bind("epgm://192.168.0.158;239.192.1.1:5555");
                // requester.Connect("tcp://127.0.0.1:6666");
                while (true)
                {
                    string requestText = "rrr";
                    Console.Write("Sending {0}...", requestText);
                    using (var message = new ZMessage())
                    {

                        message.Add(new ZFrame(string.Format("B {0}", "ss")));
                        message.Add(new ZFrame(string.Format(" We do like to see this.")));
                        Thread.Sleep(1000);


                        requester.Send(message);
                    }
                    // Send
                    //  requester.Send(new ZFrame(requestText));
                    Thread.Sleep(1000);
                    // Receive
                    //using (ZFrame reply = requester.ReceiveFrame())
                    //{
                    //    Console.WriteLine(" Received: {0} {1}!", requestText, reply.ReadString());
                    //}
                }
            }
        }


    }
}
