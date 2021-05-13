using EF.RPC.Impl.annotation;
using EF.RPC.Impl.ProducerImpl;
using EF.RPC.Protobuf;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace EF.RPC.Impl.RabbitMQImpl.ProducerImpl
{
    public class RabbitMQMsgProducerMap: MsgProducerMap
    {
        ConnectionFactory factory;
        public RabbitMQMsgProducerMap setOptions(RabbitMQOptions opt)
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
                ///获取接口和类信息
                this.interfaceFullName = t.FullName;

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
                    RabbitMQMsgFun mfs = new RabbitMQMsgFun();
                    mfs.Name = md.Name;
                    mfs.rep = md.ReturnType;
                    mfs.FullName= version + "." + interfaceFullName + "." + mfs.Name;
                    mfs.methodInfo = md;
                    mfs.ReqFullName = version + "." + FullName + "." + mfs.Name;
                       
                    this.put(md.Name, mfs);
                    for (int j = 0; j < paramInfos.Length; j++)
                    {
                        ParameterInfo parameterInfo = paramInfos[j];
                        mfs.req = parameterInfo.ParameterType;
                        Console.WriteLine($"类名:{ t.Name}, {"方法名：" + md.Name},{"入参" + j + ":" + parameterInfo.ParameterType}");
                    }
                  
                      
                    IConnection connection = factory.CreateConnection();

                    IModel channel = connection.CreateModel();
                    mfs.channel = channel;
                    String correlationId = mfs.ReqFullName;
                    // 创建一个临时队列, 返回队列的名字
                    String replyQueue = channel.QueueDeclare().QueueName;
                    IBasicProperties properties = channel.CreateBasicProperties();
                    properties.ReplyTo = replyQueue;
                    properties.CorrelationId = correlationId;
                    mfs.properties= properties;
                    //创建消费者用于消息回调
                    var callbackConsumer = new EventingBasicConsumer(channel);
                    //绑定临时队列的消费
                    channel.BasicConsume(queue: replyQueue, autoAck: true, consumer: callbackConsumer);

                    callbackConsumer.Received += (model, ea) =>
                    {
                        if (ea.BasicProperties.CorrelationId == correlationId)
                        {
                            //解码反回消息
                            SuperMsgMulti superMsg = this.serializer.DeSerializeString<SuperMsgMulti>(ea.Body.ToString());
                            var responseMsg = $"Get Response: {Encoding.UTF8.GetString(ea.Body.ToArray())}";
                            Console.WriteLine($"[x]: {responseMsg}");
                        }
                    };
                }
                #endregion
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public void GetClassInfo(Type t) {
            this.packageName = t.Namespace;
            this.FullName = t.FullName;
            this.className = t.Name;
        }
        public void GetClassInfo<T>()
        {
            Type t = typeof(T);
            GetClassInfo(t);
        }
        public override void GetMathsInfo<T>()
        {
            Type t = typeof(T);
            GetMathsInfo(t);
        }
        public override void GetMathsInfoMulti(Type interfaces)
        {
            this.clear();
            try
            {
                IConnection conection = factory.CreateConnection();
                ///获取接口和类信息
                MethodInfo[] info = interfaces.GetMethods();
                this.interfaceFullName = interfaces.FullName;
               
                for (int i = 0; i < info.Length; i++)
                {
                   
                    MethodInfo md = info[i];
        
                    //方法名
                    string mothodName = md.Name;

                    //参数集合
                    ParameterInfo[] paramInfos = md.GetParameters();
                    if (md.Name.Equals("ToString") || md.Name.Equals("Equals") || md.Name.Equals("GetHashCode") || md.Name.Equals("GetType")) continue;
                    //获取标识注解
                    IEnumerable<CustomAttributeData> attributes = md.CustomAttributes.Where(Attribute => (!typeof(EFRpcMethodAttribute).Equals(Attribute.GetType())));
                    RabbitMQMsgFun mfs = new RabbitMQMsgFun();
                    mfs.Name = md.Name;
                    foreach (var a in attributes)
                    {
                        foreach (var kv in a.NamedArguments)
                        {
                            if (kv.MemberName.Equals("mark"))
                            {
                                mfs.Name += kv.TypedValue.Value.ToString();
                            }
                        }
                    }
                    Console.WriteLine($"类名:{ this.className}, {"方法名：" + mfs.Name}");
                    mfs.rep = md.ReturnType;
                    mfs.FullName = version + "." + interfaceFullName + "." + mfs.Name;

                    mfs.methodInfo = md;
                    mfs.ReqFullName = version + "." + FullName + "." + mfs.Name;
                    this.put(md.Name, mfs);
                    for (int j = 0; j < paramInfos.Length; j++)
                    {
                        ParameterInfo parameterInfo = paramInfos[j];
                        mfs.req = parameterInfo.ParameterType;
                      
                    }
                    IConnection connection = factory.CreateConnection();

                    IModel channel = connection.CreateModel();
                    mfs.channel = channel;
                    if (md.ReturnType != typeof(void))
                    {
                        mfs.initUnsafeSynchronizer();
                        String correlationId = mfs.ReqFullName;
                        // 创建一个临时队列, 返回队列的名字
                        String replyQueue = channel.QueueDeclare().QueueName;

                        IBasicProperties properties = channel.CreateBasicProperties();
                        properties.ReplyTo = replyQueue;
                        properties.CorrelationId = correlationId;
                        mfs.properties = properties;
                        //创建消费者用于消息回调
                        var callbackConsumer = new EventingBasicConsumer(channel);
                        //绑定临时队列的消费
                        channel.BasicConsume(queue: replyQueue, autoAck: true, consumer: callbackConsumer);

                        callbackConsumer.Received += (model, ea) =>
                        {
                            if (ea.BasicProperties.CorrelationId == correlationId)
                            {
                            //解码反回消息
                                SuperMsgMulti superMsg = this.serializer.DeSerializeBytes<SuperMsgMulti>(ea.Body.ToArray());
                                mfs.setMsg(superMsg);
                                mfs.release(superMsg.Id);
                                //var responseMsg = $"Get Response: {Encoding.UTF8.GetString(ea.Body.ToArray())}";
                                //Console.WriteLine($"[x]: {responseMsg}");
                            }
                        };
                    }

                }
            
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
        public override object invoke(object proxy, MethodInfo method, object[] args)
        {
            SuperMsgMulti msg = new SuperMsgMulti(args);
           
            byte[] body =  this.serializer.SerializeBytes(msg);
            RabbitMQMsgFun rabbitMQMsgFun= (RabbitMQMsgFun)this.get(method.Name);
            if (rabbitMQMsgFun != null)
            {
                rabbitMQMsgFun.channel.BasicPublish(exchange: "", routingKey: rabbitMQMsgFun.FullName,
                    basicProperties: rabbitMQMsgFun.properties, body: body);
                if (method.ReturnType != typeof(void))
                {
                    rabbitMQMsgFun.acquire(msg.Id);
                    msg = rabbitMQMsgFun.getAndRemoveMsg(msg);
                    if (null == msg)
                    {
                        return Activator.CreateInstance(method.ReturnType);
                    }
                    else
                    {// this.serializer.DeSerializeString(method.ReturnType, new object().ToString());
                        return this.serializer.DeSerializeString(method.ReturnType, msg.req.ToString());
                    }
                }



            }
            else { 
                throw new NotImplementedException("未成功加载到方法,请仔细排查一下");
            }
          

            return Activator.CreateInstance(method.ReturnType);// this.serializer.DeSerializeString(method.ReturnType, new object().ToString());
        }

    }
}
