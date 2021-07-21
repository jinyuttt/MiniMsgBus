# MiniMsgBus
集成消息通信
## 说明
1.ZMQ集成PGM通信，只负责发布节点的主题发布地址  
2.一个节点的数据发布地址与订阅接收地址相同  
3.可以绑定多网卡，数据通过节点标识和消息ID订阅节点去重  
3.发布节点根据订阅节点的标识，选择其中一个地址发送数据，逐步稳定后就不往多卡发送，减少订阅方去重压力   
4.发布节点根据订阅节点返回判断订阅节点是否已经无效，可以移除  
5.数据通信集成nanomsg.nng库通信 
6.进程间采用内存共享通信c++程序，同时支持windows和Linux  
 

## 测试 
1.windows平台已经测试  
2.项目已经完成订阅发布模式，代码简单，注释明确    
3.使用的zmq和nng库在项目中的dll文件夹，64位  
4.linux平台已经测试(deepin)  
5.该组件可以使用了  

## 程序介绍 
1.通信组件初始化，可以设置IP与端口，也可以不设置  
2.启动全局发布地址通信（pgm和ipc）  
3.通过pgm和ipc发布global主题，从其它节点获取全局发布地址  
4.初始化完成  

### 发布数据
1.检查本地订阅方地址，根据地址发送数据  
2.本地无订阅，则检查全局发布地址是否有本节点地址，有则表示没有节点订阅，抛弃数据，没有则表示第一次发布数据，立即通知一次发布节点，等待1秒订阅，再次检查发送    
3.通知发布地址时，将本地通信地址发布，如果ip是*,则获取本地全部ip,组合ip全部发送  

### 订阅数据
1.检查全局发布地址，向发布此主题的地址发送订阅信息（主题+本地通信地址）   
2.订阅是否成功不用考虑  

## 翻译
有兴趣的童鞋可以全部翻译成c++程序  

## 使用 
             
1.订阅发布 
支持三类通信，进程内（观察者模式），进程间（内存共享），网络通信（订阅发布）   
			  var bus=  BusFactory.Create(BusType.Ipc);  
              bus.Subscribe("AA");  
              bus.OnCall += Bus_OnCall;  
               bus.Publish("AA", new byte[] { 34 });  
2.点对点通信  			    
直接创建tcp通信  
  var ptp = PtpFactory.Create();  
            ptp.Address = "127.0.0.1";  
            ptp.Port = 6667;  
            ptp.Start();  
            ptp.Send(new byte[] { 45 });  
3.订阅发布扩展RPC  
  var rpc = BusFactory.Create(BusType.tcp);  
            LocalNode.IsMsgReturn = true;//启用消息反馈  
          //  rpc.Subscribe("AA");  
           // rpc.OnCall += Bus_OnCall;  
            msgid= rpc.Publish("AA", new byte[] { 34 });  
            MsgTopicCount.Instance.OnCall += Instance_OnCall;  
 
  private static void Instance_OnCall(PubRecords obj)  
        {
            if(obj.MsgId==msgid)  
            {
                if(obj.SucessNum>0)  
                {  
                    //
                }  
                else  
                {  
                    //失败
                }  
            }  
        } 
		
	4.增加默认主题  
  如果主题空，则使用默认主题  	
##  欢迎测试  

感兴趣的童鞋可以测试  

