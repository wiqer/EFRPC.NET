using System;
using System.Collections.Generic;
using System.Text;

namespace EF.RPC.Impl.annotation
{
    public class EFRpcMethodAttribute : Attribute
    {
        /// <summary>
        /// 方法唯一标识符
        /// </summary>
        public string mark { get; set; }
     
    }
}
