using EF.RPC.Impl.ProducerImpl;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace EF.RPC.Impl.ProducerImpl
{
    public abstract class MsgProducerMap : MsgControllersMap, InvocationHandlerInterface
    {
        public virtual object invoke(object proxy, MethodInfo method, object[] args)
        {
            throw new NotImplementedException();
        }
    }
}
