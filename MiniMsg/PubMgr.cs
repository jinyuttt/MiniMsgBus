using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Concurrent;

namespace MiniMsg
{

    /// <summary>
    /// 发布数据关联
    /// </summary>
    internal  class PubMgr
    {
        private static readonly Lazy<PubMgr> pub = new Lazy<PubMgr>(() => new PubMgr());

        /// <summary>
        /// 异常数据
        /// </summary>
        private readonly BlockingCollection<Records> errorRecords = new BlockingCollection<Records>();
        readonly List<ErrorQueue> queues = new List<ErrorQueue>();
        readonly SemaphoreSlim _semaphore = new SemaphoreSlim(Environment.ProcessorCount);
        readonly SemaphoreSlim  sub = new SemaphoreSlim(Environment.ProcessorCount);
        private ulong msgid = 0;
      
        public static PubMgr Instance
        {
            get { return pub.Value; }
        }
       
        private PubMgr()
        {
            // Check();
            CheckErrRecord();
        }

        /// <summary>
        /// 处理发送失败的数据
        /// </summary>
        private void Check()
        {
            Thread check = new Thread(() =>
           {

               //每个主题，循环处理每个主题发送失败的数据
               foreach (var p in errorRecords.GetConsumingEnumerable())
               {
                  
                   //获取本地订阅节点
                   var addr= SubTable.Instance.GetAddressLst(p.Topic);
                   foreach(var kv in p.Record)
                   {
                      
                       //将数据再次发送
                      var ret= DataTransfer.Send(p.Topic, kv.Value,kv.Key);
                        if (ret == null|| ret.Length == 0)
                       {
                           //还是失败，判断是否是多网卡绑定订阅地址
                           var err = addr.SubAddresses.Find(X => X.Address.CompareTo(kv.Key)==0);
                           if (err != null)
                           {
                               //返回判断非异常的地址
                               var cur=  err.AllAddress.FindAll(X => !err.ErrorAddress.Contains(X));
                               if (cur.Count == 0)
                               {
                                   //已经没有正确地址,节点所有地址都发送10次后移除节点，任务订阅节点异常
                                   if (err.NumAll > 10)
                                   {
                                       lock (addr)
                                       {
                                           addr.SubAddresses.Remove(err);
                                           addr.LstAddress.RemoveAll(X => err.AllAddress.Contains(X));
                                       }
                                       continue;
                                   }
                                   cur = err.AllAddress;//全尝试
                                   err.NumAll++;
                               }
                               //发送需要发送的节点
                               foreach(var  right in cur)
                               {
                                   //Console.WriteLine("异常处理发送，"+right);
                                   ret = DataTransfer.Send(p.Topic, kv.Value,right);
                                   if (ret!=null&&ret.Length > 0)
                                   {
                                       //收到回复该地址正常
                                       err.ErrorAddress.Add(err.Address);
                                       err.Address = right;
                                       Console.WriteLine("功能处理异常，" + right);
                                       break;
                                   }
                               }
                              
                           }
                       }
                     
                   }
                   

               }
           });
            check.Name = "checknode";
            check.IsBackground = true;
            check.Start();
        }
        

        private void CheckErrRecord()
        {
            Thread check = new Thread(() =>
            {
                for(int i=0;i< Environment.ProcessorCount; i++)
                {
                    //初始化
                    queues.Add(new ErrorQueue() { IsComlepte = true, Records = new ConcurrentQueue<Records>(), Topic = "" });
                   
                }
                //每个主题，循环处理每个主题发送失败的数据
                foreach (var p in errorRecords.GetConsumingEnumerable())
                {
                  
                    var cur = RecordsProcess(p);
                    if (cur.IsComlepte)
                    {
                        _semaphore.Wait();//等待前面的主题处理完成，不能太多线程
                   
                        StartThredRecord(cur);
                    }
                 
                }
            });
            check.Name = "checknode";
            check.IsBackground = true;
            check.Start();
        }

        private ErrorQueue RecordsProcess(Records queue)
        {
            ErrorQueue errorQueue = null;
            var cur = queues.Find(X => X.Topic == queue.Topic);

            if (cur == null)
            {
                var curTmp = queues.Find(X => X.IsComlepte && X.Records.IsEmpty);
                if (curTmp == null)
                {

                  
                    var tmp = queues.Find(X => X.IsComlepte);
                    tmp.Records.Enqueue(queue);
                    tmp.Topic = queue.Topic;
                    errorQueue = tmp;
                }
                else
                {

                    curTmp.Records.Enqueue(queue);
                    curTmp.Topic = queue.Topic;
                    errorQueue = curTmp;
                }
            }
            else
            {

                cur.Records.Enqueue(queue);
                errorQueue = cur;

            }
            return errorQueue;

        }


        private void StartThredRecord(ErrorQueue queue)
        {
            Task.Run(() =>
            {
                queue.IsComlepte = false;
                while (!queue.Records.IsEmpty)
                {
                   
                    //获取本地订阅节点
                    Records p = null;
                    queue.Records.TryDequeue(out p);
                  
                    var addr = SubTable.Instance.GetAddressLst(p.Topic);
                  
                    foreach (var kv in p.Record)
                    {

                        //将数据再次发送
                        var ret = DataTransfer.Send(p.Topic, kv.Value, kv.Key);
                        if (ret == null || ret.Length == 0)
                        {
                            //还是失败，判断是否是多网卡绑定订阅地址
                            var err = addr.SubAddresses.Find(X => X.Address.CompareTo(kv.Key) == 0);
                            if (err != null)
                            {
                                //返回判断非异常的地址
                                var cur = err.AllAddress.FindAll(X => !err.ErrorAddress.Contains(X));
                                if (cur.Count == 0)
                                {
                                    //已经没有正确地址,节点所有地址都发送10次后移除节点，任务订阅节点异常
                                    if (err.NumAll > 10)
                                    {
                                        lock (addr)
                                        {
                                            addr.SubAddresses.Remove(err);
                                            addr.LstAddress.RemoveAll(X => err.AllAddress.Contains(X));
                                        }
                                        continue;
                                    }
                                    cur = err.AllAddress;//全尝试
                                    err.NumAll++;
                                }
                                //发送需要发送的节点
                                foreach (var right in cur)
                                {
                                    //Console.WriteLine("异常处理发送，"+right);
                                    ret = DataTransfer.Send(p.Topic, kv.Value, right);
                                    if (ret != null && ret.Length > 0)
                                    {
                                        //收到回复该地址正常
                                        err.ErrorAddress.Add(err.Address);
                                        err.Address = right;
                                        Console.WriteLine("成功处理异常，" + right);
                                        break;
                                    }
                                }

                            }
                        }

                    }
                }
                queue.IsComlepte = true;
                _semaphore.Release();
            });
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
          
            if (lst != null && lst.Count > 0)
            {

                //记录发送异常的地址和数据
                Dictionary<string, byte[]> dic = new Dictionary<string, byte[]>();
                foreach (var p in lst)
                {
                  
                    var ret = DataTransfer.Send(topic, msg, p, 0, Interlocked.Increment(ref msgid));
                    if (ret==null||ret.Length == 0)
                    {
                        //发布失败没有返回
                        dic[p] = msg;
                    }

                }
                if (dic.Count > 0)
                {
                    //存入异常数据，等待再次处理
                    errorRecords.Add(new Records() { Record = dic, Topic = topic });
                }
            }
            else
            {

                //本地没有订阅节点
                var lstPub = PubTable.Instance.GetAddress(topic);//从全局发布表中查询
                if (lstPub != null && lstPub.Contains(LocalNode.LocalAddress) && !LocalNode.LocalAddress.Contains("*"))
                {
                    //本节点已经发布过地址就丢数据,说明没有节点订阅这个主题
                    return;
                }
                if (LocalNode.LocalAddress.Contains("*") && lstPub != null)
                {
                    //本节点绑定了所有网卡，找到本节点所有网卡地址
                    var pubs = LocalNode.LocalAddressFamily.Where(X => lstPub.Contains(X.IPV4)).ToList();

                    if (pubs.Count > 0)
                    {
                        //已经发布过地址就丢数据,说明没有节点订阅这个主题
                        return;
                    }
                }

                SubMgr.Instance.OpenChanel();//初始化，启动数据接收订阅
                //第一次本节点发布
                TopicPgm pgm = new TopicPgm();
                var lstLocal = pgm.PgmPub(topic);
               
                //将新发布节点加入本地
                foreach (var p in lstLocal)
                {
                    PubTable.Instance.Add(topic, p);
                }
                sub.Wait();
                Task.Run(() =>
                {
                    //等待20次，每次100ms,1秒了完成发布，否则数据丢失；因为地址通知考虑没有回复
                    //通知地址后不会有消息回复，会增加消息交互量
                    for (int i = 0; i < 10; i++)
                    {
                        Thread.Sleep(100);
                        //再次检查是否有订阅
                        lst = SubTable.Instance.GetAddress(topic);
                        if (lst != null&&lst.Count>0)
                        {
                          
                            foreach (var p in lst)
                            {
                               
                                DataTransfer.Send(topic, msg, p);
                            }

                            break;
                        }
                    }
                    sub.Release();
                });
            }
        }
    }



    internal class ErrorQueue
    {
        public string Topic { get; set; }

        public ConcurrentQueue<Records> Records { get; set; }

        public bool IsComlepte { get; set; }
    }
}
