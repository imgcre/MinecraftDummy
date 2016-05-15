using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Mojang.Minecraft;

namespace Mojang.Minecraft.Bukkit
{
    public class Client : Minecraft.Protocol.EssentialClient
    {
        public Client(string serverAddress, ushort serverPort) : base(serverAddress, serverPort)
        {

        }
    }
}
