using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mojang.Minecraft.Protocol;
using Mojang.Minecraft.Protocol.Providers;
using System.IO;
using System.Reflection;

namespace Mojang.Minecraft.Protocol
{
    partial class EssentialClient
    {
        private class PackageHandlerManager : HandlerManager<EssentialClient, PackageHandlerAttribute, PackageHandlerKey>
        {

            public PackageHandlerManager(EssentialClient ownerInstance) : base(ownerInstance)
            {

            }

        }


        [AttributeUsage(AttributeTargets.Method)]
        private class PackageHandlerAttribute : Attribute, IHandlerAttribute<PackageHandlerKey>
        {
            public PackageHandlerKey HandlerKey { get; set; }

            public PackageHandlerAttribute(int packageID, State state = State.Play)
            {
                HandlerKey = new PackageHandlerKey(packageID, state);
            }

        }


        private struct PackageHandlerKey
        {
            public State State;
            public int PackageID;

            public PackageHandlerKey(int packageID, State state = State.Play)
            {
                State = state;
                PackageID = packageID;
            }

        }



    }

}
