using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Text;

namespace EF.RPC.Impl.RabbitMQImpl
{
    public class RabbitMQOptionsFactory<MsgMathsInfoMap> : MsgMathsInfoFactory<MsgMathsInfoMap> where MsgMathsInfoMap : new()
    {
        public RabbitMQOptions opt;
        // ;
        public RabbitMQOptionsFactory() {
            opt = new RabbitMQOptions();
            opt.factory = new ConnectionFactory() { HostName = "localhost" };//
        }
        //
        // 摘要:
        //     Default value for the desired maximum channel number. Default: 2047.
        public const ushort DefaultChannelMax = 2047;
        //
        // 摘要:
        //     Default value for the desired maximum frame size. Default is 0 ("no limit").
        public const uint DefaultFrameMax = 0;
        //
        // 摘要:
        //     Default password (value: "guest").
        public const string DefaultPass = "guest";
        //
        // 摘要:
        //     Default user name (value: "guest").
        public const string DefaultUser = "guest";
        //
        // 摘要:
        //     Default virtual host (value: "/").
        public const string DefaultVHost = "/efrpc";
        //
        // 摘要:
        //     Default value for connection attempt timeout.
        public static readonly TimeSpan DefaultConnectionTimeout;
        //
        // 摘要:
        //     Default value for desired heartbeat interval. Default is 60 seconds, TimeSpan.Zero
        //     means "heartbeats are disabled".
        public static readonly TimeSpan DefaultHeartbeat;
        //
        // 摘要:
        //     The host to connect to.
        public string HostName { get; set; }
        //
        // 摘要:
        //     Password to use when authenticating to the server.
        public string Password { get; set; }

        // 摘要:
        //     The port to connect on. RabbitMQ.Client.AmqpTcpEndpoint.UseDefaultPort indicates
        //     the default for the protocol should be used.
        public int Port { get; set; }
        //
        // 摘要:
        //     Heartbeat timeout to use when negotiating with the server.
        public TimeSpan RequestedHeartbeat { get; set; }
        //
        // 摘要:
        //     Username to use when authenticating to the server.
        public string UserName { get; set; }
        public override MsgMathsInfoMap getMsgMathsInfoMap()
        {
            return new MsgMathsInfoMap();
        }
    }
}
