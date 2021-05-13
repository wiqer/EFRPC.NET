using System;
using System.Collections.Generic;
using System.Text;

namespace EF.RPC.Impl.ConsumerImpl
{
    /// <summary>
    /// 在不断优化下会将MsgConsumerMap实现类中的通用部分放到这个类中
    /// </summary>
    public abstract class MsgConsumerMap:MsgControllersMap
    {
        public object ControllerObj { get; set; }
    }
}
