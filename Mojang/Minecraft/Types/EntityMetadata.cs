using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mojang.Minecraft;
using Mojang.Minecraft.Protocol.Providers;
using Mojang.Minecraft.Protocol;

namespace Mojang.Minecraft
{

    public enum MetadataType : byte
    {
        Byte,
        Short,
        Int,
        Float,
        String,
        Slot,
        XYZ,
        PitchYawRoll
    }


    public class EntityMetadata : IPackageField
    {
        private Dictionary<int, object> _MetadataDictionary = new Dictionary<int, object>();

        public EntityMetadata()
        {

        }

        void IPackageField.FromFieldMatcher(FieldMatcher fieldMatcher)
        {
            for (var keyAndType = fieldMatcher.ReadByte(); keyAndType != 127; keyAndType = fieldMatcher.ReadByte())
            {
                var key = keyAndType & 0x1F;
                var valueTypeCode = keyAndType >> 5;
                var type = GetValueType(valueTypeCode);

                if (type.GetType().IsArray)
                {
                    var typeArray = (Type[])type;
                    var valueArray = new object[typeArray.Length];
                    for (var i = 0; i < valueArray.Length; i++)
                        valueArray[i] = fieldMatcher.Match(typeArray[i]);
                    _MetadataDictionary[key] = valueArray;
                }
                else
                    _MetadataDictionary[key] = fieldMatcher.Match(type as Type);
            }
        }


        object GetValueType(int typeCode)
            => new object[]
                {
                    typeof(byte),
                    typeof(short),
                    typeof(int),
                    typeof(float),
                    typeof(VarintPrefixedUTF8String),
                    typeof(Slot),
                    new[] { typeof(int), typeof(int), typeof(int) },
                    new[] { typeof(float), typeof(float), typeof(float) }
                }[typeCode];


        void IPackageField.AppendIntoPackageMaker(PackageMaker packageMaker)
        {
            throw new NotImplementedException();
        }


        public override string ToString()
        {
            var sb = new StringBuilder();
            Array.ForEach(_MetadataDictionary.ToArray(), kvp => sb.AppendLine($"key: {kvp.Key}, value: {kvp.Value}"));
            return sb.ToString();
        }

    }
}
