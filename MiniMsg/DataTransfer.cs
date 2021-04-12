using System.Buffers;

namespace MiniMsg
{

    /// <summary>
    /// 数据传输nng调用
    /// </summary>
    public class DataTransfer
    {
        static ArrayPool<byte> pool = ArrayPool<byte>.Shared;
     
        public void Send(string topic,byte[] bytes,string address,byte flage=0)
        {
           var v= Util.Convert(topic, bytes, flage);
            //
            DataNative native = new DataNative();
            native.Send(address, v);
            pool.Return(v);
        }

        public void Rec(string address)
        {
            //接收数据
            DataNative native = new DataNative();
             native.Receive(address);
            while (true)
            {
                var buf = native.GetData();
                var v = Util.Convert(buf);
                if (v.Flage == 0)
                {
                    //数据
                    SubMgr.Instance.Add(v);
                }
            }
        }

    }
}
