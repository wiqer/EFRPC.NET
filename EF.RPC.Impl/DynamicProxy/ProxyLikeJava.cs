using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace EF.RPC.Impl.ProducerImpl
{

    /// <summary>
    /// 对应java的InvocationHandler接口
    /// 使用上应该是差不多的
    /// 注意点是，因为C#的Property的getset也是走这一个方法的
    /// 对于接口来说是全部代理，但是对于类只有虚/抽象方法代理
    /// </summary>
    public interface InvocationHandlerInterface
    {
        object invoke(object proxy, MethodInfo method, object[] args);
    }

}
