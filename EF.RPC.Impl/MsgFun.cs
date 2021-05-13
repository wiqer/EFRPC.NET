using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace EF.RPC.Impl
{
    public class MsgFun
    {
  
        public MethodInfo methodInfo { get; set; }
        public string packageName { get; set; }
        public string FullName { get; set; }
        public string ReqFullName { get; set; }
        public string className { get; set; }
        public string Name { get; set; }
        public Type rep { get; set; }
        public Type req { get; set; }
        public Type[] reqs { get; set; }
    }
}
