using Mojang.Minecraft.Protocol.Providers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mojang.Minecraft.Protocol
{
    /// <summary>
    /// 内部实现了假人登陆的流程
    /// </summary>
    public class PlayerClient : EssentialClient
    {
        public string Username { get; set; }

        /// <summary>
        /// 连接至mc服务器
        /// </summary>
        /// <param name="serverAddress">服务器ip或域名</param>
        /// <param name="serverPort">服务器端口</param>
        /// <param name="Username">玩家名称</param>
        public PlayerClient(string serverAddress, ushort serverPort, string username) : base(serverAddress, serverPort)
        {
            LoginToServer(username);
        }


        /// <summary>
        /// 连接至mc服务器
        /// </summary>
        /// <param name="connectProvider">至服务器的连接</param>
        /// <param name="username">玩家名称</param>
        public PlayerClient(IConnectProvider connectProvider, string username) : base(connectProvider)
        {
            LoginToServer(username);
        }


        private void LoginToServer(string username)
        {
            Username = username;
            Handshake(47, ServerAddress, ServerPort, LoginNextState.Login).Wait();
            Login(username).Wait();
            StartRecievePackage();
        }


        

    }
}
