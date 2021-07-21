using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZeroMQ;

namespace MiniMsg
{

    /// <summary>
    /// 暂时不使用
    /// </summary>
    internal class TopicZmqIpcTcp
    {
        public  void Send(string addr)
        {
            try
            {
                using (var requester = new ZSocket(ZSocketType.REQ))
                {
                    requester.Connect("tcp://127.0.0.1:8080");
                    requester.SendFrame(new ZFrame(addr));
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        public void Rec(int num)
        {
            try
            {
                int count = 0;
                using (var requester = new ZSocket(ZSocketType.REP))
                {
                    requester.Bind("tcp://127.0.0.1:8080");
                   
                    while (true)
                    {
                        var tsp = requester.ReceiveFrame();
                        requester.SendFrame(new ZFrame("reply"));
                        Console.WriteLine("第{0}线程，{1}",num,tsp.ReadString());
                        count++;
                        if(count>20)
                        {
                            break;
                        }
                    }
                }
                Console.WriteLine("{0}退出接收", num);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
    }
}
