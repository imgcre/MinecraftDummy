using fNbt;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using static System.Console;
using Mojang.Minecraft.Protocol.Providers;
using System.Threading;
using System.Dynamic;
using Mojang.Minecraft.Protocol;
using System.IO.Compression;
using System.IO;

namespace TestConsole
{
    class Program
    {
        static Program()
        {
            Title = " my Minecraft dummy";
        }
        
        static void Main(string[] args)
        {
            new TestClient("localhost", 25565, "pst");
            ReadLine();

        }

    }


}
