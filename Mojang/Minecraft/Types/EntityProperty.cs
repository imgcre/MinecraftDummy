using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mojang.Minecraft.Protocol.Providers;

namespace Mojang.Minecraft
{
    public class EntityProperty : IPackageField
    {
        public string Key { get; set; }
        public double Value { get; set; }
        public Modifier[] Modifiers { get; set; }

        void IPackageField.AppendIntoPackageMaker(PackageMaker packageMaker)
        {
            throw new NotImplementedException();
        }

        void IPackageField.FromFieldMatcher(FieldMatcher fieldMatcher)
        {
            Key = fieldMatcher.MatchPackageField<VarintPrefixedUTF8String>();
            Value = fieldMatcher.MatchMetaType<double>();

            int listLength = fieldMatcher.MatchPackageField<VarInt>();
            var modifierList = new List<Modifier>();
            for (var i = 0; i < listLength; i++)
            {
                var currentModifier = new Modifier();
                currentModifier.Uuid = fieldMatcher.MatchMetaType<Uuid>();
                currentModifier.Amount = fieldMatcher.MatchMetaType<double>();
                currentModifier.Operation = fieldMatcher.MatchMetaType<byte>();
                modifierList.Add(currentModifier);
            }
            Modifiers = modifierList.ToArray();

        }
    }


    public struct Modifier
    {
        public Uuid Uuid;
        public double Amount;
        public byte Operation;
    }

}
