using System;
using System.Collections.Generic;
using System.Text;

namespace EF.RPC.Sharing
{
    /// <summary>
    /// 客户端和服务端最好使用同一个接口实现，方便编程
    /// 这里分开写是为了验证灵活性
    /// </summary>
    public interface IMsgServer
    {
        GetMsgSumReply GetSum(GetMsgNumRequest req);
        GetMsgMulReply GetMul(GetMsgNumRequest req);
        void Conversations();
    }
}
