using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mojang.Minecraft.Protocol.Providers;

namespace Mojang.Minecraft
{
    public class Player : IPackageField
    {
        public Uuid UUID { get; private set; }


        void IPackageField.FromField(FieldMatcher fieldMatcher)
        {
            throw new NotImplementedException();
        }

        void IPackageField.AppendIntoField(FieldMaker fieldMaker)
        {
            throw new NotImplementedException();
        }
    }
}
