using System.Collections.Generic;

namespace MiniMsg
{

    /// <summary>
    /// 记录发送的数据,处理异常
    /// </summary>
    internal class Records
    {
        /// <summary>
        /// 地址+数据
        /// </summary>
        public Dictionary<string,byte[]> Record { get; set; } 

        /// <summary>
        /// 主题
        /// </summary>
        public string Topic { get; set; }

        /// <summary>
        /// 消息ID
        /// </summary>
        public ulong MsgId { get; set; }
    }
}
