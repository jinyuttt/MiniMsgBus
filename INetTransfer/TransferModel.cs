using System;

namespace INetTransfer
{
    /// <summary>
    /// 定义通信组件
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class TransferModel : System.Attribute
    {
        public string Name { get; set; }
        public TransferModel(string name)
        {
            this.Name = name.ToLower().Trim();
        }
    }
}
