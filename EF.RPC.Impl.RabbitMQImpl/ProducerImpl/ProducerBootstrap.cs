using EF.RPC.Impl.annotation;
using EF.RPC.Impl.ProducerImpl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace EF.RPC.Impl.RabbitMQImpl.ProducerImpl
{
    public class ProducerBootstrap<T>
    {

        List<RabbitMQMsgProducerMap> msgConsumerMaplist = new List<RabbitMQMsgProducerMap>();
        public LinkMap<string, object> proxyObjectMap = new LinkMap<string, object>();
        /// <summary>
        /// 对标注  的所有属性进行代理
        /// </summary>
        /// <param name="mcf"></param>
        /// <returns></returns>
        public ProducerBootstrap<T> start(RabbitMQOptionsFactory<RabbitMQMsgProducerMap> mcf)
        {
            //精确到MsgController节省启动时间
            List<Type> Producers = typeof(T).Assembly.GetTypes().AsEnumerable()
.Where(type => (typeof(MsgController).IsAssignableFrom(type) && type.IsClass && !type.IsAbstract)).ToList();
            ///类遍历
            for (int i = 0; i < Producers.Count; i++)
            {
                //取到所有类的指定方法
                //没找到Where条件找不到注解的原因所以要分开写
                IEnumerable<FieldInfo> fieldInfos = Producers[i].GetFields();
                //属性便利
                foreach (var fieldinfo in fieldInfos)
                {
                    IEnumerable<CustomAttributeData> attributes = fieldinfo.CustomAttributes;//.Where(type => type.AttributeType == typeof(EFRpcAutowiredAttribute));
                    ///注解遍历
                    foreach (var ab in attributes)
                    {
                        //加载到EFRpcAutowiredAttribute注入并跳出循环
                        if (ab.AttributeType == typeof(EFRpcAutowiredAttribute))
                        {
                            EFRpcAutowiredAttribute autowiredAttribute= new EFRpcAutowiredAttribute();
                           //遍历注解信息
                            foreach (var kv in ab.NamedArguments)
                            {
                                if (kv.MemberName.Equals("version"))
                                {
                                    autowiredAttribute.version = kv.TypedValue.Value.ToString();
                                } else if (kv.MemberName.Equals("runMode"))
                                {
                                    autowiredAttribute.runMode = kv.TypedValue.Value.ToString();
                                }
                                
                            }
                            RabbitMQMsgProducerMap rabbitMQMsgProducer = mcf.getMsgMathsInfoMap().setOptions(mcf.opt);
                            rabbitMQMsgProducer.version = autowiredAttribute.version;
                            rabbitMQMsgProducer.GetClassInfo(Producers[i]);
                            rabbitMQMsgProducer.GetMathsInfoMulti(fieldinfo.FieldType);
                            //创建原对象
                            object obj =Activator.CreateInstance(Producers[i]);
                            //将代理对象注入到属性
                            fieldinfo.SetValue(obj, DynamicProxyFactory.createProxyByInterface(fieldinfo.FieldType, rabbitMQMsgProducer));
                           
                            proxyObjectMap.put(Producers[i].Name, obj);
                            msgConsumerMaplist.Add(rabbitMQMsgProducer);
                            break;
                        }

                    }

                }
            }
            return this;
        }
        //这叫享元模式吗？
        public O getController<O>()
        {
            return (O)proxyObjectMap.get(typeof(O).Name);
        }
        
    }
}
