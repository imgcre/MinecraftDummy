using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Net;
using System.Net.Sockets;

using static System.BitConverter;
using System.IO;
using Microsoft.CSharp.RuntimeBinder;

using System.Reflection;
using Mojang.Minecraft.Protocol.Providers;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;

namespace Mojang.Minecraft.Protocol
{
    /// <summary>
    /// 表示minecraft smp的协议客户端，提供了基本的api
    /// </summary>
    public partial class EssentialClient
    {
        private IConnectProvider _ConnectProvider;
        private Stream _ConnectStream;
        private State _ConnectState = State.Handshaking;
        private Task _DispatchCycle;
        private PackageHandlerManager _PackageHandlerManager;
        private PackageSenderManager _PackageSenderManager;

        public string ServerAddress => _ConnectProvider.ServerAddress;
        public ushort ServerPort => _ConnectProvider.ServerPort;


        private enum State
        {
            Handshaking,
            Status,
            Login,
            Play,
        }

        /// <summary>
        /// 连接至mc服务器
        /// </summary>
        /// <param name="serverAddress">服务器ip或域名</param>
        /// <param name="serverPort">服务器端口</param>
        /// <param name="Username">玩家名称</param>
        public EssentialClient(string serverAddress, ushort serverPort)
        {
            _ConnectProvider = new DefaultConnectProvider(serverAddress, serverPort);
            Initlize();
        }


        /// <summary>
        /// 连接至mc服务器
        /// </summary>
        /// <param name="connectProvider">至服务器的连接</param>
        /// <param name="username">玩家名称</param>
        public EssentialClient(IConnectProvider connectProvider)
        {
            _ConnectProvider = connectProvider;
            Initlize();
        }


        private void Initlize()
        {
            _ConnectStream = new ConnectStream(_ConnectProvider);
            _PackageHandlerManager = new PackageHandlerManager(this);
            _PackageSenderManager = new PackageSenderManager(this);
            _ChannelSenderManager = new ChannelSenderManager(this);

        }


        private bool _Strated;
        protected void StartRecievePackage()
        {
            if (!_Strated)
            {
                _Strated = true;

                _DispatchCycle = Task.Run(() =>
                {
                    for (;;)
                    {
                        var fieldMatcher = new FieldMatcher(this, _ConnectStream);
                        if (fieldMatcher.IsNullPackage)
                            continue;
                        OnPackageReceived(fieldMatcher);
                        fieldMatcher.Dispose();
                    }
                });
            }
        }
    }
}
