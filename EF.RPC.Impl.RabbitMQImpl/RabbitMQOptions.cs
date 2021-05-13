using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Text;

namespace EF.RPC.Impl.RabbitMQImpl
{
    public class RabbitMQOptions: Options 
    {
        public ConnectionFactory factory { get; set; }
    }
}
