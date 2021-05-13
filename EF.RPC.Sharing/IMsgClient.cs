using System;
using System.Collections.Generic;
using System.Text;

namespace EF.RPC.Sharing
{
    public interface IMsgClient
    {
        int GetSum(int num1, int num2);
        int GetMul(int num1, int num2);
        void Conversations();

        void Conversations(int i);
    }
}

