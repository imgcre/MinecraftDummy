using Microsoft.CSharp.RuntimeBinder;
using Mojang.Minecraft.Protocol.Providers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Mojang.Minecraft.Protocol
{
    /// <summary>
    /// minecraft smp ping流程的抽象，提供了基本的ping功能
    /// </summary>
    public class PingClient : EssentialClient
    {
        /// <summary>
        /// 服务器版本
        /// </summary>
        public struct ServerVersion
        {
            /// <summary>
            /// 服务器版本名称
            /// </summary>
            public string Name;
            /// <summary>
            /// 服务器协议版本号
            /// </summary>
            public int Protocol;
        }

        /// <summary>
        /// 服务器版本
        /// </summary>
        public ServerVersion Version  => new ServerVersion
        {
            Name = GetServerInfo(r => r.version.name),
            Protocol = GetServerInfo(r => r.version.protocol)
        };


        /// <summary>
        /// 玩家信息
        /// </summary>
        public struct PlayerInfo
        {
            /// <summary>
            /// 最大在线玩家数
            /// </summary>
            public int Max;
            /// <summary>
            /// 在线玩家数
            /// </summary>
            public int Online;
            /// <summary>
            /// 部分在线玩家信息
            /// </summary>
            public PlayerSample[] Sample;
        }


        /// <summary>
        /// 部分在线玩家的信息
        /// </summary>
        public struct PlayerSample
        {
            /// <summary>
            /// 玩家名称
            /// </summary>
            public string Name;
            /// <summary>
            /// 玩家的全局唯一标识符
            /// </summary>
            public Guid Id;
        }

        /// <summary>
        /// 玩家信息，包括最大玩家数，在线玩家数，以及部分在线玩家的信息
        /// </summary>
        public PlayerInfo players => new PlayerInfo
        {
            Max = GetServerInfo(r => r.players.max),
            Online = GetServerInfo(r => r.players.online),
            Sample = Array.ConvertAll(
                GetServerInfo(r => r.players.sample), 
                new Converter<dynamic, PlayerSample>(sample => new PlayerSample
                {
                    Name = sample.name,
                    Id = new Guid(sample.id)
                }
            )),
        };


        /// <summary>
        /// 获得原始且完整的服务器信息
        /// </summary>
        public string Raw
        {
            get
            {
                _Lock.WaitOne();
                return _Response.ToString();
            }
        }

        /// <summary>
        /// 服务器的描述，即motd
        /// </summary>
        public string Description => GetServerInfo(r => r.description);


        /// <summary>
        /// 连接至mc服务器
        /// </summary>
        /// <param name="serverAddress">服务器ip或域名</param>
        /// <param name="serverPort">服务器端口</param>
        /// <param name="Username">玩家名称</param>
        public PingClient(string serverAddress, ushort serverPort) : base(serverAddress, serverPort)
        {
            PingProcess();
        }


        /// <summary>
        /// 连接至mc服务器
        /// </summary>
        /// <param name="connectProvider">至服务器的连接</param>
        /// <param name="username">玩家名称</param>
        public PingClient(IConnectProvider connectProvider) : base(connectProvider)
        {
            PingProcess();
        }


        /// <summary>
        /// 存储发出ping请求时tick值以及相应事件的字典
        /// </summary>
        private Dictionary<long, ManualResetEvent> _PingInfoDictionary = new Dictionary<long, ManualResetEvent>();


        /// <summary>
        /// 向服务器发出ping请求，并获得此连接的延迟
        /// </summary>
        /// <returns>此连接的延迟，单位为毫秒</returns>
        public async Task<long> Ping()
        {
            var tick = DateTime.UtcNow.Ticks;
            var waiter = new ManualResetEvent(false);
            _PingInfoDictionary[tick] = waiter;
            await Ping(tick);
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            waiter.WaitOne();
            stopwatch.Stop();
            return stopwatch.ElapsedMilliseconds;
        }


        protected override void OnPong(long payload)
        {
            if (_PingInfoDictionary.Keys.Contains(payload))
            {
                _PingInfoDictionary[payload].Set();
                _PingInfoDictionary.Remove(payload);
            }
        }


        private void PingProcess()
        {
            Handshake(47, ServerAddress, ServerPort, LoginNextState.Ping).Wait();
            HandshakeRequest().Wait();
            StartRecievePackage();
        }


        private ManualResetEvent _Lock = new ManualResetEvent(false);
        private Chat _Response;


        /// <summary>
        /// 从服务器的响应中获得服务器的信息
        /// </summary>
        /// <param name="func">获得欲获得字段的谓词</param>
        /// <returns>相应的字段值，若不存在，则为null</returns>
        private dynamic GetServerInfo(Func<dynamic, dynamic> func)
        {
            _Lock.WaitOne();
            try
            {
                return func(_Response.Root);
            }
            catch (RuntimeBinderException)
            {
                
            }

            return null;
        }


        protected override void OnResponded(Chat response)
        {
            _Response = response;
            _Lock.Set();
        }

    }
}
