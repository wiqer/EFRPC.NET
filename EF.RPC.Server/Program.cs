
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using EF.RPC.Impl;
using EF.RPC.Impl.RabbitMQImpl;
using EF.RPC.Impl.WebSocketImpl;
using EF.RPC.Protobuf;
using EF.RPC.Sharing;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace EF.RPC.Server
{
    class Program
    {
      
        static void Main(string[] args) {

           //new WebSocketConsumerMap().GetMathsInfo<MsgServiceImpl>();
           new ConsumerBootstrap<Program>().start(new RabbitMQOptionsFactory<RabbitMQMsgConsumerMap>());

            //            MsgConsumerFactory<RabbitMQMsgProducerMap> con;
            //            List<Type> Producers = typeof(Program).Assembly.GetTypes().AsEnumerable()
            //.Where(type => (typeof(MsgController).IsAssignableFrom(type) && type.IsClass && !type.IsAbstract)).ToList();
            //            for (int i=0; i < Producers.Count; i++) {
            //                new RabbitMQMsgProducerMap().GetMathsInfo(Producers[i]);
            //            }
            Console.ReadLine();
           

        }
        static void MainRabbitMQ(string[] args)
        {
            var factory = new ConnectionFactory() { HostName = "localhost" };
            using (var conection = factory.CreateConnection())
            {
                using (var channel = conection.CreateModel())
                {
                    channel.QueueDeclare(queue: "rpc_queue", durable: false,
                        exclusive: false, autoDelete: false, arguments: null);

                    var consumer = new EventingBasicConsumer(channel);
                    Console.WriteLine("[*] Waiting for message.");

                    consumer.Received += (model, ea) =>
                    {
                        
                        //GetMsgNumRequest. message
                        //int n = int.Parse(message);
                        // Console.WriteLine($"Receive request of Fib({n})");
                        //int result = n;
                        //GetMsgNumRequest getMsgNumRequest = (GetMsgNumRequest)message;
                        var properties = ea.BasicProperties;
                        var replyProerties = channel.CreateBasicProperties();
                        replyProerties.CorrelationId = properties.CorrelationId;

                        var message = Encoding.UTF8.GetString(ea.Body.ToArray());
                          // var t = typeof(Program);
                        //获取所有方法 
                        //System.Reflection.MethodInfo[] methods = t.GetMethods();
                        channel.BasicPublish(exchange: "", routingKey: properties.ReplyTo,
                            basicProperties: replyProerties, body: Encoding.UTF8.GetBytes("" + message.ToString()));

                        channel.BasicAck(ea.DeliveryTag, false);
                        Console.WriteLine($"Return result: {"消息：" + message}");

                    };
                    channel.BasicConsume(queue: "rpc_queue", autoAck: false, consumer: consumer);

                    Console.ReadLine();
                }
            }

        }

        private static int Fib(int n)
        {
            if (n == 0 || n == 1)
            {
                return n;
            }
            return Fib(n - 1) + Fib(n - 2);
        }
        private static void MyGetMaths<T>()
        {
         
            //传递参数
          
            var t = typeof(T);
            //var t = Type.GetType(className);
            object obj = Activator.CreateInstance(t);

            try
            {
             

                #region 方法二
                MethodInfo[] info = t.GetMethods();
                for (int i = 0; i < info.Length; i++)
                {
                    MsgFun mf = new MsgFun();
                    MethodInfo md = info[i];
                    
                    //方法名
                    string mothodName = md.Name;
                    Console.WriteLine($"类名:{ t.Name}, {"方法名：" + md.Name}");
                    //参数集合
                    ParameterInfo[] paramInfos = md.GetParameters();
                    for (int j = 0; j < paramInfos.Length;j++) {
                        ParameterInfo parameterInfo = paramInfos[j];
                        MsgFun mfs = new MsgFun();
                        mfs.packageName = t.Namespace;
                        mfs.FullName = t.FullName;
                        mfs.className = t.Name;
                        mfs.Name = md.Name;
                        mfs.rep = md.ReturnType;
                        mfs.req = parameterInfo.ParameterType;
                        var objj= Activator.CreateInstance(mfs.req);
                     
                        Console.WriteLine($"类名:{ t.Name}, {"方法名：" + md.Name},{"入参"+j+":"+ parameterInfo.ParameterType}");
                    }
                    
                    Console.WriteLine($"类名:{ t.Name}, {"方法名：" + md.Name},{"返回值"  + ":" + md. ReturnType}");
                    //md.
                    //for (int j = 0; j < info.Length; j++)
                    //{
                    //    ParameterInfo parameterInfo = paramInfos[j];
                    //}
                    //方法名相同且参数个数一样

                }
                #endregion
            }
            catch (Exception ex)
            {
                throw ex;
            }
            Console.ReadKey();
        }
        private GetMsgSumReply sum(GetMsgNumRequest getMsgNumRequest) {
            return null;
        }
    }
}