using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniMsg
{

   /// <summary>
   /// 记录发送的数据
   /// </summary>
   public class Records
    {
        public Dictionary<string,byte[]> Record { get; set; } 

        public string Topic { get; set; }
    }
}
