using System;
using System.Collections.Generic;
using System.Text;

namespace EF.RPC.Sharing
{

    public class GetMsgNumRequest
    {
        public GetMsgNumRequest() { }


           public int Num1 { get; set; }

        public int Num2 { get; set; }
        
    }

    //[Serializable]
    public class GetMsgSumReply
    {

        public int Sum { get; set; }
       
    }

    //[Serializable]
    public class GetMsgMulReply
    {
      
        public int Mul { get; set; }
       
    }

}
