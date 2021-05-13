
using EF.RPC.Impl.annotation;
using EF.RPC.Impl.ConsumerImpl;
using EF.RPC.Protobuf;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace EF.RPC.Impl.RabbitMQImpl
{
    public class RabbitMQMsgConsumerMap : MsgConsumerMap 
    {
        ConnectionFactory factory;
        public RabbitMQMsgConsumerMap setOptions(RabbitMQOptions opt)
        {
            factory = opt.factory;
            return this;
        }
        public override void GetMathsInfo(Type t)
        {
            this.clear();

 
            try
            {
                IConnection conection = factory.CreateConnection();
                #region 方法二
                MethodInfo[] info = t.GetMethods();
                List<Type> interfaces = t.GetInterfaces().Where(type => (!typeof(MsgController).IsAssignableFrom(type))).ToList();
                if (interfaces.Count > 1|| interfaces.Count ==0) throw new Exception("MsgController的实现类必须继承Sharing中共享的接口," +
                    "且不能继承MsgController外的其他接口,且不能多层继承");

                for (int k = 0; k < interfaces.Count; k++)
                {
                    ///获取接口和类信息
                    this.ControllerObj = Activator.CreateInstance(t);
                    this.packageName = t.Namespace;
                    this.FullName = t.FullName;
                    this.interfaceFullName = interfaces[k].FullName;

                    IEnumerable<CustomAttributeData> eFRpcServiceAttribute = t.CustomAttributes.Where(type => type.AttributeType == typeof(EFRpcServiceAttribute)).ToList();
                    foreach (var a in eFRpcServiceAttribute)
                    {
                        foreach (var kv in a.NamedArguments)
                        {
                            if (kv.MemberName.Equals("version")) {
                                this.version = kv.TypedValue.Value.ToString();
                            }                       
                        } 
                    }
                    
                    for (int i = 0; i < info.Length; i++)
                    {
                        MsgFun mf = new MsgFun();
                        MethodInfo md = info[i];

                        //方法名
                        string mothodName = md.Name;
                        Console.WriteLine($"类名:{ t.Name}, {"方法名：" + md.Name}");
                        //参数集合
                        ParameterInfo[] paramInfos = md.GetParameters();
                        if (md.Name.Equals("ToString") || md.Name.Equals("Equals") || md.Name.Equals("GetHashCode") || md.Name.Equals("GetType") ) continue;
                        MsgFun mfs = new MsgFun();
                        mfs.Name = md.Name;
                        mfs.rep = md.ReturnType;
                        mfs.methodInfo = md;
                        this.put(md.Name, mfs);
                        for (int j = 0; j < paramInfos.Length; j++)
                        {
                            ParameterInfo parameterInfo = paramInfos[j];
                            mfs.req = parameterInfo.ParameterType;
                            Console.WriteLine($"类名:{ t.Name}, {"方法名：" + md.Name},{"入参" + j + ":" + parameterInfo.ParameterType}");
                        }
                        IModel channel = conection.CreateModel();
                        channel.QueueDeclare(queue: this.version+this.interfaceFullName + "." + md.Name, durable: false,
                            exclusive: false, autoDelete: false, arguments: null);
                        var consumer = new EventingBasicConsumer(channel);
                        Console.WriteLine("[*] Waiting for message.");
                        consumer.Received += (model, ea) =>
                        {
                            var properties = ea.BasicProperties;
                            var replyProerties = channel.CreateBasicProperties();
                            replyProerties.CorrelationId = properties.CorrelationId;

                            SuperMsg superMsg = this.serializer.DeSerializeString<SuperMsg>(ea.Body.ToString());
                            object[] objs = new object[] { superMsg.msg };// new object[] { JsonSerializer.CreateDefault().Deserialize( ea.Body.ToArray().ToString(), parameterInfo.ParameterType) };//new object[] { ProtobufSerializer.DeSerializeBytes(parameterInfo.ParameterType, ea.Body.ToArray()) }
                            object rep = mfs.methodInfo.Invoke(this.ControllerObj, objs);
                            if (md.ReturnType != typeof(void))
                            {
                                channel.BasicPublish(exchange: "", routingKey: properties.ReplyTo,
                                basicProperties: replyProerties, body: this.serializer.SerializeBytes(superMsg.setMsg(rep)));//ProtobufSerializer.SerializeBytes(mfs.rep,rep)
                            }
                            channel.BasicAck(ea.DeliveryTag, false);
                            // Console.WriteLine($"Return result: {"消息：" + message}");

                        };
                        channel.BasicConsume(queue: this.version+this.interfaceFullName + "." + md.Name, autoAck: false, consumer: consumer);
                        Console.WriteLine($"类名:{ t.Name}, {"方法名：" + md.Name},{"返回值" + ":" + md.ReturnType}");

                    }
                }
                #endregion
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public override  void GetMathsInfo<T>()
        {
            Type t = typeof(T);
            GetMathsInfo(t);
        }
        public override void GetMathsInfoMulti(Type t)
        {
            this.clear();
            //var t = Type.GetType(className);

            try
            {


                IConnection conection = factory.CreateConnection();
                #region 方法二
                MethodInfo[] info = t.GetMethods();
                List<Type> interfaces = t.GetInterfaces().Where(type => (!typeof(MsgController).IsAssignableFrom(type))).ToList();
                if (interfaces.Count > 1 || interfaces.Count == 0) throw new Exception("MsgController的实现类必须继承Sharing中共享的接口," +
                      "且不能继承MsgController外的其他接口,且不能多层继承");

                for (int k = 0; k < interfaces.Count; k++)
                {
                    ///获取接口和类信息
                    this.ControllerObj = Activator.CreateInstance(t);
                    this.packageName = t.Namespace;
                    this.FullName = t.FullName;
                    this.className = t.Name;
                    this.interfaceFullName = interfaces[k].FullName;
                    IEnumerable<CustomAttributeData> eFRpcServiceAttribute = t.CustomAttributes.Where(type => type.AttributeType == typeof(EFRpcServiceAttribute)).ToList();
                    foreach (var a in eFRpcServiceAttribute)
                    {
                        foreach (var kv in a.NamedArguments)
                        {
                            if (kv.MemberName.Equals("version"))
                            {
                                this.version = kv.TypedValue.Value.ToString();
                            }
                        }
                    }
                    for (int i = 0; i < info.Length; i++)
                    {
                        MsgFun mf = new MsgFun();
                        MethodInfo md = info[i];

                        //方法名
                        string mothodName = md.Name;
                        Console.WriteLine($"类名:{ t.Name}, {"方法名：" + md.Name}");
                        //参数集合
                        ParameterInfo[] paramInfos = md.GetParameters();
                        if (md.Name.Equals("ToString") || md.Name.Equals("Equals") || md.Name.Equals("GetHashCode") || md.Name.Equals("GetType")) continue;
                        //获取标识注解
                        MethodInfo interfaceMethod = null;
                        MethodInfo[] methodInfos = interfaces[k].GetMethods();
                        foreach (var method in methodInfos) {
                            if ((method.Name.Equals(md.Name)))
                            {
                                ParameterInfo[] parameterInfos= md.GetParameters();
                                if (method.GetParameters().Length == parameterInfos.Length)
                                {
                                    interfaceMethod = method;
                                    for (int index = 0; index < method.GetParameters().Length; index++)
                                    {
                                        if (!method.GetParameters()[index].GetType().Equals(parameterInfos[index].GetType()))
                                        {
                                            interfaceMethod = null;
                                            break;
                                        }
                                    }
                                    if (null != interfaceMethod) { break; }
                                }
                            }

                        }
                        
                            
                        
                        IEnumerable<CustomAttributeData> attributes = interfaceMethod.CustomAttributes.Where(Attribute => (!typeof(EFRpcMethodAttribute).Equals(Attribute.GetType())));
                        MsgFun mfs = new MsgFun();
                        mfs.Name = md.Name;
                        foreach (var a in attributes)
                        {
                            foreach (var kv in a.NamedArguments)
                            {
                                if (kv.MemberName.Equals("mark"))
                                {
                                    mfs.Name+= kv.TypedValue.Value.ToString();
                                }
                            }
                        }

                        mfs.rep = md.ReturnType;
                        mfs.methodInfo = md;
                        this.put(md.Name, mfs);
                        mfs.reqs = new Type[paramInfos.Length];
                        for (int j = 0; j < paramInfos.Length; j++)
                        {
                            ParameterInfo parameterInfo = paramInfos[j];
                            mfs.reqs[j] = parameterInfo.ParameterType;
                            Console.WriteLine($"类名:{ t.Name}, {"方法名：" + md.Name},{"入参" + j+1 + ":" + parameterInfo.ParameterType}");
                        }
                        mfs.FullName = version + "." + interfaceFullName + "." + mfs.Name;
                        IModel channel = conection.CreateModel();
                        channel.QueueDeclare(queue: mfs.FullName, durable: false,
                            exclusive: false, autoDelete: false, arguments: null);
                        var consumer = new EventingBasicConsumer(channel);
                        Console.WriteLine("[*] Waiting for message.");
                        consumer.Received += (model, ea) =>
                        {
                            var properties = ea.BasicProperties;
                            var replyProerties = channel.CreateBasicProperties();
                            replyProerties.CorrelationId = properties.CorrelationId;
                         
                            string s = ea.Body.ToString();
                            SuperMsgMulti superMsg = this.serializer.DeSerializeBytes<SuperMsgMulti>(ea.Body.ToArray());
                            //Console.WriteLine(superMsg.msg[0].ToString());
                            if (null != superMsg.msg&& superMsg.msg.Length>0)
                            {
                                object[] objs = new object[superMsg.msg.Length];
                                for (int j = 0; j < superMsg.msg.Length; j++)
                                {
                                    //mfs.reqs.Length
                                    objs[j] = this.serializer.DeSerializeString(mfs.reqs[j],superMsg.msg[j].ToString());
                                }
                                object rep = mfs.methodInfo.Invoke(this.ControllerObj, objs);
                                if (md.ReturnType != typeof(void))
                                {
                                    channel.BasicPublish(exchange: "efrpc", routingKey: properties.ReplyTo,
                                    basicProperties: replyProerties, body: this.serializer.SerializeBytes(superMsg.setReq(rep)));//ProtobufSerializer.SerializeBytes(mfs.rep,rep)
                                }
                            }
                            channel.BasicAck(ea.DeliveryTag, false);
                            // Console.WriteLine($"Return result: {"消息：" + message}");

                        };
                        channel.BasicConsume(queue: mfs.FullName, autoAck: false, consumer: consumer);
                        Console.WriteLine($"类名:{ t.Name}, {"方法名：" + md.Name},{"返回值" + ":" + md.ReturnType}");

                    }
                }
                #endregion
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public override void GetMathsInfoMulti<T>()
        {
            Type t = typeof(T);
            GetMathsInfo(t);
        }
       
    }
}
