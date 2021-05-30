using System;
using ZeroMQ;
using System.Collections.Generic;

namespace MiniMsg
{

    /// <summary>
    /// 主题传输
    /// </summary>
    public  class TopicPgm1
    {

        
    

        public string MultAddress { get; set; } = "239.192.1.1:5555";

        public List<string> lstBindPgmAddress = new List<string>();
      
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
                    //if (LocalNode.LocalAddressFamily == null || LocalNode.LocalAddressFamily.Count == 0)
                    {
                        LocalNode.GetNetworkInterface();
                    }
                    foreach (var p in LocalNode.LocalAddressFamily)
                    {
                        if (p.IPV4 == "127.0.0.1" || p.IPV4.Contains("169.254"))
                        {
                            continue;
                        }
                        string addr = "epgm://" + p.IPV4 + ";" + MultAddress;
                        requester.Bind(addr);
                         if(!lstBindPgmAddress.Contains("addr"))
                        {
                            lstBindPgmAddress.Add(addr);
                        }
                        Console.WriteLine("bindpgm:" + addr);
                    }

                }
                else
                {
                    string tmp = LocalNode.LocalAddress;
                   // Console.WriteLine("bindpgm:" + tmp);
                
                    string addr = "epgm://" + tmp + ";" + MultAddress;
                    Console.WriteLine("bindpgm:" + addr);
                    requester.Bind(addr);
                    if(!lstBindPgmAddress.Contains(addr))
                    {
                        lstBindPgmAddress.Add(addr);
                    }
                  
                }
                requester.Subscribe("noticetopicaddress");
                while (true)
                {
                    
                    using (ZFrame reply = requester.ReceiveFrame())
                    {
                        try
                        {
                            var data = reply.ReadString();
                            if(data== "noticetopicaddress")
                            {
                                //主题数据
                                continue;
                            }
                            if (ReceiveTopic != null)
                            {
                                ReceiveTopic(data);
                            }
                        }catch(Exception ex)
                        {
                            Console.WriteLine(ex);
                        }
                    }
                }

            }
          
        }

        /// <summary>
        ///  通知主题发布地址
        /// </summary>
        /// <param name="topic">发布的主题</param>
        /// <returns>本地发布地址</returns>
        public List<string> PgmPub(string topic)
        {
            List<string> lst = new List<string>();
            using (var requester = new ZSocket(ZSocketType.PUB))
            {
             
                //如果绑定了所有网卡接收
                if (LocalNode.LocalAddress.Contains("*"))
                {
                   
                    LocalNode.GetNetworkInterface();    
                    //绑定本地所有IP
                    foreach(var p in LocalNode.LocalAddressFamily)
                    {
                        try
                        {
                            //当前只考虑IPV4
                            if (!string.IsNullOrEmpty(p.IPV4))
                            {
                                //采用epgm协议发送数据
                                if(p.IPV4=="127.0.0.1"||p.IPV4.Contains("169.254"))
                                {
                                    continue;
                                }
                                try
                                {
                                    string addr = "epgm://" + p.IPV4 + ";" + MultAddress;
                                    requester.Bind(addr);
                                 //   Console.WriteLine("pgmBind:" + addr);
                                    //using(StreamWriter sw=new StreamWriter("dd.txt",true))
                                    //{
                                    //    sw.WriteLine(addr);
                                    //}
                                    if (!lstBindPgmAddress.Contains(addr))
                                    {
                                        lstBindPgmAddress.Add(addr);
                                    }

                                    using (var message = new ZMessage())
                                    {
                                        string tmp = "";
                                        if (string.IsNullOrEmpty(LocalNode.protocol))
                                        {
                                            tmp = string.Format("{0}:{1}", p.IPV4, LocalNode.LocalPort);
                                        }
                                        else
                                        {
                                            tmp = string.Format("{0}://{1}:{2}", LocalNode.protocol, p.IPV4, LocalNode.LocalPort);
                                        }

                                        message.Add(new ZFrame("noticetopicaddress"));//这里定一个主题
                                        message.Add(new ZFrame(string.Format(topic + "|" + tmp)));//主题数据，只使用数据
                                        requester.Send(message);
                                        lst.Add(tmp);
                                     //   Console.WriteLine("将本地地址加入发布列表:" + tmp);
                                        //Console.WriteLine("通知一次主题地址:" + topic);
                                    }
                                }
                                catch(Exception ex)
                                {
                                    Console.WriteLine(ex);
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
                        string tmp = LocalNode.LocalAddress;
                      
                        string addr = "epgm://" + tmp +";" + MultAddress;
                        requester.Bind(addr);

                        if (!lstBindPgmAddress.Contains(addr))
                        {
                            lstBindPgmAddress.Add(addr);
                        }


                        if (string.IsNullOrEmpty(LocalNode.protocol))
                        {
                            tmp = string.Format("{0}:{1}", LocalNode.LocalAddress, LocalNode.LocalPort);
                        }
                        else
                        {
                            tmp = string.Format("{0}://{1}:{2}", LocalNode.protocol, LocalNode.LocalAddress, LocalNode.LocalPort);
                        }
                        lst.Add(tmp);
                        using (var message = new ZMessage())
                        {

                            message.Add(new ZFrame("noticetopicaddress"));//这里定一个主题
                            message.Add(new ZFrame(string.Format(topic + "|" + tmp)));//主题数据，只使用数据
                            requester.Send(message);
                       
                        }
                        //Console.WriteLine("加入组播地址:" + addr);
                        //Console.WriteLine("绑定本地地址:" + tmp);
                        //Console.WriteLine("通知一次主题地址:" + topic);

                    }
                    catch(Exception ex)
                    {
                        Console.WriteLine(ex);
                    }
                }
               
            }
            return lst;
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
                foreach (var p in lstBindPgmAddress)
                {
                    requester.Bind(p);
                    using (var message = new ZMessage())
                    {
                        message.Add(new ZFrame("noticetopicaddress"));//这里定一个主题
                        message.Add(new ZFrame(string.Format(topic + "|" + address)));//主题数据，只使用数据
                        requester.Send(message);
                    }
                }
            }
        }
    
       
    }
}
