using System;
using System.Collections.Generic;
using System.Text;

namespace EF.RPC.Impl
{
    public abstract class MsgMathsInfoFactory<MsgMathsInfoMap> where MsgMathsInfoMap : new()
    {

        public virtual MsgMathsInfoMap getMsgMathsInfoMap()
        {
            return new MsgMathsInfoMap();
        }
    }
}
