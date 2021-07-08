using MiniMsg;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ZeroMQ;

namespace ConsoleApp1
{
    class Program
    {
        //static string ipcdir="";

        static void Main(string[] args)
        {
            //ipcdir = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            //ipcdir = Path.Combine(ipcdir, "minimsg.ipc");
            // Test();
             // LocalNode.LocalAddress = "192.168.0.152";
             // LocalNode.LocalPort = 6667;
           
            Console.WriteLine(System.Environment.Is64BitProcess);
           // IpcNativeMethods.MapChannel("jinyu");
            Task.Run(() => {
                //  SendPgm();
                //  PgmClient();
                // Thread.Sleep(5000);
                try
                {
                    Sub();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }

                // ZmqIpcTcpRec();
                // IpcSendNative();
             //   IpcSendNameNative();
            });
            Task.Run(() => {
                // IpcRecNative();
                // IpcRecNameNative();
                // ZmqIpcSub();
                // RecPgm();


                // ZmqIpcSend();
                // PgmServer();
                // ZmqIpcTcpSend();
                try
                {

                    Pub();
                   
                }
                catch (Exception ex)
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

        private static void ZmqPgm_ReceiveTopic(string arg1, byte[] arg2)
        {
            throw new NotImplementedException();
        }

        private static void Sub1_ReceiveTopic(string obj)
        {
            Console.WriteLine("static接收："+obj);
        }

        private static void Sub_ReceiveTopic(string obj)
        {
            Console.WriteLine(obj);
        }


        #region 通信测试

        static void NNgSend()
        {
            while(true)
            {
                DataNative dataNative = new DataNative();
                dataNative.Send("tcp://127.0.0.1:6667", System.Text.UTF8Encoding.UTF8.GetBytes(DateTime.Now.ToString()));
                Thread.Sleep(1000);
            }
          
        }
        static void Rec()
        {
            DataNative dataNative = new DataNative();
            dataNative.Receive("tcp://127.0.0.1:6667");
            Task.Run(() => {
                while (true)
                {
                    var ss = dataNative.GetData();
                    Console.WriteLine(UTF8Encoding.UTF8.GetString(ss)+"_1");
                }
            });
        }
        static void NNgRec()
        {
            for (int i = 0; i < 3; i++)
            {
                DataNative dataNative = new DataNative();
                dataNative.Receive("tcp://*:6667");
                int num = i;
                Task.Run(() =>
                {
                    int count = 0;
                     while (true)
                    {
                        var ss = dataNative.GetData();
                        Console.WriteLine("{0}线程,{1}",num,UTF8Encoding.UTF8.GetString(ss));
                        count++;
                        if(count>10)
                        {
                            break;
                        }
                       
                    }
                    //try
                    //{
                    //    dataNative.Close();
                    //}
                    //catch(Exception ex)
                    //{
                    //    Console.WriteLine(ex);
                    //}
                    Console.WriteLine("退出{0}", num);
                });
            }
        }

        static void SendPgm()
        {
            TopicZmqPgm zmqPgm = new TopicZmqPgm();
            zmqPgm.LocalAddres = new System.Collections.Generic.List<string>();
            zmqPgm.LocalAddres.Add("192.168.0.170");
            while (true)
            {
                var buf = Encoding.Default.GetBytes(DateTime.Now.ToString());
                zmqPgm.Publish("pgm", buf);
               // Console.WriteLine("发送");
                Thread.Sleep(1000);
            }

        }
        static void RecPgm()
        {
            TopicZmqPgm zmqPgm = new TopicZmqPgm();
            zmqPgm.LocalAddres = new List<string>();
            zmqPgm.LocalAddres.Add("192.168.0.170");
            zmqPgm.ReceiveTopic += ZmqPgm_ReceiveTopic1;
            zmqPgm.Subscribe("pgma");
            //Task.Run(() => {
            //    while (true)
            //    {
            //         topicPgm.PgmSub();
            //    }
            //});


        }

        private static void ZmqPgm_ReceiveTopic1(string arg1, byte[] arg2)
        {
            Console.WriteLine("主题:{0},内容:{1}", arg1, Encoding.Default.GetString(arg2));
        }

        public static void PgmClient()
        {

            using (var requester = new ZSocket(ZSocketType.SUB))
            {
                // Connect192.168.0.158
                requester.Bind("epgm://192.168.0.126;239.192.1.1:5555");
             //   requester.SetOption(ZSocketOption.)
                requester.Subscribe("bb");
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
               // requester.Bind("epgm://192.168.0.126;239.192.1.1:5555");
                requester.Connect("epgm://192.168.0.126;239.192.1.1:5555");
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
                    requester.SetOption(ZSocketOption.LINGER, -1);
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
      
        public static void ZmqIpcRec()
        {
            try
            {
                Random random = new Random();
                string ss = random.Next().ToString() + "_";
                Console.WriteLine("pre:" + ss);
                using (var requester = new ZSocket(ZSocketType.ROUTER))
                {
                 
                    requester.Bind("ipc://tmp.ipc");

                    while (true)
                    {
                        var tsp = requester.ReceiveMessage();
                        Console.WriteLine(tsp[1]);
                        requester.SendMore(tsp[0]);
                        requester.SendMore(new ZFrame());
                        requester.Send(new ZFrame(ss + DateTime.Now.ToString()));
                        Thread.Sleep(3000);

                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
        public static void ZmqIpcSend()
        {
            try
            {
                Random random = new Random();
                string ss = random.Next().ToString() + "_";
              
                
                    using (var requester = new ZSocket(ZSocketType.DEALER))
                     {
                    requester.IdentityString = "PEER";
                    requester.Connect("ipc://tmp.ipc");
                  
                    while (true)
                    {
                        requester.SendMore(new ZFrame(requester.Identity));
                        requester.SendMore(new ZFrame());
                        requester.Send(new ZFrame(requester.IdentityString));
                        using (ZMessage msg = requester.ReceiveMessage())
                        {
                          
                            Console.WriteLine(msg[1].ReadString());
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        //public static void ZmqIpcSub()
        //{
        //    try
        //    {
        //        TopicZmqIpc ipc = new TopicZmqIpc();
        //        ipc.ReceiveTopic += Ipc_ReceiveTopic;
        //        ipc.ZmqIpcSub("");
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine(ex);
        //    }
        //}

        private static void Ipc_ReceiveTopic(string arg1, byte[] arg2)
        {
            Console.WriteLine("主题:{0},内容:{1}", arg1, Encoding.Default.GetString(arg2));
        }

       
        //public static void ZmqIpcPub()
        //{
        //    TopicZmqIpc ipc = new TopicZmqIpc();
        //    Random random = new Random();
        //    string pre = random.Next().ToString() + "_";
        //    Console.WriteLine(pre);
        //    while (true)
        //    {
        //        ipc.ZmqIpcPub("ipc",Encoding.Default.GetBytes(pre+DateTime.Now.ToString()));
        //        Thread.Sleep(1000);
        //    }
        //}

        //public static void ZmqIpcTcpRec()
        //{

        //    for (int i = 0; i < 3; i++)
        //    {
        //        int num = i;
        //        Task.Run(() =>
        //        {
        //            TopicZmqIpcTcp ipcTcp = new TopicZmqIpcTcp();
        //            ipcTcp.Rec(num);
        //        });
        //    }
        //}
        //public static void ZmqIpcTcpSend()
        //{
        //    TopicZmqIpcTcp ipcTcp = new TopicZmqIpcTcp();
        //    while(true)
        //    {
        //        ipcTcp.Send(DateTime.Now.ToString());
        //        Thread.Sleep(1000);
        //    }
        //}

        public static  void IpcSendNative()
        {
            Random random = new Random();
            string pre = random.Next().ToString()+"_年月";

            while (true)
            {

                var buf = Encoding.Default.GetBytes(pre + DateTime.Now.ToString());
                IpcNativeMethods.Send(buf,buf.Length);
            
                Thread.Sleep(6000);
            }
        }

        public static void IpcRecNative()
        {
           

            while (true)
            {

                int num = 0;
               var buf =IpcNativeMethods.Rec(ref num);
                byte[] dst = new byte[num];
                Marshal.Copy(buf, dst, 0, num);
                Console.WriteLine(Encoding.Default.GetString(dst));
            }
        }

        public static void IpcSendNameNative()
        {
            Random random = new Random();
            string pre = random.Next().ToString() + "靳雨";

            while (true)
            {

                var buf = Encoding.Default.GetBytes(pre + DateTime.Now.ToString());
                IpcNativeMethods.SendName("jinyu",buf, buf.Length);

                Thread.Sleep(6000);
            }
        }

        public static void IpcRecNameNative()
        {


            while (true)
            {

                int num = 0;
                var buf = IpcNativeMethods.RecName("jinyu",ref num);
                byte[] dst = new byte[num];
                Marshal.Copy(buf, dst, 0, num);
                Console.WriteLine(Encoding.Default.GetString(dst));
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
            miniMsgTopic.Subscribe("rrtop");
            miniMsgTopic.OnCall += MiniMsgTopic_OnCall;

        }

        public static void Pub()
        {
            MiniMsgTopic miniMsgTopic = new MiniMsgTopic();
            string tmp = "";
            while (true)
            {
                 tmp ="rr，"+ DateTime.Now.ToString();
                miniMsgTopic.Publish("maintop", Encoding.UTF8.GetBytes(tmp));
                //Console.WriteLine(tmp);
                Thread.Sleep(2000);
            }
        }
        private static void MiniMsgTopic_OnCall(string arg1, byte[] arg2)
        {
            Console.WriteLine("主题：{0} ,内容:{1}", arg1, Encoding.UTF8.GetString(arg2));
        }

       static ulong msgid = 0;
       protected static void exexam()
        {
            var bus=  BusFactory.Create(BusType.Ipc);
              bus.Subscribe("AA");
            bus.OnCall += Bus_OnCall;
            bus.Publish("AA", new byte[] { 34 });

            var ptp = PtpFactory.Create();
            ptp.Address = "127.0.0.1";
            ptp.Port = 6667;
            ptp.Start();
            ptp.Send(new byte[] { 45 });

            var rpc = BusFactory.Create(BusType.tcp);
            LocalNode.IsMsgReturn = true;//启用消息反馈
          //  rpc.Subscribe("AA");
           // rpc.OnCall += Bus_OnCall;
            msgid= rpc.Publish("AA", new byte[] { 34 });
            MsgTopicCount.Instance.OnCall += Instance_OnCall;

        }

        private static void Instance_OnCall(PubRecords obj)
        {
            if(obj.MsgId==msgid)
            {
                if(obj.SucessNum>0)
                {
                    //
                }
                else
                {
                    //失败
                }
            }
        }

        private static void Bus_OnCall(string arg1, byte[] arg2)
        {
            throw new NotImplementedException();
        }
    }
}
