using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mojang.Minecraft.Protocol.Providers
{
    public interface IConnectProvider
    {
        Task Send(byte[] data);


        Task<byte[]> Receive(int length);


        string ServerAddress { get; }


        ushort ServerPort { get; }

    }


    /// <summary>
    /// ConnectProvider内部连接已断开
    /// </summary>
    public class ConnectDisconnectedException : Exception
    {

    }


}
