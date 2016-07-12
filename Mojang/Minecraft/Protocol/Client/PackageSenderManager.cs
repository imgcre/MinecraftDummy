using Mojang.Minecraft.Protocol.Providers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Mojang.Minecraft.Protocol
{
    partial class EssentialClient
    {
        private class PackageSenderManager : SenderManager<EssentialClient, PackageSenderAttribute, PackageSenderKey>
        {
            public PackageSenderManager(EssentialClient ownerInstance) : base(ownerInstance)
            {

            }

            protected override bool IdentifySender(PackageSenderAttribute attribute)
                => base.IdentifySender(attribute) && OwnerInstance._ConnectState == attribute.SenderKey.State;


            protected override async Task<FieldMaker> MakeFieldInternal(MethodBase sender, PackageSenderAttribute attribute, params object[] fields)
            {
                var fieldMaker = await base.MakeFieldInternal(sender, attribute, fields);
                var typeCode = attribute.SenderKey.PackageID;
                var packageMaker = new PackageMaker(typeCode, fieldMaker);
                await OwnerInstance._ConnectProvider.Send(packageMaker.MakePackage());
                return null;
            }
        }


        [AttributeUsage(AttributeTargets.Method)]
        private class PackageSenderAttribute : Attribute, ISenderAttribute<PackageSenderKey>
        {

            public PackageSenderKey SenderKey { get; set; }

            public PackageSenderAttribute(int packageID, State state = State.Play)
            {
                SenderKey = new PackageSenderKey(packageID, state);
            }

        }

        private struct PackageSenderKey
        {
            public State State;
            public int PackageID;

            public PackageSenderKey(int packageID, State state = State.Play)
            {
                State = state;
                PackageID = packageID;
            }

        }

    }
}
