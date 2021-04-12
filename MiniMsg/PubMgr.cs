using System;
using System.Threading;
using System.Threading.Tasks;

namespace MiniMsg
{
    public  class PubMgr
    {
        private static readonly Lazy<PubMgr> pub = new Lazy<PubMgr>(() => new PubMgr());
        public static PubMgr Instance
        {
            get { return pub.Value; }
        }

        /// <summary>
        /// 发送数据
        /// </summary>
        /// <param name="topic"></param>
        /// <param name="msg"></param>
        public void Send(string topic, byte[] msg)
        {

            //从本地已经订阅的地址查找
            var lst = SubTable.Instance.GetAddress(topic);
            if (lst != null)
            {
                DataTransfer native = new DataTransfer();
                foreach(var p in lst)
                {
                    native.Send(topic, msg,p);
                }
                
            }
            else
            {

                var lstPub = PubTable.Instance.GetAddress(topic);
                if (lstPub != null && lstPub.Contains(LocalNode.LocalAddress))
                {
                    //已经发布过地址就丢数据,说明没有节点订阅这个主题
                    return;
                }
                else
                {
                    //第一次本节点发布
                    TopicPgm pgm = new TopicPgm();
                    pgm.PgmPub(topic);
                    Task.Run(() =>
                    {
                        for (int i = 0; i < 10; i++)
                        {
                            Thread.Sleep(100);
                            //再次检查是否有订阅
                            lst = SubTable.Instance.GetAddress(topic);
                            if (lst != null)
                            {
                                DataTransfer native = new DataTransfer();
                               
                                foreach (var p in lst)
                                {
                                    native.Send(topic,msg,p);
                                }
                              
                                break;
                            }
                        }
                    });

                }
            }
        }
    }
}
