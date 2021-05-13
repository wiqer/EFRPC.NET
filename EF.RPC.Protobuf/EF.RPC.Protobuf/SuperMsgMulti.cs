using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace EF.RPC.Protobuf
{
    public class SuperMsgMulti : BaseMsg
    {
        [JsonProperty]
        public object[] msg { get; private set; }
        [JsonProperty]
        public object req { get; private set; }
        public SuperMsgMulti() : base()
        {
        }
        public SuperMsgMulti(object[] msg) : this()
        {
            this.msg = msg;
        }
        public SuperMsgMulti setMsg(object[] msg)
        {
            this.msg = msg;
            return this;
        }
        public SuperMsgMulti setReq(object req)
        {
            this.msg = null;
            this.req = req;
            return this;
        }
        [JsonConstructor]
        public SuperMsgMulti(string id, DateTime createDate) : base(id, createDate)
        {
        }
    }
}
