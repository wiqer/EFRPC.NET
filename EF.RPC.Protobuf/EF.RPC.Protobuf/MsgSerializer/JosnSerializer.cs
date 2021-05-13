using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace EF.RPC.Protobuf.MsgSerializer
{
    public class JosnSerializer : SerializerInterface
    {
        static JosnSerializer(){
            ins =new JosnSerializer();
        }
        public static JosnSerializer ins ;
        private JosnSerializer() { 
        
        }
        public static JosnSerializer getSerializer() {
         
            return ins;
        }
        public object DeSerializeBytes(Type type, string content)
        {
            return JsonConvert.DeserializeObject(content, type);
        }
        public T DeSerializeBytes<T>(byte[] content)
        {
            return JsonConvert.DeserializeObject<T>(Encoding.UTF8.GetString(content));
        }
        public T DeSerializeString<T>(string content)
        {
            return JsonConvert.DeserializeObject<T>(content);
        }

        public object DeSerializeBytes(Type type, byte[] content)
        {
           return JsonConvert.DeserializeObject(Encoding.UTF8.GetString(content), type);
          
        }
        public object DeSerializeString(Type type, string content)
        {
            return JsonConvert.DeserializeObject(content, type);
        }
        //public byte[] SerializeBytes<T>(T t)
        //{
        //    return SerializeBytes(t);
        //}

        public byte[] SerializeBytes(Type type, object t)
        {
            return SerializeBytes(t);
        }

        public byte[] SerializeBytes(object t)
        {
           return  Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(t));
        }

        public string SerializeString(object t)
        {
            return JsonConvert.SerializeObject(t);
        }

   
    }
}
