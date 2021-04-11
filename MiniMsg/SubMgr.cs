using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MiniMsg
{

   /// <summary>
   /// 订阅管理
   /// </summary>
  public  class SubMgr
    {
        
        private static readonly Lazy<SubMgr> sub = new Lazy<SubMgr>(() => new SubMgr());
        public static SubMgr Instance
        {
            get { return sub.Value; }
        }

        private SubMgr()
        {
            TopicPgm pgm = new TopicPgm();
            pgm.ReceiveTopic += Pgm_ReceiveTopic;
         
            Thread thread = new Thread(() => {
                pgm.PgmSub();
               });
            thread.IsBackground = true;
            thread.Name = "jie";
            thread.Start();
        }

        private void Pgm_ReceiveTopic(string obj)
        {
            string[] tmp = obj.Split('_');
            StringBuilder builder = new StringBuilder();
            for(int i=0;i<tmp.Length-1;i++)
            {
                builder.AppendFormat("{0}_", tmp[i]);
            }
            builder.Remove(builder.Length - 1, 1);
            string topic = builder.ToString();

            //保存到全局发布列表
            PubTable.Instance.Add(topic, tmp[tmp.Length - 1]);
           var ov= LocalNode.GetLocal(topic);
            if (ov != null)
            {
                //本地已经订阅的发送订阅信息
                this.SendSub(topic, ov as NngTopic);
            }

        }

        public void  SendSub(string topic, NngTopic sub)
        {
            var lst = PubTable.Instance.GetAddress(topic);
            if(lst==null)
            {
                //没有发布地址，放入本地节点信息
                LocalNode.AddLocal(topic,sub);
                return;
            }
            foreach (var str in lst)
            {
                DataNative data = new DataNative();
                string addr = topic+"_" + LocalNode.LocalAddress;
                data.Send(UTF8Encoding.UTF8.GetBytes(addr));
            }
        }
    }
}
