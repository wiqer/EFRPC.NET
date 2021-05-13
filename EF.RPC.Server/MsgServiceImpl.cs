using EF.RPC.Impl;
using EF.RPC.Impl.annotation;
using EF.RPC.Protobuf;
using EF.RPC.Sharing;
using System;
using System.Collections.Generic;
using System.Text;

namespace EF.RPC.Server
{
    [EFRpcService(version ="v1")]
    public class MsgServiceImpl: IMsgServer, MsgController
    {
        public  GetMsgSumReply GetSum(GetMsgNumRequest request)
        {
          
                var result = new GetMsgSumReply();

                result.Sum = request.Num1 + request.Num2;

                Console.WriteLine(request.Num1 + "+" + request.Num2 + "=" + result.Sum);

                return result;
           

        }
        public  GetMsgMulReply GetMul(GetMsgNumRequest request)
        {
          
                var result = new GetMsgMulReply();

                result.Mul = request.Num1 * request.Num2;

                Console.WriteLine(request.Num1 + "*" + request.Num2 + "=" + result.Mul);

                return result;
           

        }

        public void Conversations()
        {
            throw new NotImplementedException();
        }
       
        public void Conversations(int i)
        {
            throw new NotImplementedException();
        }
    }
}
