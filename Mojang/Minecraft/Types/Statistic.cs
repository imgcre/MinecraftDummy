using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mojang.Minecraft.Protocol.Providers;

namespace Mojang.Minecraft
{
    public class Statistic : IPackageField
    {
        public string Name { get; private set; }

        public int Value { get; private set; }


        void IPackageField.FromFieldMatcher(FieldMatcher fieldMatcher)
        {
            Name = fieldMatcher.MatchPackageField<VarintPrefixedUTF8String>();
            Value = fieldMatcher.MatchPackageField<VarInt>();
        }


        void IPackageField.AppendIntoPackageMaker(PackageMaker packageMaker)
        {
            packageMaker.AppendPackageField<VarintPrefixedUTF8String>(Name);
            packageMaker.AppendPackageField<VarInt>(Value);
        }
    }
}
