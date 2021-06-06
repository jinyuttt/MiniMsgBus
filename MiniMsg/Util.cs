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
        static readonly byte[]guid = Encoding.UTF8.GetBytes(LocalNode.GUID);
       
        /// <summary>
        /// 标记（1）+guid+msgid+主题长度（2）+主题（m）+数据
        /// </summary>
        /// <param name="topic"></param>
        /// <param name="bytes"></param>
        /// <param name="flage"></param>
        /// <returns></returns>
        public static byte[] Convert(string topic, byte[] bytes, byte flage,ulong msgid)
        {
            byte[] tp = Encoding.UTF8.GetBytes(topic.Trim());
            byte[] len = BitConverter.GetBytes((short)tp.Length);
            var buf = new byte[1 + 32 + 8 + 2 + tp.Length + bytes.Length];
            //var v = pool.Rent(1+32+8+2+tp.Length + bytes.Length);
            using(MemoryStream  mem=new MemoryStream(buf))
            {
                mem.WriteByte(flage);
                mem.Write(guid);
                mem.Write(BitConverter.GetBytes(msgid));
                mem.Write(len);
                mem.Write(tp);
                mem.Write(bytes);
            }
            return buf;
        }

        /// <summary>
        /// 标记（1）+guid+msgid+主题长度（2）+主题（m）+数据
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public static TopicStruct Convert(byte[] bytes)
        {
                byte flage = bytes[0];
                //数据类型
                MemoryStream memory = new MemoryStream(bytes);
                memory.ReadByte();

               //节点guid
                var id = pool.Rent(32);
                memory.Read(id, 0, 32);
                var guid = Encoding.UTF8.GetString(id).Trim();
                pool.Return(id);
                //消息ID
                var msgno = pool.Rent(8);
                memory.Read(msgno, 0, 8);
                var msgid = BitConverter.ToUInt64(msgno);
                 pool.Return(msgno);
                //主题长度
                var len = pool.Rent(2);
                memory.Read(len, 0, 2);
                //
                short curLen = BitConverter.ToInt16(len);
                pool.Return(len);
               
                //主题
                var topic = pool.Rent(curLen);
                memory.Read(topic, 0, curLen);
                string strTopic = Encoding.UTF8.GetString(topic,0,curLen).Trim();
                pool.Return(topic);
         
            //数据
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
