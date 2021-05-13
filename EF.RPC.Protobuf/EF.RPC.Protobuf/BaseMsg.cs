using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace EF.RPC.Protobuf
{
    public class BaseMsg
    {
        public BaseMsg()
        {
            Id = System.Guid.NewGuid().ToString();
            CreationDate = DateTime.Now;
        }
        [JsonConstructor]
        public BaseMsg(string id, DateTime createDate)
        {
            Id = id;
            CreationDate = createDate;
        }
        [JsonProperty]
        public string Id { get; private set; }
        [JsonProperty]
        public DateTime CreationDate { get; private set; }
    }
}
