
using EF.RPC.Impl.ConsumerImpl;
using EF.RPC.Protobuf;
using Fleck;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace EF.RPC.Impl.WebSocketImpl
{
    public class WebSocketConsumerMap : MsgConsumerMap
    {
        string ipaddres = "0.0.0.0";
        string sockettype = "ws";
        string ipport = "50000"; //端口
        private delegate int Weekly(WebSocketServer socket, List<IWebSocketConnection> allSockets,
          string Markmessage,
          int Marknum,
          string ConnectsocketnotificationSection = "已连接",
          string ConnectsocketCloseSection = "连接已关闭",
          string ReturnMesSection = "数据已更新!"
          );
        private delegate int WeeklyAction(WebSocketServer socket, Action<IWebSocketConnection> webSocketConnection);
        private static string Markmessage = "";

        private void Run()
        {
            FleckLog.Level = LogLevel.Debug;
            var allSockets = new List<IWebSocketConnection>();
            var server = new WebSocketServer($"{sockettype }://{ipaddres}:{ ipport}//");
            try
            {
              
                Action<IWebSocketConnection> config = new Action<IWebSocketConnection>(socket =>
               {
                   socket.OnOpen = () =>
                   {
                      
                       allSockets.Add(socket);
                   };
                   socket.OnClose = () =>
                   {
                       Console.WriteLine();
                       allSockets.Remove(socket);

                   };
                   socket.OnMessage = message =>
                   {
                       Markmessage = message;
                       Console.WriteLine(Markmessage);
                        //message 然后开启查询
                        allSockets.ToList().ForEach(s => s.Send($"Echo: {message}{socket.ConnectionInfo.Path}"));

                   };
               });
                WeeklyAction weeklyAction = new WeeklyAction(SendMsg);
                weeklyAction.Invoke(server, config);

            }
            catch (Exception ex)
            {

                Console.WriteLine($"错误:  { ex.Message}");
            }
           
        }

        public override void GetMathsInfo<T>()
        {
            this.clear();
            //传递参数
            var t = typeof(T);
            //var t = Type.GetType(className);

            List<Type> interfaces = t.GetInterfaces().Where(type => (!typeof(MsgController).IsAssignableFrom(type))).ToList();
            if (interfaces.Count > 1 || interfaces.Count == 0) throw new Exception("MsgController的实现类必须继承Sharing中共享的接口," +
                  "且不能继承MsgController外的其他接口,且不能多层继承");

            for (int k = 0; k < interfaces.Count; k++)
            {
                this.ControllerObj = Activator.CreateInstance(t);
                this.packageName = t.Namespace;
                this.FullName = t.FullName;
                this.className = t.Name;
                this.interfaceFullName = interfaces[k].FullName;
                try
                {
                    #region 方法二
                    MethodInfo[] info = t.GetMethods();
                    for (int i = 0; i < info.Length; i++)
                    {
                        MsgFun mf = new MsgFun();

                        MethodInfo md = info[i];
                        if (md.Name.Equals("ToString") || md.Name.Equals("Equals") || md.Name.Equals("GetHashCode") || md.Name.Equals("GetType")) continue;
                        //方法名
                        string mothodName = md.Name;
                        Console.WriteLine($"类名:{ t.Name}, {"方法名：" + md.Name}");
                        //参数集合
                        ParameterInfo[] paramInfos = md.GetParameters();

                        MsgFun mfs = new MsgFun();
                        mfs.Name = md.Name;
                        mfs.rep = md.ReturnType;

                        //var objj = Activator.CreateInstance(mfs.req);
                        mfs.methodInfo = md;
                        this.put(md.Name, mfs);

                        for (int j = 0; j < paramInfos.Length; j++)
                        {
                            ParameterInfo parameterInfo = paramInfos[j];
                            mfs.req = parameterInfo.ParameterType;
                          
                            Console.WriteLine($"类名:{ t.Name}, {"方法名：" + md.Name},{"入参" + j + ":" + parameterInfo.ParameterType}");
                        }
                        FleckLog.Level = LogLevel.Debug;
                        var allSockets = new List<IWebSocketConnection>();
                        var server = new WebSocketServer($"{sockettype }://{ipaddres}:{ ipport}//{this.interfaceFullName + "." + md.Name}");

                        Action<IWebSocketConnection> config = new Action<IWebSocketConnection>(socket =>
                        {
                            socket.OnOpen = () =>
                            {

                                allSockets.Add(socket);
                            };
                            socket.OnClose = () =>
                            {
                                Console.WriteLine();
                                allSockets.Remove(socket);

                            };
                            socket.OnMessage = message =>
                            {
                                SuperMsg superMsg = this.serializer.DeSerializeString<SuperMsg>(message);
                                object[] objs = new object[] { superMsg.msg };// new object[] { JsonSerializer.CreateDefault().Deserialize( ea.Body.ToArray().ToString(), parameterInfo.ParameterType) };//new object[] { ProtobufSerializer.DeSerializeBytes(parameterInfo.ParameterType, ea.Body.ToArray()) }
                                object rep = mfs.methodInfo.Invoke(this.ControllerObj, objs);
                                if (md.ReturnType != typeof(void))
                                {
                                    //message 然后开启查询
                                    allSockets.ToList().ForEach(s => s.Send(this.serializer.SerializeString(new SuperMsg(rep))));
                                }

                            };
                        });
                        WeeklyAction weeklyAction = new WeeklyAction(SendMsg);
                        weeklyAction.Invoke(server, config);
                        Console.WriteLine($"类名:{ t.Name}, {"方法名：" + md.Name},{"返回值" + ":" + md.ReturnType}");
                    }
                    #endregion
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
        }
        /// <summary>
        /// Seed
        /// </summary>
        /// <param name="webSocket"></param>
        /// <param name="allSockets"></param>
        /// <param name="Markmessage"></param>
        /// <param name="Marknum"></param>
        /// <param name="ConnectsocketnotificationSection"></param>
        /// <param name="ConnectsocketCloseSection"></param>
        /// <param name="ReturnMesSection"></param>
        /// <returns></returns>
        private int SendMes(WebSocketServer webSocket, List<IWebSocketConnection> allSockets,
              string Markmessage,
              int Marknum,
              string ConnectsocketnotificationSection = "",
              string ConnectsocketCloseSection = "",
              string ReturnMesSection = ""
              )
        {
            webSocket.Start(socket =>
            {
                socket.OnOpen = () =>
                {
                    Console.WriteLine(ConnectsocketnotificationSection);
                    allSockets.Add(socket);
                };
                socket.OnClose = () =>
                {
                    Console.WriteLine(ConnectsocketCloseSection);
                    allSockets.Remove(socket);

                };
                socket.OnMessage = message =>
                {
                    Markmessage = message;
                    Console.WriteLine(Markmessage);
                    //message 然后开启查询
                    allSockets.ToList().ForEach(s => s.Send($"Echo: {message}{socket.ConnectionInfo.Path}"));

                };
            });
           
            return 0;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="webSocket"></param>
        /// <param name="config"></param>
        /// <returns></returns>
        private int SendMsg(WebSocketServer webSocket, Action<IWebSocketConnection> config)
        {
            webSocket.Start(config);

            return 0;
        }
    }
}
