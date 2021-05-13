using System;
using System.Collections.Generic;
using System.Text;

namespace EF.RPC.Impl.ProducerImpl.Interceptor
{
    /// <summary>
    /// 调用器
    /// </summary>
    public class Invocation
    {
        public object[] Parameter { get; set; }
        public Delegate DelegateMethod { get; set; }
        public object Proceed()
        {
            return this.DelegateMethod.DynamicInvoke(Parameter);
        }
    }

}
