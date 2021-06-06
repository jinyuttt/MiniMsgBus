using System;
using System.IO;
using System.Text;
using System.Threading;
using ZeroMQ;

namespace MiniMsg
{

    /// <summary>
    /// 进程通信
    /// </summary>
    public class TopicZmqIpc
    {
        static string ipcfile = "";
        public event Action<string,byte[]> ReceiveTopic;
         ZSocket pubSocket= new ZSocket(ZSocketType.PUB);
         ZSocket subSocket = new ZSocket(ZSocketType.SUB);
        bool isStart = false;
        bool isBind = true;
        bool isSubBind = true;
        static TopicZmqIpc()
        {
            ipcfile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "minimsg.ipc");
        }

        private void ReceiveData()
        {
            if (isStart)
            {
                return;
            }
            isStart = true;
            Thread recTopic = new Thread(() =>
              {
                
                  while (true)
                  {
                     
                      using (var reply = subSocket.ReceiveMessage())
                      {
                        
                          string topic = reply.Pop().ReadString(Encoding.UTF8);
                          var buf = reply.Pop().Read();
                          if (ReceiveTopic != null)
                          {
                              ReceiveTopic(topic, buf);
                          }
                      }
                  }
              });
            recTopic.IsBackground = true;
            recTopic.Name = "ipcrec";
            recTopic.Start();
        }
        public  void ZmqIpcSub(string topic)
        {
            try
            {


                if (isSubBind)
                {
                    subSocket.Connect("ipc://" + ipcfile);
                    isSubBind = false;

                   
                }

                if (string.IsNullOrEmpty(topic))
                {
                    subSocket.SubscribeAll();
                }
                else
                {
                    subSocket.Subscribe(topic);
                   
                }
                if(isStart)
                {
                    return;
                }
                lock(subSocket)
                {
                    
                    ReceiveData();
                    
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

      
       
        public void ZmqIpcPub(string topic,byte[]buf)
        {
            try
            {
                if (isBind)
                {
                    pubSocket.Bind("ipc://" + ipcfile);
                    isBind = false;
                  
                }
                using(var msg=new ZMessage())
                {
                    msg.Add(new ZFrame(topic, Encoding.UTF8));
                    msg.Add(new ZFrame(buf));
                    pubSocket.SendMessage(msg);
                   
                }
                  
                
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
   
    }
}
