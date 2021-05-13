using System;
using System.Collections.Generic;
using System.Text;

namespace EF.RPC.Impl.annotation
{
    /// <summary>
    /// sealed最终类，类似于Java中final关键字
    /// </summary>
    public sealed class EFRpcServiceAttribute:Attribute
    {
       
        /// <summary>
        /// 版本号
        /// </summary>
        public string version { get; set; }
       /// <summary>
       /// 类的类型
       /// </summary>
        public Type StrategyType { get; set; }
    }
}
