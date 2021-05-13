using System;
using System.Collections.Generic;
using System.Text;

namespace EF.RPC.Protobuf.MsgSerializer
{
    public interface SerializerInterface
    {
       

        /// <summary>
        /// 序列化
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="t"></param>
        /// <returns></returns>
          byte[] SerializeBytes(Type type, object t);
        byte[] SerializeBytes(object t);
       string SerializeString(object t);
        /// <summary>
        /// 反序列化
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="content"></param>
        /// <returns></returns>
        T DeSerializeBytes<T>(byte[] content);

        /// <summary>
        /// 反序列化
        /// </summary>

        /// <param name="content"></param>
        /// <returns></returns>
        object DeSerializeBytes(Type type, byte[] content);
        object DeSerializeBytes(Type type, string content);
        T DeSerializeString<T>(string content);
        object DeSerializeString(Type type, string content);
    }
}
