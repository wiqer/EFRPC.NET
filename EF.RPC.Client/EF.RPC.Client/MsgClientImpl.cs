using EF.RPC.Impl;
using EF.RPC.Impl.annotation;
using EF.RPC.Sharing;
using System;
using System.Collections.Generic;
using System.Text;

namespace EF.RPC.Client
{
    public class MsgClientImpl : IMsgClient, MsgController
    {
        [EFRpcAutowired(version = "v1", runMode="Auto")]
        public IMsgServer ServiceAutoUpdate;
        //[EFRpcAutowired(version = "AutoUpdate", runMode = "Auto")]
        //public IMsgServer ServiceAutoUpdate2;
        //[EFRpcAutowired(version = "AutoUpdate", runMode = "Auto")]
        public IMsgServer ServiceAutoUpdate3;
        public void Conversations()
        {
            
            ServiceAutoUpdate.Conversations();
        }
       
        public  int GetMul(int num1, int num2)
        {
          
           return ServiceAutoUpdate.GetMul(new GetMsgNumRequest() { Num1 = num1, Num2 = num2 }).Mul;
        }
       
        public int GetSum(int num1, int num2)
        {
            return ServiceAutoUpdate.GetSum(new GetMsgNumRequest() { Num1 = num1, Num2 = num2 }).Sum;

        }
        public void Conversations(int i)
        {
            ServiceAutoUpdate.Conversations(i);
        }
    }
}
