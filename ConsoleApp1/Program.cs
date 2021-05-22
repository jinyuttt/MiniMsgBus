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
           // Test();
            // LocalNode.LocalAddress = "192.168.0.129";
            //  LocalNode.LocalPort = 6667;
            Console.WriteLine(System.Environment.Is64BitProcess);
            Task.Run(() => {
                //  PgmClient();
                // Thread.Sleep(5000);
                try
                {
                    Sub();
                }
                catch(Exception ex)
                {
                    Console.WriteLine(ex);
                }

              //   Rec();
              //  ZmqSend();
            });
            Task.Run(() => {
                try
                {
                    //  PgmServer();
                    // Send();
                    Pub();
                    //   ZmqRec();
                }
                catch(Exception ex)
                {
                    Console.WriteLine(ex);
                }
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


        #region 通信测试

        static void Send()
        {
            while(true)
            {
                DataNative dataNative = new DataNative();
                dataNative.Send("tcp://192.168.0.108:6667", System.Text.UTF8Encoding.UTF8.GetBytes(DateTime.Now.ToString()));
                Thread.Sleep(1000);
            }
          
        }
        static void Rec()
        {
            DataNative dataNative = new DataNative();
            dataNative.Receive("tcp://*:6667");
            Task.Run(() => {
                while (true)
                {
                    var ss = dataNative.GetData();
                    Console.WriteLine(UTF8Encoding.UTF8.GetString(ss)+"_1");
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
                requester.Bind("epgm://192.168.0.110;239.192.1.1:5555");
                requester.Subscribe("aa");
                //string requestText = "Hello";
                //Console.Write("Sending {0}...", requestText);
                // requester.Connect("tcp://127.0.0.1:6666");
                while (true)
                {


                    // Send
                    // requester.Send(new ZFrame(requestText));

                    // Receive
                    using (ZFrame reply = requester.ReceiveFrame())
                    {
                        Console.WriteLine(" Received: {0} !", reply.ReadString());
                    }
                }
            }
        }
        public static void PgmServer()
        {

            using (var requester = new ZSocket(ZSocketType.PUB))
            {
                // Connect
                requester.Bind("epgm://192.168.0.110;239.192.1.1:5555");
                // requester.Connect("tcp://127.0.0.1:6666");
                while (true)
                {
                    //string requestText = "rrr";
                    //Console.Write("Sending {0}...", requestText);
                    using (var message = new ZMessage())
                    {

                        message.Add(new ZFrame(string.Format("bb {0}", "mm")));
                        message.Add(new ZFrame(string.Format(" we are 110.")));
                        Thread.Sleep(3000);


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

        public static void ZmqSend()
        {
            try
            {
                using (var requester = new ZSocket(ZSocketType.REQ))
                {
                    requester.Connect("tcp://127.0.0.1:8080");
                    requester.SendFrame(new ZFrame("sss"));
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        public static void ZmqRec()
        {
            try
            {
                using (var requester = new ZSocket(ZSocketType.REP))
                {
                    requester.Bind("tcp://127.0.0.1:8080");
                   
                    while(true)
                    {
                      var tsp=  requester.ReceiveFrame();
                        
                        Console.WriteLine(tsp.ReadString());
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
        #endregion


        private static void Test()
        {
            string str = "jinyu1";
            string str1 = "jinyu1";
            if(str==str1)
            {
                Console.WriteLine("真");
            }
            else if(str.CompareTo(str)==0)
            {
                Console.WriteLine("比较真");
            }
            else  if(str.Equals(str1))
            {
                Console.WriteLine("Equals真");
            }
        }

        public static void Sub()
        {
            MiniMsgTopic miniMsgTopic = new MiniMsgTopic();
            miniMsgTopic.Subscribe("leveltop");
            miniMsgTopic.OnCall += MiniMsgTopic_OnCall;

        }

        public static void Pub()
        {
            MiniMsgTopic miniMsgTopic = new MiniMsgTopic();
            string tmp = "";
            while (true)
            {
                 tmp ="Mai，"+ DateTime.Now.ToString();
                miniMsgTopic.Publish("maintop", Encoding.UTF8.GetBytes(tmp));
                //Console.WriteLine(tmp);
                Thread.Sleep(1000);
            }
        }
        private static void MiniMsgTopic_OnCall(string arg1, byte[] arg2)
        {
            Console.WriteLine("主题：{0} ,内容:{1}", arg1, Encoding.UTF8.GetString(arg2));
        }
    }
}
