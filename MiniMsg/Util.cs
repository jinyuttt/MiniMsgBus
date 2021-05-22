using System;
using System.Buffers;
using System.IO;
using System.Text;

namespace MiniMsg
{

    /// <summary>
    /// 工具
    /// </summary>
    public class Util
    {
        static readonly ArrayPool<byte> pool = ArrayPool<byte>.Shared;
        static byte[]guid = Encoding.UTF8.GetBytes(LocalNode.GUID);

        /// <summary>
        /// 标记（1）+guid+msgid+主题长度（2）+主题（m）+数据
        /// </summary>
        /// <param name="topic"></param>
        /// <param name="bytes"></param>
        /// <param name="flage"></param>
        /// <returns></returns>
        public static byte[] Convert(string topic, byte[] bytes, byte flage,ulong msgid)
        {
            byte[] tp = Encoding.UTF8.GetBytes(topic);
            byte[] len = BitConverter.GetBytes((short)tp.Length);
            var v = pool.Rent(1+32+8+2+tp.Length + bytes.Length);
            using(MemoryStream  mem=new MemoryStream(v))
            {
                mem.WriteByte(flage);
                mem.Write(guid);
                mem.Write(BitConverter.GetBytes(msgid));
                mem.Write(len);
                mem.Write(tp);
                mem.Write(bytes);
            }
            return v;
        }

        /// <summary>
        /// 标记（1）+guid+msgid+主题长度（2）+主题（m）+数据
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public static TopicStruct Convert(byte[] bytes)
        {
                byte flage = bytes[0];
                //
                MemoryStream memory = new MemoryStream(bytes);
                memory.ReadByte();
                var id = pool.Rent(32);
                memory.Read(id, 0, 32);
                var guid = Encoding.UTF8.GetString(id);
                   pool.Return(id);
                //
                var msgno = pool.Rent(8);
                memory.Read(msgno, 0, 8);
                var msgid = BitConverter.ToUInt64(msgno);
                 pool.Return(msgno);
                //

                var len = pool.Rent(2);
                memory.Read(len, 0, 2);
                //
                short curLen = BitConverter.ToInt16(len, 0);
                pool.Return(len);
                var topic = pool.Rent(curLen);
                memory.Read(topic, 0, curLen);
                string strTopic = Encoding.UTF8.GetString(topic);
                pool.Return(topic);
                byte[] buffer = new byte[bytes.Length - curLen - 43];
                memory.Read(buffer, 0, buffer.Length);

                return new TopicStruct() { Flage = flage, Msg = buffer, Topic = strTopic, MsgId = msgid, MsgNode=guid };
           
        }
    }

    public class TopicStruct
    {

        /// <summary>
        /// 消息ID
        /// </summary>
        public ulong MsgId { get; set; }

        /// <summary>
        /// 消息节点标识
        /// </summary>
        public string MsgNode { get; set; }

        /// <summary>
        /// 主题
        /// </summary>
        public  string Topic { get; set; }

        /// <summary>
        /// 消息标识0是消息1是订阅
        /// </summary>
        public  byte Flage { get; set; }

        /// <summary>
        /// 消息内容
        /// </summary>
        public byte[] Msg { get; set; }
           
    }
}
