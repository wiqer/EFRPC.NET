using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using EF.RPC.Protobuf;
using EF.RPC.Sharing;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using EF.RPC.Impl.annotation;
using System.Reflection;
using EF.RPC.Impl;
using EF.RPC.Impl.ProducerImpl;
using EF.RPC.Impl.RabbitMQImpl.ProducerImpl;
using EF.RPC.Impl.RabbitMQImpl;

namespace EF.RPC.Client
{
    class Program
    {
        static void Main(string[] args)
        {
            //fun1(args);
            //Console.ReadLine();
            MsgClientImpl msgClientImpl= new ProducerBootstrap<Program>().start(new RabbitMQOptionsFactory<RabbitMQMsgProducerMap>()).getController<MsgClientImpl>();

            // msgClientImpl.GetSum(1, 2);
            //            MsgConsumerFactory<RabbitMQMsgProducerMap> con;
            //            List<Type> Producers = typeof(Program).Assembly.GetTypes().AsEnumerable()
            //.Where(type => (typeof(MsgController).IsAssignableFrom(type) && type.IsClass && !type.IsAbstract)).ToList();
            //            for (int i=0; i < Producers.Count; i++) {
            //                new RabbitMQMsgProducerMap().GetMathsInfo(Producers[i]);
            //            }
            for (int i = 0; i < 10000; i++)
            {
                msgClientImpl.GetSum(1, i);
                msgClientImpl.GetMul(i, i - 5);
            }
            Console.ReadLine();
            Thread t3 = new Thread((mmm) =>
            {
                for (int i = 0; i < 1000; i++) {
                    msgClientImpl.GetSum(1, i);
                   
                }
                // Console.WriteLine(msgClientImpl.GetSum(1, i) + "");
            });
   
            Thread t4 = new Thread((mmm) =>
            {
                for (int i = 0; i < 1000; i++)
                {
                    msgClientImpl.GetMul(i, i - 5);

                }
                // Console.WriteLine(msgClientImpl.GetSum(1, i) + "");
            });
            t4.Start();
            Console.ReadLine();
            for (int i = 0; i < 1000; i++)
            {
                Thread t = new Thread((mmm) =>
                {
                    msgClientImpl.GetSum(1, i);
                   // Console.WriteLine(msgClientImpl.GetSum(1, i) + "");
                });
              
                t.Start();
                //Thread.Sleep(2000);

                //Thread.Sleep(1000);   
                Thread t1 = new Thread((mmm) =>
                {
                    msgClientImpl.GetMul(i, i - 5);
                    //Console.WriteLine(msgClientImpl.GetMul(i, i - 5) + "");
                });

            t1.Start();
           
            }
            Console.WriteLine("ok");
            Console.ReadLine();
            // DynamicProxyFactory.createProxyByInterface<myInvocationHandlerInterface>(new II()).m("hello w" , "hello e" );

            //Example1.Run();
            //daili();
            // fun1(args);
           // Console.ReadLine();
        }
        static void daili() {
           
           //取到所有类
           List<Type> Producers = typeof(Program).Assembly.GetTypes().AsEnumerable()
.Where(type => (typeof(IMsgClient).IsAssignableFrom(type) && type.IsClass && !type.IsAbstract)).ToList();

            for (int i = 0; i < Producers.Count; i++)
            {
                // FieldInfo[] GetFields
                //取到所有类的指定方法
                IEnumerable<FieldInfo> fieldInfos = Producers[i].GetFields().Where(type => type.GetCustomAttributes(true).Any(e => e as EFRpcAutowiredAttribute != null));

                //PropertyInfo[] fieldInfos3 = Producers[i].GetProperties();
                //PropertyInfo propertyInfo = Producers[i].GetProperty("ServiceAutoUpdate");
                // Type[] ts = Producers[i].GetGenericArguments();//.Where(type=>type.GetCustomAttributes()));
                //foreach (var a in ts)
                //{
                //    IEnumerable<Attribute> attributes= a.GetCustomAttributes();
                //    foreach (var b in attributes)
                //    {

                //    }
                //}
                //Type[] ts2 = Producers[i].GetGenericParameterConstraints();//GetGenericTypeDefinition();
                IEnumerable<FieldInfo> fieldInfos2 = Producers[i].GetFields();
                /// Type[] ts2 = Producers[i].GetGenericArguments();//.Where(type=>type.GetCustomAttributes()));
                IEnumerable<FieldInfo> fieldInfos3 = Producers[i].GetFields().ToList().Where(t => t.GetCustomAttributes().Where(type => type.GetType().Equals(typeof(EFRpcAutowiredAttribute))) == null);
                IEnumerable<FieldInfo> fieldInfos4 = Producers[i].GetFields().ToList().Where(t => t.CustomAttributes.Where(type => type.AttributeType == typeof(EFRpcAutowiredAttribute)).ToArray().Length > 0);
                foreach (var a in fieldInfos2)
                {
                    IEnumerable<CustomAttributeData> attributes3 = a.CustomAttributes;//.Where(type => type.AttributeType == typeof(EFRpcAutowiredAttribute));
                    IEnumerable<Attribute> attributes2 = a.GetCustomAttributes().Where(type => type.GetType().Equals(typeof(EFRpcAutowiredAttribute)));
                    foreach (var b in attributes3)
                    {
                        //foreach (var kv in b.)
                        //{
                        //    if (kv.MemberName.Equals("version"))
                        //    {
                        //        this.version = kv.TypedValue.Value.ToString();
                        //    }
                        //}
                        if (b.AttributeType == typeof(EFRpcAutowiredAttribute))
                        {
                            string version = "";
                            foreach (var kv in b.NamedArguments)
                            {
                                if (kv.MemberName.Equals("version"))
                                {
                                    version = kv.TypedValue.Value.ToString();
                                }
                            }
                            //a.SetValue(a.GetType(), ProxyFactory.createService(a, version));

                        }

                    }
                    //foreach (var b in attributes2)
                    //{
                    //    //foreach (var kv in b.)
                    //    //{
                    //    //    if (kv.MemberName.Equals("version"))
                    //    //    {
                    //    //        this.version = kv.TypedValue.Value.ToString();
                    //    //    }
                    //    //}
                    //    a.SetValue(a.GetType(),ProxyFactory.createService(a, "0"));
                    //}
                }
                // Producers[i].DeclaredFields = { System.Reflection.FieldInfo[1]}

                //foreach (var a in fieldInfos2)
                //{

                //}
            }
        }
        static void deftFun(string[] args)
        {
            var factory = new ConnectionFactory() { HostName = "localhost" };
            using (var connection = factory.CreateConnection())
            {
                using (var channel = connection.CreateModel())
                {
                    var correlationId = Guid.NewGuid().ToString();
                    var replyQueue = channel.QueueDeclare().QueueName;

                    var properties = channel.CreateBasicProperties();
                    properties.ReplyTo = replyQueue;
                    properties.CorrelationId = correlationId;

                    string number = args.Length > 0 ? args[0] : "30";
                    var body = Encoding.UTF8.GetBytes(number);
                    //发布消息
                    channel.BasicPublish(exchange: "", routingKey: "rpc_queue", basicProperties: properties, body: body);

                    Console.WriteLine($"[*] Request fib({number})");

                    // //创建消费者用于消息回调
                    var callbackConsumer = new EventingBasicConsumer(channel);
                    channel.BasicConsume(queue: replyQueue, autoAck: true, consumer: callbackConsumer);

                    callbackConsumer.Received += (model, ea) =>
                    {
                        if (ea.BasicProperties.CorrelationId == correlationId)
                        {
                            var responseMsg = $"Get Response: {Encoding.UTF8.GetString(ea.Body.ToArray())}";

                            Console.WriteLine($"[x]: {responseMsg}");
                        }
                    };
                  

                    Console.ReadLine();

                }
            }

        }
        static void fun1(string[] args)
        {
            string rpc_queue = "v1.EF.RPC.Sharing.IMsgServer.GetSum";
               var factory = new ConnectionFactory() { HostName = "localhost" };
            using (var connection = factory.CreateConnection())
            {
                using (var channel = connection.CreateModel())
                {
                    String correlationId = Guid.NewGuid().ToString();
                    // 创建一个临时队列, 返回队列的名字
                    String replyQueue = channel.QueueDeclare().QueueName;

                    IBasicProperties properties = channel.CreateBasicProperties();
                    properties.ReplyTo = replyQueue;
                    properties.CorrelationId = correlationId;

                    string number = args.Length > 0 ? args[0] : "30";
                    var body = Encoding.UTF8.GetBytes(number);
                    //发布消息
                    //channel.BasicPublish(exchange: "", routingKey: "rpc_queue", basicProperties: properties, body: body);

                    Console.WriteLine($"[*] Request fib({number})");

                    // //创建消费者用于消息回调
                    var callbackConsumer = new EventingBasicConsumer(channel);
                    //绑定临时队列的消费
                    channel.BasicConsume(queue: replyQueue, autoAck: true, consumer: callbackConsumer);

                    callbackConsumer.Received += (model, ea) =>
                    {
                        if (ea.BasicProperties.CorrelationId == correlationId)
                        {
                            //var responseMsg = $"Get Response: {Encoding.UTF8.GetString(ea.Body.ToArray())}";

                            //Console.WriteLine($"[x]: {responseMsg}");
                        }
                    };
                    for (int i = 0; i < 10000; i++)
                    {
                        //Thread.Sleep(200);
                        SuperMsgMulti superMsgMulti = new SuperMsgMulti();
                        GetMsgNumRequest msgNumRequest = new GetMsgNumRequest();
                        msgNumRequest.Num1 = i;
                        msgNumRequest.Num2 = 99;
                        superMsgMulti.setMsg(new object[] { msgNumRequest });
                        body = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(superMsgMulti));
                        //body = Encoding.UTF8.GetBytes($"{ "msg":[{ \"Num1\":1,\"Num2\":398}],\"req\":null,\"Id\":\"b48a9ab2-2813-4868-a432-0f87606c8db0\",\"CreationDate\":\"2021-03-04T18:51:48.8286043+08:00\"}");//ProtobufSerializer.SerializeBytes< GetMsgNumRequest>( msgNumRequest);// Encoding.UTF8.GetBytes(msgNumRequest.ToString());
                        //发布消息
                        //默认路由，发布的制定rpc_queue队列
                        //配置参数带上临时队列的名字
                        channel.BasicPublish(exchange: "", routingKey: rpc_queue, basicProperties: properties, body: body);
                        msgNumRequest.Num1 = i-8;
                        msgNumRequest.Num2 = 9;
                        superMsgMulti.setMsg(new object[] { msgNumRequest });
                        body = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(superMsgMulti));
                        channel.BasicPublish(exchange: "", routingKey: rpc_queue, basicProperties: properties, body: body);

                    }
                    Console.ReadLine();

                }
            }
        }
    }
}
