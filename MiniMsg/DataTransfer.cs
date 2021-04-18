using System.Buffers;

namespace MiniMsg
{

    /// <summary>
    /// 数据传输nng调用
    /// </summary>
    internal class DataTransfer
    {
        static ArrayPool<byte> pool = ArrayPool<byte>.Shared;

        /// <summary>
        /// 发送数据
        /// </summary>
        /// <param name="topic">主题</param>
        /// <param name="bytes">数据</param>
        /// <param name="address">地址</param>
        /// <param name="flage">数据类型0是数据1是订阅</param>
        /// <param name="msgid"></param>
        public static byte[] Send(string topic, byte[] bytes, string address, byte flage = 0, ulong msgid = 0)
        {
            var v = Util.Convert(topic, bytes, flage, msgid);
            //
            DataNative native = new DataNative();
            var ret = native.Send(address, v);
            pool.Return(v);
            return ret;
        }

       

    }
}
