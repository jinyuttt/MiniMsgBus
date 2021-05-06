using INetTransfer;
using System.IO;
using System;
using System.Linq;
using System.Runtime.Loader;

namespace MiniMsg
{

    /// <summary>
    /// 数据底层网络传输
    /// </summary>
    public class DataNative
    {
        static ISocketFactory Factory = null;
        static string asmPath = null;
        static MsgLoadContext loadContext = null;
        public static object factoryObj = null;
        // public Func<string, byte[], byte[]> MsgSend;
        //public Func<string, string> MsgRecvice;
        // public Func<byte[]> dataCall = null;

        public  dynamic MsgSend;
        public dynamic MsgRecvice;
        public dynamic dataCall = null;
        public dynamic  LisClose = null;

        private ITransfer transfer = null;

        static DataNative()
        {
           
            ITransfer.NodeGuid = LocalNode.GUID;
            var path =Path.GetDirectoryName(typeof(DataNative).Assembly.Location);
            loadContext = new MsgLoadContext(path);
            asmPath = path;
            LoadNativeFile();
        }

        public DataNative()
        {
            //if(factoryObj!=null)
            //{
            //    var mth= factoryObj.GetType().GetField("MsgSend");
            //    this.MsgSend = mth.GetValue(factoryObj);
            //    mth = factoryObj.GetType().GetField("MsgRecvice");
            //    this.MsgRecvice = mth.GetValue(factoryObj);
            //    mth = factoryObj.GetType().GetField("dataCall");
            //    this.dataCall = mth.GetValue(factoryObj);
            //    mth = factoryObj.GetType().GetField("LisClose");
            //    this.LisClose = mth.GetValue(factoryObj);
            //}
        }

        private static void LoadFile()
        {
            string[] files = Directory.GetFiles(asmPath, "*.dll");
            foreach(var f in files)
            {
                try
                {
                   // var fs = File.OpenRead(f);
                  //  var assm = loadContext.LoadFromStream(fs);
                    var assm = MsgLoadContext.Default.LoadFromAssemblyPath(f);
                    if (assm != null)
                    {
                        try
                        {
                            var factory = assm.DefinedTypes.Where(X => X.IsClass && X.IsPublic && X.GetInterface("ISocketFactory") != null);
                            foreach (var asm in factory)
                            {
                                var atts = asm.GetCustomAttributes(false);
                                var ty = typeof(TransferModel);
                                var ss = MsgLoadContext.Default;
                                var fff = AssemblyLoadContext.All;
                                foreach (var p in atts)
                                {
                                    if(p.GetType().Name== "TransferModel")
                                    {
                                       var v= p.GetType().GetProperty("Name").GetValue(p).ToString();
                                        if (v == LocalNode.Netprotocol)
                                        {
                                             Factory =(ISocketFactory)asm.Assembly.CreateInstance(asm.FullName,false);
                                            object obj =asm.Assembly.CreateInstance(asm.FullName, false);
                                            if(obj!=null)
                                            {
                                                var fsAssembly = MsgLoadContext.Default.Assemblies.Where(X => X.ManifestModule.ScopeName == "INetTransfer.dll");
                                                foreach(var cur in fsAssembly)
                                                {
                                                  var tys=  cur.DefinedTypes.Where(X => X.IsClass && X.Name == "MiniMsgProxy");
                                                    foreach (var tt in tys)
                                                    {
                                                       var fobj= cur.CreateInstance(tt.FullName);
                                                        var  fv= tt.GetField("Factory", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
                                                        fv.SetValue(fobj, obj);
                                                        factoryObj = fobj;

                                                    }
                                                }
                                            }
                                          
                                            break;
                                        }
                                    }
                                    
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex);
                        }
                      //  fs.Close();
                    }
                }
                catch(Exception ex)
                {
                    Console.WriteLine(ex);
                }
            }
        
        }

        /// <summary>
        /// 加载库
        /// </summary>
        private static void LoadNativeFile()
        {
            string[] files = Directory.GetFiles(asmPath, "*.dll");
            foreach (var f in files)
            {
                try
                {
                    
                    var assm = MsgLoadContext.Default.LoadFromAssemblyPath(f);
                    if (assm != null)
                    {
                        try
                        {
                            var factory = assm.DefinedTypes.Where(X => X.IsClass && X.IsPublic && X.GetInterface("ISocketFactory") != null);
                            foreach (var asm in factory)
                            {
                                var atts = asm.GetCustomAttributes(false);
                             
                              
                                foreach (var p in atts)
                                {
                                    var trf = p as TransferModel;

                                    if (trf != null)
                                    {
                                        if (trf.Name == LocalNode.Netprotocol)
                                        {
                                            Factory = (ISocketFactory)asm.Assembly.CreateInstance(asm.FullName, false);
                                            break;
                                        }
                                    }
                                    
                                    

                                }
                            }
                        }
                        catch(BadImageFormatException ex)
                        {
                            continue;
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex);
                        }
                       
                    }
                }
                catch (BadImageFormatException ex)
                {
                    continue;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
            }

        }

        /// <summary>
        /// 发送
        /// </summary>
        /// <param name="address"></param>
        /// <param name="v"></param>
        /// <returns></returns>
        public byte[] Send(string address, byte[] v)
        {
            // return MsgSend(address, v);
            return Factory.Create().Send(address, v);
        }

        /// <summary>
        /// 接收
        /// </summary>
        /// <param name="v"></param>
        /// <returns></returns>
        public string Receive(string v)
        {
            transfer = Factory.Create();
            return transfer.Receive(v);
          //  return MsgRecvice(v);
        }

        /// <summary>
        /// 获取数据
        /// </summary>
        /// <returns></returns>
        public byte[] GetData()
        {
            if(transfer!=null)
            {
                return transfer.GetData();
            }
            return null;
          //  return dataCall();
        }
        public void Close()
        {
            if (transfer != null)
            {
                 transfer.Close();
            }
            // LisClose();
        }
    }
}
