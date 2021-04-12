using System;
using System.Net;
using System.Net.Sockets;
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
        public static string LocalAddress = "";

        public static string LocalMask = "";

        public static string LocalGateway = "";
        private static List<NetWorkInfo> LocalAddressFamily=new List<NetWorkInfo>();
       
        private static readonly ConcurrentDictionary<string, string> dicLocal = new ConcurrentDictionary<string, string>();
        private static readonly ConcurrentDictionary<string,object> localSub = new ConcurrentDictionary<string,object>();

        public static void AddLocal(string topic,object ov)
        {
            localSub[topic] = ov;
        }
        public static object GetLocal(string topic)
        {
            object ov = null;
            localSub.TryGetValue(topic,out ov);
            return ov;
                
        }

        /// <summary>
        /// 添加订阅的地址
        /// </summary>
        /// <param name="topic"></param>
        public static void Add(string topic)
        {
            dicLocal[topic] = null;
        }

        /// <summary>
        /// 是否存在
        /// </summary>
        /// <param name="topic"></param>
        /// <returns></returns>
        public static  bool IsSub(string topic)
        {
            return dicLocal.ContainsKey(topic);
        }

        /// <summary>
        /// 获取本地IP
        /// </summary>
        /// <returns></returns>
        public static string GetLocalAddress()
        {
            if(string.IsNullOrWhiteSpace(LocalAddress))
            {
                LocalAddress = GetLocalIP();
            }
            return LocalAddress;
        }
        public static string GetLocalIP()
        {
            try
            {

                string HostName = Dns.GetHostName(); //得到主机名
                IPHostEntry IpEntry = Dns.GetHostEntry(HostName);
                for (int i = 0; i < IpEntry.AddressList.Length; i++)
                {
                    //从IP地址列表中筛选出IPv4类型的IP地址
                    //AddressFamily.InterNetwork表示此IP为IPv4,
                    //AddressFamily.InterNetworkV6表示此地址为IPv6类型
                    if (IpEntry.AddressList[i].AddressFamily == AddressFamily.InterNetwork)
                    {
                        string ip = "";
                        ip = IpEntry.AddressList[i].ToString();
                        
                        return IpEntry.AddressList[i].ToString();
                    }
                }
                return "";
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }
        public static void GetNetworkInterface()
        {
            try
            {

                NetworkInterface[] adapters = NetworkInterface.GetAllNetworkInterfaces();
                foreach (NetworkInterface ni in adapters)
                {
                    IPInterfaceProperties ipProperties = ni.GetIPProperties();

                    foreach(var curIP in ipProperties.UnicastAddresses)
                    {
                        
                        var v = new NetWorkInfo() { IPV4 = curIP.Address.ToString(), Mask = curIP.IPv4Mask.ToString(), GatewayAddress = ipProperties.GatewayAddresses[0].Address.ToString() };
                        LocalAddressFamily.Add(v);
                    }


                }
            }
            catch (Exception ex)
            {
              
            }
        }

    }


    /// <summary>
    /// 网卡信息
    /// </summary>
    public class NetWorkInfo
    {
        public string IPV4 { get; set; }

        public string IPV6 { get; set; }

        public string Mask { get; set; }

        public string GatewayAddress { get; set; }
    }
}
