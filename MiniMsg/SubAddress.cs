using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniMsg
{
    /// <summary>
    /// 订阅节点地址
    /// </summary>
   public class SubAddress
    {
        /// <summary>
        /// 使用地址
        /// </summary>
        public string Address { get; set; }

        /// <summary>
        /// 所有地址
        /// </summary>
        public  List<string> AllAddress { get; set; }

       
        /// <summary>
        /// 认为异常的地址
        /// </summary>
        public List<string> ErrorAddress { get; set; }

     
        /// <summary>
        /// 节点标识guid
        /// </summary>
       public string NodeFlage { get; set; }

        /// <summary>
        /// 次数
        /// </summary>
       public int NumAll { get; set; }
    }

    /// <summary>
    /// 主题订阅地址
    /// </summary>
    public class SubAddressLst
    {
        /// <summary>
        /// 所有节点地址
        /// </summary>
        public List<SubAddress> SubAddresses { get; set; }

        /// <summary>
        /// 控制地址加入
        /// </summary>
        public List<string> LstAddress { get; set; }
    }
}
