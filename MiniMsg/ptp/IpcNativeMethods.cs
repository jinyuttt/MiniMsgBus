using System;
using System.Runtime.InteropServices;

namespace MiniMsg
{
    using static Globals;
    sealed partial class Globals
    {
        public const string IpcDll = "ipc";
    }

    /// <summary>
    /// 本地封装类
    /// </summary>
    public  class IpcNativeMethods
    {
        [DllImport(IpcDll)]
        public static extern void MapChannel(string name);

        [DllImport(IpcDll)]
        public static extern void Send(byte[] buf, int len);
        [DllImport(IpcDll)]
        public static extern IntPtr Rec(ref int len);


        [DllImport(IpcDll)]
        public static extern void SendName(string name,byte[] buf, int len);
        [DllImport(IpcDll)]
        public static extern IntPtr RecName(string name,ref int len);

    }
}
