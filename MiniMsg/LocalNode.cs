using System;
using System.Collections.Concurrent;
using System.Net.NetworkInformation;
using System.Collections.Generic;

namespace MiniMsg
{
    /// <summary>
    /// 节点信息
    /// </summary>
    public class LocalNode
    {
        /// <summary>
        /// 本地节点地址
        /// </summary>
        public static string LocalAddress = "*";

        /// <summary>
        /// 本地端口
        /// </summary>
        public static int LocalPort = 0;

        /// <summary>
        /// 网络地址
        /// </summary>
        public static string LocalNetAddress = "";

        /// <summary>
        /// 掩码
        /// </summary>
        public static string LocalMask = "";

        /// <summary>
        /// 网关
        /// </summary>
        public static string LocalGateway = "";

        /// <summary>
        /// 默认通信协议
        /// </summary>
        public static string Netprotocol = "nng";

        public static string protocol = "tcp";

        /// <summary>
        /// 节点唯一标识
        /// </summary>
        public static readonly string GUID = Guid.NewGuid().ToString("N");

        public static readonly List<NetWorkInfo> LocalAddressFamily=new List<NetWorkInfo>();
       
      
        /// <summary>
        /// 本地节点订阅信息（没有发布地址时的临时存放）
        /// </summary>
        private static readonly ConcurrentDictionary<string,object> localSub = new ConcurrentDictionary<string,object>();

        /// <summary>
        /// 本地订阅临时存放，直到有发布地址
        /// </summary>
        /// <param name="topic"></param>
        /// <param name="ov"></param>
        public static void AddLocal(string topic,object ov)
        {
            localSub[topic] = ov;
        }

        /// <summary>
        /// 获取本地临时订阅
        /// </summary>
        /// <param name="topic"></param>
        /// <returns></returns>
        public static object GetLocal(string topic)
        {
           
            object ov = null;
            foreach(var p in localSub.Keys)
            {
                if(p.CompareTo(topic)==0)
                {
                    topic = p;
                    break;
                }
            }
            localSub.TryGetValue(topic,out ov);
            return ov;
                
        }

        
        
       
        /// <summary>
        /// 获取所有IP地址
        /// </summary>
        public static void GetNetworkInterface()
        {
            try
            {
                if(LocalAddressFamily.Count > 0)
                {
                    return;
                }
                lock (LocalAddressFamily)
                {
                    if (LocalAddressFamily.Count > 0)
                    {
                        return;
                    }
                    NetworkInterface[] adapters = NetworkInterface.GetAllNetworkInterfaces();
                    foreach (NetworkInterface ni in adapters)
                    {
                        IPInterfaceProperties ipProperties = ni.GetIPProperties();

                        foreach (var curIP in ipProperties.UnicastAddresses)
                        {

                            var v = new NetWorkInfo() { IPV4 = curIP.Address.ToString(), Mask = curIP.IPv4Mask.ToString(), DnsAddress = ipProperties.DnsAddresses.ToString() };
                            if (v.IPV4.Contains(":"))
                            {
                                v.IPV6 = v.IPV4;
                                v.IPV4 = "127.0.0.1";
                            }
                            if (ipProperties.GatewayAddresses.Count > 0)
                                v.GatewayAddress = ipProperties.GatewayAddresses[0].Address.ToString();

                            LocalAddressFamily.Add(v);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

    }


    /// <summary>
    /// 网卡信息
    /// </summary>
    public class NetWorkInfo
    {
        /// <summary>
        /// IP4网络
        /// </summary>
        public string IPV4 { get; set; }

       /// <summary>
       /// IP6网络
       /// </summary>
        public string IPV6 { get; set; }

        /// <summary>
        /// 掩码
        /// </summary>
        public string Mask { get; set; }

        /// <summary>
        /// 路由地址
        /// </summary>
        public string GatewayAddress { get; set; }

        /// <summary>
        /// Dns地址
        /// </summary>
        public string DnsAddress { get; set; }
    }
}
