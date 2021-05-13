using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace EF.RPC.Protobuf
{
    /// <summary>
    /// 唯一id，相比System.Guid.NewGuid()，区分消息唯一id，System.Guid.NewGuid()对于业务更合理
    /// </summary>
    public class SnowflakeId
    {
        public const long Twepoch = 1288834974657L;

        private const int workerIdBits = 8;
        private const int datacenterIdBits = 8;
        private const int sequenceBits = 8;
        private const long maxworkerId = -1L ^ (-1L << workerIdBits);
        private const long maxdatacenterId = -1L ^ (-1L << datacenterIdBits);

        private const int workerIdShift = sequenceBits;
        private const int datacenterIdShift = sequenceBits + workerIdBits;
        public const int timestampLeftShift = sequenceBits + workerIdBits + datacenterIdBits;
        private const long sequenceMask = -1L ^ (-1L << sequenceBits);
        private static SnowflakeId snowflakeId;
        private  static readonly object sLock = new object();
        private volatile int index =0 ;
        public SnowflakeId(long workerId, long datacenterId)
        {
            this.workerId = workerId + System.Diagnostics.Process.GetCurrentProcess().Id; 
            this.datacenterId = datacenterId;


            // sanity check for workerId
            while (workerId > maxworkerId || workerId < 0) {
                this.workerId = this.workerId >> 1;
            }
            if (datacenterId > maxdatacenterId || datacenterId < 0)
                throw new ArgumentException($"datacenter Id can't be greater than {maxdatacenterId} or less than 0");
        }

        public long workerId { get; protected set; } 
        public long datacenterId { get; protected set; }


        public static SnowflakeId Default()
        {
            if (snowflakeId != null)
            {
                return snowflakeId;
            }
            lock (sLock)
            {
                if (snowflakeId != null)
                {
                    return snowflakeId;
                }

                var random = new Random();
                var workerId = random.Next((int)maxworkerId + +System.Diagnostics.Process.GetCurrentProcess().Id);
                while (workerId > maxworkerId || workerId < 0)
                {
                    workerId = workerId >> 1;
                }
                var datacenterId = random.Next((int)maxdatacenterId);
                return snowflakeId = new SnowflakeId(workerId, datacenterId);
            }
        }
        /// <summary>
        /// 伪唯一id
        /// 无io操作cup级操作下id无效
        /// 性能测试代码如下
        /// </summary>
        /// <returns></returns>
        //Dictionary<long, int> d = new Dictionary<long, int>();
        //new Thread(() =>
        //    {
        //    for (; ; )
        //    {
        //        long l = SnowflakeId.Default().NextId();
        //        Console.WriteLine($"Return result: {"id：" + l }");
        //        d.Add(l, 1);
        //    }
        //}).Start();
        //new Thread(() =>
        //    {
        //    for (; ; )
        //    {
        //        long l = SnowflakeId.Default().NextId();
        //        Console.WriteLine($"Return result: {"id：" + l }");
        //        d.Add(l, 1);
        //    }
        //}).Start();
        public virtual long NextId()
        {
            return index++ + (((DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() + Thread.CurrentThread.ManagedThreadId) - Twepoch) << timestampLeftShift) |
                        (datacenterId << datacenterIdShift) |
                        (workerId << workerIdShift);
        }

       

        protected virtual long TimeGen()
        {
            return DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        }
        protected virtual long ManagedThreadId()
        {
            return Thread.CurrentThread.ManagedThreadId;
        }
    }
}
