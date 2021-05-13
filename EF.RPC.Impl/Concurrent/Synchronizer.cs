using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace EF.RPC.Impl.Concurrent
{
    public abstract class Synchronizer
    {
        protected Thread t;
        protected int sleeptime =0;
        public Synchronizer() {
             t = Thread.CurrentThread;
        }
        public Synchronizer(int i) : this()
        {
            sleeptime = i;
           
        }
        public  void acquire()
        {
            if (tryAcquire()) {
                try
                {
                    if (sleeptime > 0) { Thread.Sleep(sleeptime); }
                    else
                    {
                        ///永久堵塞
                        Thread.Sleep(Timeout.Infinite);
                    }
                }
                catch (ThreadInterruptedException)
                {
                    //被中断消息唤醒
                } 
            }
        }
        public  bool release()
        {
            if (tryRelease())
            {
                unparkSuccessor(t);
                return true;
            }
            return false;
        }

        private void unparkSuccessor(Thread t)
        {
            if (t.ThreadState == ThreadState.WaitSleepJoin) {
                t.Interrupt();
            }
        }
        //<exception cref = "Exception" >1111</ exception >
        public abstract bool tryRelease();
        public abstract bool tryAcquire();
    }
    //[Obsolete("逻辑不安全的同步器,可能造成线程锁死,或者未达到预期时唤醒", false)]
    public class UnsafeSynchronizer : Synchronizer
    {
        public UnsafeSynchronizer() : base()
        {
        }
        public UnsafeSynchronizer(int i):base(i)
        {
        }
        public override bool tryAcquire()
        {
            return t.ThreadState == ThreadState.Running|| t.ThreadState == ThreadState.Background;
        }

        public override bool tryRelease()
        {
            return null!=t&&t.ThreadState == ThreadState.WaitSleepJoin;
        }
    }


}
