using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace MiniMsg
{
    internal class TopicBroadcast
    {
          TopicIpcNative ipc = new TopicIpcNative();
          TopicZmqPgm pgm = new TopicZmqPgm() {  LocalAddres=new List<string>()};
          public static List<string> lstNodeAddress = new List<string>();
          public event Action<string,string> ReceiveTopic;
          static List<string> lstBindIP = new List<string>();


        static TopicBroadcast()
        {
            InitAddress();
          
        }

        /// <summary>
        /// 初始化本地地址
        /// </summary>
        static void InitAddress()
        {
            if (LocalNode.LocalAddress.Contains("*"))
            {
                //绑定本地所有IP
                LocalNode.GetNetworkInterface();

                foreach (var p in LocalNode.LocalAddressFamily)
                {
                    if (p.IPV4 == "127.0.0.1" || p.IPV4.Contains("169.254"))
                    {
                        continue;
                    }
                    lstBindIP.Add(p.IPV4);
                }
            }
            else
            {
                lstBindIP.Add(LocalNode.LocalAddress);
            }
        }

        /// <summary>
        /// 获取地址
        /// </summary>
        public  void GetLocalAddress()
        {

            foreach (var p in lstBindIP)
            {
                try
                {
                    string tmp = "";
                    if (string.IsNullOrEmpty(LocalNode.protocol))
                    {
                        tmp = string.Format("{0}:{1}", p, LocalNode.LocalPort);
                    }
                    else
                    {
                        tmp = string.Format("{0}://{1}:{2}", LocalNode.protocol, p, LocalNode.LocalPort);
                    }
                    if (!lstNodeAddress.Contains(tmp))
                    {
                        lstNodeAddress.Add(tmp);
                    }
                    
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
            }
        }


        /// <summary>
        /// 处理接收的数据
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        private (string,string) GetTopicData(string obj)
        {
            Console.WriteLine("recvice  {0}", obj);
            
            try
            {
                string[] tmp = obj.Split('|');//主题与发布地址用|分割
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < tmp.Length - 1; i++)
                {
                    //防止主题中有|
                    builder.AppendFormat("{0}|", tmp[i]);
                }
                builder.Remove(builder.Length - 1, 1);
                string topic = builder.ToString();
                string address = tmp[tmp.Length - 1];
                return (topic, address);
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex);
                
            }
            return ("", "");
        }

        /// <summary>
        /// 订阅数据
        /// </summary>
        public void TopicSub()
        {
            //
            if (pgm.LocalAddres.Count == 0)
            {
                pgm.LocalAddres = lstBindIP;
            }
            pgm.ReceiveTopic += Pgm_ReceiveTopic;
            
            pgm.Subscribe("noticetopicaddress");
            ipc.ReceiveTopic += Ipc_ReceiveTopic;
            //ipc.ZmqIpcSub("minimsg");
            ipc.IpcRecv();

        }

        private void Ipc_ReceiveTopic(string arg1, byte[] arg2)
        {
            Console.WriteLine("Ipc接收");
            var data = Encoding.UTF8.GetString(arg2);
            var items = GetTopicData(data);
            int len = items.Item2.Length;
            int flageLen = LocalNode.GUID.Length;
            var guid = items.Item2.Substring(len - flageLen);
            if (guid.CompareTo(LocalNode.GUID) == 0)
            {
                return;//自己的不要
            }
           
            if (items.Item1 == "Global")
            {
                Broadcast(false);//ipc接收用ipc回复
                return;
            }
            var msg = items.Item2.Substring(0, items.Item2.Length - LocalNode.GUID.Length-2);
            Console.WriteLine("ipc消息;{0}", msg);
            if (ReceiveTopic != null)
            {
                ReceiveTopic(items.Item1, msg);
            }

        }

        private void Pgm_ReceiveTopic(string arg1, byte[] arg2)
        {
            Console.WriteLine("pgm接收");
            var data = Encoding.UTF8.GetString(arg2);
            var items = GetTopicData(data);
            if (items.Item1 == "Global")
            {
                Broadcast(true);//组播接收用组播回复
                return;
            }
            if (ReceiveTopic != null)
            {
                ReceiveTopic(items.Item1, items.Item2);
            }
           // ipc.IpcSend(arg1, arg2);
           // ipc.ZmqIpcPub("minimsg", arg2);

        }

        
        /// <summary>
        ///  通知主题本节点发布地址
        /// </summary>
        /// <param name="topic">发布的主题</param>
        /// <returns>本地发布地址</returns>
        public List<string> PgmPub(string topic)
        {
            List<string> lst = new List<string>();
            //如果绑定了所有网卡接收
            //绑定本地所有IP
            if (pgm.LocalAddres.Count == 0)
            {
                pgm.LocalAddres = lstBindIP;
            }
           
            foreach (var p in lstNodeAddress)
            {
                try
                {
                    //通知本节点发布主题发布地址，组播和ipc同时，组播可能被占
                    pgm.Publish("noticetopicaddress", Encoding.UTF8.GetBytes(topic + "|" + p));
                    ipc.IpcSend("minimsg", Encoding.UTF8.GetBytes(topic + "|" + p+">>"+LocalNode.GUID));//同时发给本机其它节点
                    Console.WriteLine("pgmpub:");
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }

            }
            return lstNodeAddress;
        }



        private void Broadcast(bool ispgm)
        {
           
            //如果接收到的是Global信息，则把本节点保持的所有发布节点发送出去，让新加入的节点获取
            //发布地址
            var dic = PubTable.Instance.GetPairs();
            Console.WriteLine("接收到Global主题，通知一次全局主题地址");
            foreach (var kv in dic)
            {
                foreach (var p in kv.Value)
                {
                    if (pgm.LocalAddres.Count == 0)
                    {
                        pgm.LocalAddres = lstBindIP;
                    }
                    if (ispgm)
                    {
                        pgm.Publish("noticetopicaddress", Encoding.UTF8.GetBytes(kv.Key + "|" + p));
                    }
                    else
                    {
                        ipc.IpcSend("minimsg", Encoding.UTF8.GetBytes(kv.Key + "|" + p +">>"+ LocalNode.GUID));
                    }

                }

            }

        }

    }
}
