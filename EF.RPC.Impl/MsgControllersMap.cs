using EF.RPC.Protobuf;
using EF.RPC.Protobuf.MsgSerializer;
using Newtonsoft.Json;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Reflection;
using System.Text;

namespace EF.RPC.Impl
{
    /// <summary>
    /// 共享接口容器
    /// </summary>
    public abstract class MsgControllersMap: MsgMathsInfoMap
    {
        public SerializerInterface serializer  { get; set; }
      
        public string packageName { get; set; }
        public string FullName { get; set; }
        public string interfaceFullName { get; set; }

        public string version { get; set; } = "";

        public string className { get; set; }
        public LinkMap<string , MsgFun> linkMap;
        public MsgControllersMap() {
            serializer = JosnSerializer.getSerializer();
            linkMap = new LinkMap<string, MsgFun>();
        }
        public virtual MsgControllersMap setOptions(Options opt)
        {
            return this;
        }
        
        public MsgFun get(string key)
        {
            return linkMap.get(key);
        }

        public bool isEmpty()
        {
            return linkMap.isEmpty();
        }

        public Collection<string> keys()
        {
            return linkMap.keys();
        }

        public MsgFun put(string key, MsgFun value)
        {
            return linkMap.put(key,  value);
        }
        public int Size()
        {
            return linkMap.Size();
        }

        public Collection<MsgFun> values()
        {
            return linkMap.values();
        }
        public void clear()
        {
            linkMap.clear();
        }
        public virtual void GetMathsInfo<T>()
        {

            throw new NotImplementedException();
        }
        public virtual void GetMathsInfo(Type t) {
            throw new NotImplementedException();
        }

        public virtual void GetMathsInfoMulti<T>()
        {
            throw new NotImplementedException();
        }

        public virtual void GetMathsInfoMulti(Type t)
        {
            throw new NotImplementedException();
        }
    }
   
}
