
using EF.RPC.Impl.annotation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EF.RPC.Impl.RabbitMQImpl
{
    public class ConsumerBootstrap<T>
    {
       
        List<RabbitMQMsgConsumerMap> msgConsumerMaplist  = new List<RabbitMQMsgConsumerMap>();
        List<Type> Producers;
        /// <summary>
        /// 对T下所有实现标注 EFRpcServiceAttribute的所有类进行代理
        /// </summary>
        /// <param name="mcf"></param>
        /// <returns></returns>
        public ConsumerBootstrap<T> start(RabbitMQOptionsFactory<RabbitMQMsgConsumerMap> mcf) {
           
             Producers = typeof(T).Assembly.GetTypes().AsEnumerable()
.Where(type => (typeof(MsgController).IsAssignableFrom(type) && type.IsClass && !type.IsAbstract && type.GetCustomAttributes(true).Any(e => (e as EFRpcServiceAttribute != null)))).ToList();
            for (int i = 0; i < Producers.Count; i++)
            {


                RabbitMQMsgConsumerMap rabbitMQMsgProducer = mcf.getMsgMathsInfoMap().setOptions(mcf.opt);
                msgConsumerMaplist.Add(rabbitMQMsgProducer);
                rabbitMQMsgProducer.GetMathsInfoMulti(Producers[i]);
                // msgConsumerMaplist.Add(m);

            }
            return this;
        }
    }
}
