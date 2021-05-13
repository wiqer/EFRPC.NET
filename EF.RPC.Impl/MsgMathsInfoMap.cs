using System;
using System.Collections.Generic;
using System.Text;

namespace EF.RPC.Impl
{
    public interface MsgMathsInfoMap
    {
        void GetMathsInfo<T>();
        void GetMathsInfo(Type t);
        void GetMathsInfoMulti<T>();
        void GetMathsInfoMulti(Type t);
    }
}
