using nng;
using System.Collections.Concurrent;
using System.IO;
using System.Threading;

namespace MiniMsg
{
    public class DataNative
    {
        static IAPIFactory<INngMsg> factory = null;
        private readonly BlockingCollection<byte[]> queue = new BlockingCollection<byte[]>();
        private static IAPIFactory<INngMsg> GetFactory()
        {
            //  FactoryExt
            if (factory == null)
            {
                var path = Path.GetDirectoryName(typeof(DataNative).Assembly.Location);
                var ctx = new NngLoadContext(path);
                factory = NngLoadContext.Init(ctx);
            }
            return factory;
        }
        public  void Send(string address,byte[]bytes)
        {


            using (var socket = GetFactory().BusOpen().ThenDial(address).Unwrap())
            {
                var msg = factory.CreateMessage();
                msg.Append(bytes);
                socket.SendMsg(msg);
            }
        }

        internal byte[] GetData()
        {
            return queue.Take();
        }

        public  void  Receive(string address)
        {
            Thread thread = new Thread(() => { 
          
            var socket = GetFactory().BusOpen().ThenListen(address).Unwrap();
            while(true)
            {
               var msg= socket.RecvMsg().Unwrap().AsSpan();
               var data= msg.ToArray();
                queue.Add(data);
            }
            });
            thread.IsBackground = true;
            thread.Start();
        }
    }
}
