using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniMsg
{

    /// <summary>
    /// 发布信息记录
    /// </summary>
    public class PubRecords
    {

        /// <summary>
        /// 消息ID
        /// </summary>
        public ulong MsgId { get; set; }

        /// <summary>
        /// 成功次数
        /// </summary>
        public int SucessNum { get; set; }

        /// <summary>
        /// 失败次数
        /// </summary>
        public int FaildNum { get; set; }
    }
}
