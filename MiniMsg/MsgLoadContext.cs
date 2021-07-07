using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Loader;

namespace MiniMsg
{
    internal class MsgLoadContext : AssemblyLoadContext
    {
        protected override Assembly Load(AssemblyName assemblyName)
        {
          
            return null;
         
        }

        protected override IntPtr LoadUnmanagedDll(string unmanagedDllName)
        {
            Console.WriteLine(unmanagedDllName);
            //if (unmanagedDllName == "udt")
            //{
#if NETSTANDARD2_0
                bool is64bit = Environment.Is64BitProcess;
#else
                bool is64bit = (IntPtr.Size == 8);
#endif
                string arch = string.Empty;
                switch (RuntimeInformation.ProcessArchitecture)
                {
                    case Architecture.Arm64: arch = "-arm64"; break;
                    case Architecture.Arm: arch = "-arm"; break;
                    default: arch = is64bit ? "-x64" : "-x86"; break;
                }
                if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                {
                    var fullPath = Path.Combine(assemblyPath, "runtimes", "osx" + arch, "native", "libnng.dylib");
                    return LoadUnmanagedDllFromPath(fullPath);
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    var fullPath = Path.Combine(assemblyPath, "runtimes", "linux" + arch, "native", "udt.so");
                    return LoadUnmanagedDllFromPath(fullPath);
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    var fullPath = Path.Combine(assemblyPath, "runtimes", "win" + arch, "native", "udt.dll");
                    return LoadUnmanagedDllFromPath(fullPath);
                }
                else
                {
                    throw new Exception("Unexpected runtime OS platform: " + RuntimeInformation.OSDescription);
                }
         
        }

       
        readonly string assemblyPath;
        private AssemblyDependencyResolver _resolver;
        public MsgLoadContext(string managedAssemblyPath):base("MsgTransfer",false)
        {
            
            this.assemblyPath = managedAssemblyPath;
            this.ResolvingUnmanagedDll += PluginLoadContext_ResolvingUnmanagedDll;
            this.Resolving += PluginLoadContext_Resolving;
            //第1步,解析des.json文件,并调用Load和LoadUnmanagedDll函数
            _resolver = new AssemblyDependencyResolver(assemblyPath);
        }
       
        private Assembly PluginLoadContext_Resolving(AssemblyLoadContext arg1, AssemblyName arg2)
        {
            return null;
        }

        private IntPtr PluginLoadContext_ResolvingUnmanagedDll(Assembly arg1, string arg2)
        {
            return IntPtr.Zero;
        }

    }
}
