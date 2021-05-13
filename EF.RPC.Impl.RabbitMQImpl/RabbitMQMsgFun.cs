using EF.RPC.Impl.Concurrent;
using EF.RPC.Protobuf;
using RabbitMQ.Client;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace EF.RPC.Impl.RabbitMQImpl
{
    public class RabbitMQMsgFun: MsgFun
    {
        public IModel channel { get; set; }
        public IBasicProperties properties { get; set; }
        //分散储存减少压力
        
        public ConcurrentDictionary<string, UnsafeSynchronizer> uscd;//= new ConcurrentDictionary<int, UnsafeSynchronizer>();
        public ConcurrentDictionary<string, SuperMsgMulti> msgcd;//= new ConcurrentDictionary<int, UnsafeSynchronizer>();

        public void initUnsafeSynchronizer() {
            uscd = new ConcurrentDictionary<string, UnsafeSynchronizer>();
            msgcd = new ConcurrentDictionary<string, SuperMsgMulti>();
        }
        public void setMsg(SuperMsgMulti superMsgMulti) {
            msgcd.GetOrAdd(superMsgMulti.Id, superMsgMulti);
        }
        public SuperMsgMulti getAndRemoveMsg( SuperMsgMulti superMsgMulti)
        {
            SuperMsgMulti smm;
            msgcd.TryGetValue(superMsgMulti.Id,out smm);
            msgcd.TryRemove(superMsgMulti.Id, out smm);
            return smm;
        }
        public void acquire(string id) {
            UnsafeSynchronizer unsafeSynchronizer = new UnsafeSynchronizer(10000);
            uscd.GetOrAdd(id, unsafeSynchronizer).acquire();
        }
        public bool release(string id) {
            UnsafeSynchronizer unsafeSynchronizer;
            try
            {
              
                uscd.TryGetValue(id, out unsafeSynchronizer);
                if (null != unsafeSynchronizer)
                {
                    return unsafeSynchronizer.release();

                }
                else
                {
                    return false;
                }
            }
            finally {
                uscd.TryRemove(id,out unsafeSynchronizer);
            }
        }
    }
}
