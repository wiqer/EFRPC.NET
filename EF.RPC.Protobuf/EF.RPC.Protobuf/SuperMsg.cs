using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace EF.RPC.Protobuf
{
    public class SuperMsg: BaseMsg
    {
        [JsonProperty]
        public object msg { get; private set; }
        public SuperMsg() : base()
        {
        }
        public SuperMsg(object msg):this()
        {
            this.msg = msg;
        }
        public SuperMsg setMsg(object msg) 
        {
            this.msg = msg;
            return this;
        }
        [JsonConstructor]
        public SuperMsg(string id, DateTime createDate):base(id, createDate)
        {
        }
    }
}
