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
}
