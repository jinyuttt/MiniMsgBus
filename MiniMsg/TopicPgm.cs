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
        public string LocalAddress { get; set; } =LocalNode.LocalAddress;

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
            using (var requester = new ZSocket(ZSocketType.SUB))
            {

                if (LocalNode.LocalAddress.Contains("*"))
                {
                    //绑定本地所有IP
                    foreach (var p in LocalNode.LocalAddressFamily)
                    {
                        string addr = "epgm://" + p.IPV4 + ";" + MultAddress;
                        requester.Bind(addr);
                        
                    }

                }
                else
                {
                    string tmp = LocalNode.LocalAddress.Substring(6);
                    int index = tmp.IndexOf(':');
                    string addr = "epgm://" + tmp.Substring(0, index + 1) + ";" + MultAddress;
                    requester.Bind(addr);
                   
                }
                while (true)
                {
                    requester.SubscribeAll();
                    using (ZFrame reply = requester.ReceiveFrame())
                    {
                        Console.WriteLine(" Received: {1}!", reply.ReadString());
                        if (ReceiveTopic != null)
                        {
                            ReceiveTopic(reply.ReadString());
                        }
                    }
                }

            }
          
        }

        /// <summary>
        /// 通知主题发布地址
        /// </summary>
        /// <param name="topic"></param>
        public void PgmPub(string topic)
        {
            using (var requester = new ZSocket(ZSocketType.PUB))
            {
             
                //如果绑定了所有网卡接收
                if (LocalNode.LocalAddress.Contains("*"))
                {
                    //绑定本地所有IP
                    foreach(var p in LocalNode.LocalAddressFamily)
                    {
                        try
                        {
                            //当前只考虑IPV4
                            if (!string.IsNullOrEmpty(p.IPV4))
                            {
                                //采用epgm协议发送数据
                                string addr = "epgm://" + p.IPV4 + ";" + MultAddress;
                                requester.Bind(addr);
                                using (var message = new ZMessage())
                                {

                                    message.Add(new ZFrame(string.Format("epgmpub {0}", "ss")));//这里定一个主题
                                    message.Add(new ZFrame(string.Format(topic + "|" + p.IPV4)));//主题数据，只使用数据
                                    requester.Send(message);
                                }
                               
                            }
                        }
                        catch(Exception ex)
                        {
                            Console.WriteLine(ex);
                        }
                    }
                   
                }
                else
                {
                    try
                    {
                        string tmp = LocalNode.LocalAddress.Substring(6);
                        int index = tmp.IndexOf(':');
                        string addr = "epgm://" + tmp.Substring(0, index + 1) + ";" + MultAddress;
                        requester.Bind(addr);
                        using (var message = new ZMessage())
                        {

                            message.Add(new ZFrame(string.Format("epgmpub {0}", "ss")));//这里定一个主题
                            message.Add(new ZFrame(string.Format(topic + "|" + tmp)));//主题数据，只使用数据
                            requester.Send(message);
                        }
                    
                    }
                    catch(Exception ex)
                    {
                        Console.WriteLine(ex);
                    }
                }
               
              
            }
        }

        /// <summary>
        /// 组播发送主题和地址
        /// </summary>
        /// <param name="topic">主题</param>
        /// <param name="address">发布节点地址</param>
        public void PgmUpdate(string topic,string address)
        {
            using (var requester = new ZSocket(ZSocketType.PUB))
            {
                
                requester.Bind(Address);
                requester.Send(new ZFrame(topic + "|" + address));
                using (var message = new ZMessage())
                {

                    message.Add(new ZFrame(string.Format("epgmpub {0}", "ss")));//这里定一个主题
                    message.Add(new ZFrame(string.Format(topic + "|" + address)));//主题数据，只使用数据
                    requester.Send(message);
                }

            }
        }
    
       
    }
}
