using System;
using System.Collections.Generic;
using System.Text;

namespace EF.RPC.Impl.annotation
{


    /// <summary>
    /// Attribute修饰的必须是public属性，否则反射获取不到属性
    /// 被注入的属性必须是是实例化的,一般单例常驻内存
    /// </summary>
    public sealed class EFRpcAutowiredAttribute:Attribute
    {      /// <summary>
           /// 版本号
           /// </summary>
        public string version { get; set; }
        /// <summary>
        /// auto
        /// syn
        /// asyn
        /// </summary>
        public string runMode { get; set; }
    }
}
