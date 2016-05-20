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
            //foreach (var s in Array.ConvertAll(typeof(Test).GetFields(), f => f.CustomAttributes.Count() != 0 ? f.CustomAttributes.First().ConstructorArguments.First().Value : "null")) WriteLine(s);
            //由名称获得枚举字段
            var field = Test.ele2;

            field.GetType().GetField(field.ToString()).CustomAttributes.ToArray();
            Array.Find(field.GetType().GetField(field.ToString()).CustomAttributes.ToArray(), custom => custom.AttributeType == typeof(RenamedTestAttribute));

            WriteLine(Array.Find(field.GetType().GetField(field.ToString()).CustomAttributes.ToArray(), custom => custom.AttributeType == typeof(RenamedTestAttribute))?.ConstructorArguments.First().Value.ToString() ?? field.ToString());

            //WriteLine(typeof(Test).GetField("ele3")?.ToString() ?? "xx");
            //new TestClient("localhost", 25565, "pst");
            ReadLine();

        }
    }

    enum Test
    {
        ele1,
        [RenamedTest("hithere")]ele2,
    }

    [AttributeUsage(AttributeTargets.Field)]
    public class RenamedTestAttribute : Attribute
    {
        //public readonly string OldName;

        public RenamedTestAttribute(string oldName)
        {
            //OldName = oldName;
        }
    }


}
