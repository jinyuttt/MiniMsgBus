using System;
using System.Buffers;
using System.IO;

namespace MiniMsg
{

    /// <summary>
    /// 工具
    /// </summary>
    public class Util
    {
        static readonly ArrayPool<byte> pool = ArrayPool<byte>.Shared;
        /// <summary>
        /// 标记（1）+主题长度（2）+主题（m）+数据
        /// </summary>
        /// <param name="topic"></param>
        /// <param name="bytes"></param>
        /// <param name="flage"></param>
        /// <returns></returns>
        public static byte[] Convert(string topic, byte[] bytes, byte flage)
        {
            byte[] tp = System.Text.UTF8Encoding.UTF8.GetBytes(topic);
            byte[] len = BitConverter.GetBytes((short)tp.Length);
            var v = pool.Rent(1 + tp.Length + 2 + bytes.Length);

            Array.Copy(len, 0, v, 1, 2);
            Array.Copy(tp, 0, v, 3, tp.Length);
            Array.Copy(bytes, 0, v, 3 + tp.Length, 2);

            v[0] = flage;


            return v;
        }

        public static TopicStruct Convert(byte[] bytes)
        {
            byte flage = bytes[0];
            MemoryStream memory = new MemoryStream(bytes);
            memory.ReadByte();
            var len = pool.Rent(2);
            memory.Read(len, 0, 2);
            //
            short curLen = BitConverter.ToInt16(len, 0);
            pool.Return(len);
            var topic = pool.Rent(curLen);
            memory.Read(topic, 0, curLen);
            string strTopic = System.Text.UTF8Encoding.UTF8.GetString(topic);
            pool.Return(topic);
            byte[] buffer = new byte[bytes.Length - curLen - 3];
            memory.Read(buffer, 0, buffer.Length);

            return new TopicStruct() { Flage = flage, msg = buffer, Topic = strTopic };
        }
    }

    public class TopicStruct
    {
        public  string Topic { get; set; }

        public  byte Flage { get; set; }

        public byte[] msg { get; set; }
           
    }
}
