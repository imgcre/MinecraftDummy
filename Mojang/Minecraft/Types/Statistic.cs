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


        void IPackageField.FromField(FieldMatcher fieldMatcher)
        {
            Name = fieldMatcher.MatchPackageField<PString>();
            Value = fieldMatcher.MatchPackageField<VarInt>();
        }


        void IPackageField.AppendIntoField(FieldMaker fieldMaker)
        {
            fieldMaker.AppendPackageField<PString>(Name);
            fieldMaker.AppendPackageField<VarInt>(Value);
        }
    }
}
