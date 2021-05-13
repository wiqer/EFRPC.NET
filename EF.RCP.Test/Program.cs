using EF.RPC.Impl;
using EF.RPC.Impl.Concurrent;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace EF.RCP.Test
{

    class Program
    {
        public class ThreadWorkItem
        {
            public int ThreadManagerId { get; set; }
            public Thread Thread { get; set; }
            public string ThreadName { get; set; }
            public bool Flag { get; set; }
            public ManualResetEvent ManualResetEvent { get; set; }

        }
        static List<ThreadWorkItem> Works = new List<ThreadWorkItem>();
        static void Main(string[] args)
        {
            fun7();

            //fun3();

            Console.ReadLine();
            

        }
        static void fun7()
        {
            UnsafeSynchronizer unsafeSynchronizer = new UnsafeSynchronizer();
            
            ConcurrentDictionary<int, UnsafeSynchronizer> nscd = new ConcurrentDictionary<int, UnsafeSynchronizer>();
            for (int i = 0; i < 3; i++)
            {
                myk mmm = new myk();
                mmm.p = i;
                Thread t = new Thread((mmm) =>
                {
                    var m = mmm as myk;
                    //var w = o as ThreadWorkItem;
                    //if (w == null) return;
                    UnsafeSynchronizer uns = new UnsafeSynchronizer();

                    nscd.GetOrAdd(m.p, uns);
                    Console.WriteLine("我是线程：" + Thread.CurrentThread.Name + ":群里无聊吗？");
                    Thread.Sleep(300);
                    uns.acquire();


                    Console.WriteLine("我是线程：" + Thread.CurrentThread.Name + "退出了群聊");
                    Thread.Sleep(5000);
                    unsafeSynchronizer.release();
                });
                t.Name = "Hello,i 'am Thread: " + i;

                t.Start(mmm);

            }
            Thread.Sleep(5000);
            for (int i = 0; i < 3; i++)
            {
                UnsafeSynchronizer un;
                nscd.TryGetValue(i, out un);
                un.release();
            }
            unsafeSynchronizer.acquire();
            Console.WriteLine("我是线程：" + Thread.CurrentThread.Name + "群主解散了群聊");
            //threads[0].Suspend();



        }
        class myk {
            public int p;
        }
        static void fun6()
        {
            UnsafeSynchronizer unsafeSynchronizer = new UnsafeSynchronizer();
            LinkMap<int,UnsafeSynchronizer> nscd = new LinkMap<int, UnsafeSynchronizer>();
            for (int i = 0; i < 3; i++)
            {
                myk mmm= new myk();
                mmm.p = i;
                Thread t = new Thread((mmm) =>
                {
                   var m= mmm as myk;
                      //var w = o as ThreadWorkItem;
                      //if (w == null) return;
                      UnsafeSynchronizer uns = new UnsafeSynchronizer();

                    nscd.put(m.p, uns);
                    Console.WriteLine("我是线程：" + Thread.CurrentThread.Name + ":群里无聊吗？");
                    Thread.Sleep(300);
                    uns.acquire();
                 
                    
                    Console.WriteLine("我是线程：" + Thread.CurrentThread.Name + "退出了群聊");
                    Thread.Sleep(5000);
                    unsafeSynchronizer.release();
                });
                t.Name = "Hello,i 'am Thread: " + i;

                t.Start(mmm);
               
            }
            Thread.Sleep(5000);
            for (int i = 0; i < 3; i++) {              
                nscd.get(i).release();
            }
            unsafeSynchronizer.acquire();
            Console.WriteLine("我是线程：" + Thread.CurrentThread.Name + "群主解散了群聊");
            //threads[0].Suspend();



        }
        private static void ThreadFuncOne()
        {
            for (int i = 0; i < 10; i++)
            {
                Console.WriteLine(Thread.CurrentThread.Name + "   i =  " + i);
            }
            Console.WriteLine(Thread.CurrentThread.Name + " has finished");
        }

        static void fun4()
        {
            Thread.CurrentThread.Name = "MainThread";

            Thread newThread = new Thread(new ThreadStart(ThreadFuncOne));
            newThread.Name = "NewThread";

            for (int j = 0; j < 20; j++)
            {
                if (j == 10)
                {
                    newThread.Start();
                    newThread.Join();
                }
                else
                {
                    Console.WriteLine(Thread.CurrentThread.Name + "   j =  " + j);
                }
            }
            Console.Read();
        }
        static void fun3()
        {

            List<Thread> threads = new List<Thread>();
            for (int i = 0; i < 3; i++)
            {
                Thread t = new Thread((o) =>
                {
                    //var w = o as ThreadWorkItem;
                    //if (w == null) return;

                    while (true)
                    {
                      

     
                        Console.WriteLine("我是线程：" + Thread.CurrentThread.Name + ":群里无聊吗？");
                        Thread.Sleep(3000);
                    }
                    Console.WriteLine("我是线程：" + Thread.CurrentThread.Name + "退出了群聊");
                });
                t.Name = "Hello,i 'am Thread: " + i;

                t.Start();
                threads.Add(t);
            }
            //threads[0].Suspend();
          


        }
        class ThreadInterrupt
        {
           public static void fun5()
            {
                StayAwake stayAwake = new StayAwake();
                Thread newThread =
                    new Thread(new ThreadStart(stayAwake.ThreadMethod));
                newThread.Start();

                // The following line causes an exception to be thrown 
                // in ThreadMethod if newThread is currently blocked
                // or becomes blocked in the future.
                //Thread.SpinWait(1000);
                Thread.Sleep(1000);
                newThread.Interrupt();
                Console.WriteLine("Main thread calls Interrupt on newThread.");

                // Tell newThread to go to sleep.
                stayAwake.SleepSwitch = true;

                //// Wait for newThread to end.
                //newThread.Join();
            }
        }

        class StayAwake
        {
            bool sleepSwitch = false;

            public bool SleepSwitch
            {
                set { sleepSwitch = value; }
            }

            public StayAwake() { }

            public void ThreadMethod()
            {
                Console.WriteLine("newThread is executing ThreadMethod.");
                while (!sleepSwitch)
                {
                    // Use SpinWait instead of Sleep to demonstrate the 
                    // effect of calling Interrupt on a running thread.
                    Thread.SpinWait(1000);
                }
                try
                {
                    Console.WriteLine("newThread going to sleep.");

                    // When newThread goes to sleep, it is immediately 
                    // woken up by a ThreadInterruptedException.
                    Thread.Sleep(Timeout.Infinite);
                }
                catch (ThreadInterruptedException e)
                {
                    Console.WriteLine("newThread cannot go to sleep - " +
                        "interrupted by main thread.");
                }
            }
        }
        static void fun1()
        {
            ThreadWorkItem itemMain = new ThreadWorkItem
            {
                //线程0,1持续运行,设置true后非阻塞，持续运行。需要手动触发Reset()才会阻塞实例所在当前线程
                ManualResetEvent = new ManualResetEvent(true),

            };

            //线程工作集合
            Task.Factory.StartNew(() =>
            {
                Thread t = null;
                ThreadWorkItem item = null;
                for (int i = 0; i < 2; i++)
                {
                    t = new Thread((o) =>
                    {
                        var w = o as ThreadWorkItem;
                        if (w == null) return;

                        while (true)
                        {
                            //阻塞当前线程
                            bool t = w.ManualResetEvent.WaitOne(10000);

                            Console.WriteLine("我是线程：" + Thread.CurrentThread.Name + ":群里无聊吗？" + !t);
                            if (!t) break;
                            Thread.Sleep(200);
                        }
                        Console.WriteLine("我是线程：" + Thread.CurrentThread.Name + "退出了群聊");
                    });
                    t.Name = "Hello,i 'am Thread: " + i;
                    item = new ThreadWorkItem
                    {
                        //线程0,1持续运行,设置true后非阻塞，持续运行。需要手动触发Reset()才会阻塞实例所在当前线程
                        ManualResetEvent = new ManualResetEvent(true),
                        Thread = t,
                        ThreadManagerId = t.ManagedThreadId,
                        ThreadName = t.Name
                    };
                    Works.Add(item);
                    t.Start(item);
                }



                //5秒后准备暂停一个线程1。线程0持续运行
                Thread.Sleep(1000);
                Console.WriteLine("close...");
                Works[1].ManualResetEvent.Reset();

                //5秒后恢复线程1;线程0,1持续运行
                Thread.Sleep(1000);
                Console.WriteLine("open...");
                Works[1].ManualResetEvent.Set();

                //5秒后准备暂停一个线程0。线程1持续运行
                Thread.Sleep(1000);
                Console.WriteLine("close0...");
                Works[0].ManualResetEvent.Reset();

                //5秒后恢复线程1;线程0,1持续运行
                Thread.Sleep(1000);
                Console.WriteLine("open0...");
                Works[0].ManualResetEvent.Set();
                //5秒后恢复线程1;线程0,1持续运行
                Thread.Sleep(5000);
                Console.WriteLine("closeAll...");
                Works[0].ManualResetEvent.Reset();
                Works[1].ManualResetEvent.Reset();
                while (true)
                {

                    ThreadState ts = Thread.CurrentThread.ThreadState;
                    Console.WriteLine("我是线程：" + Thread.CurrentThread.Name + "群主解散了群聊");
                    //while (true)
                    //{
                    //    Thread.CurrentThread.TrySetApartmentState(System.Threading.ThreadState.WaitSleepJoin);
                    //    }
                    Thread.CurrentThread.Interrupt();
                  
                   
                    itemMain.ManualResetEvent.WaitOne();
                    Thread.Sleep(2000);
                    //Reset();
                }
            });
            
            /// <summary>
            /// 假设的网络请求
            /// </summary>
            /// <param name="state">参数</param>

        }
        private static void MyHttpRequest(object state)
        {
            // Thread.Sleep(1000);
            Console.WriteLine(String.Format("哈哈:{0}", ++i));

            MutipleThreadResetEvent countdown = state as MutipleThreadResetEvent;
            //发送信号量 本线程执行完毕
            countdown.SetOne();
        }
        static int i = 0;

        /// <summary>
        /// 主方法
        /// </summary>
        /// <param name="args">参数</param>
        static void fun2()
        {
            //假设有100个请求线程
            int num = 100;

            //使用 MutipleThreadResetEvent
            using (var countdown = new MutipleThreadResetEvent(num))
            {
                for (int i = 0; i < num; i++)
                {
                    //开启N个线程，传递MutipleThreadResetEvent对象给子线程
                    ThreadPool.QueueUserWorkItem(MyHttpRequest, countdown);
                }

                //等待所有线程执行完毕
                countdown.WaitAll();
            }

            Console.WriteLine("所有的网络请求以及完毕，可以继续下面的分析...");
            Console.ReadKey();
        }
    }
   
    /// <summary>
    ///  封装ManualResetEvent
    /// </summary>
    public class MutipleThreadResetEvent : IDisposable
    {
        private readonly ManualResetEvent done;
        private readonly int total;
        private long current;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="total">需要等待执行的线程总数</param>
        public MutipleThreadResetEvent(int total)
        {
            this.total = total;
            current = total;
            done = new ManualResetEvent(false);
        }

        /// <summary>
        /// 唤醒一个等待的线程
        /// </summary>
        public void SetOne()
        {
            // Interlocked 原子操作类 ,此处将计数器减1
            if (Interlocked.Decrement(ref current) == 0)
            {
                //当所以等待线程执行完毕时，唤醒等待的线程
                done.Set();
            }
        }

        /// <summary>
        /// 等待所以线程执行完毕
        /// </summary>
        public void WaitAll()
        {
            done.WaitOne();
        }

        /// <summary>
        /// 释放对象占用的空间
        /// </summary>
        public void Dispose()
        {
            ((IDisposable)done).Dispose();
        }
    }
    class Pro{
        public static void say(string s) {
            Console.WriteLine(s);
        }

    }
}
