using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mojang.Minecraft.Protocol.Providers;

namespace Mojang.Minecraft
{
    public class Chunk : IPackageField
    {
        

        void IPackageField.FromField(FieldMatcher fieldMatcher)
        {
            var size = fieldMatcher.Match<VarInt>().ToUint();


            throw new NotImplementedException();
        }

        void IPackageField.AppendIntoField(FieldMaker fieldMaker)
        {
            throw new NotSupportedException();
        }
    }
}
