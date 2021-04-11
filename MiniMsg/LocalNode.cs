using System;
using System.Net;
using System.Net.Sockets;
using System.Collections.Concurrent;
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
        private static string LocalAddress = "";
        private static ConcurrentDictionary<string, string> dicLocal = new ConcurrentDictionary<string, string>();

        public void Add(string topic)
        {
            dicLocal[topic] = null;
        }
        public static  bool IsSub(string topic)
        {
            return dicLocal.ContainsKey(topic);
        }
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
    }
}
