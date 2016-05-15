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
        private Dictionary<int, object> _MetadataDictionary;

        public Dictionary<int, object> Root => _MetadataDictionary;

        void IPackageField.FromFieldMatcher(FieldMatcher fieldMatcher)
        {
            var payloadPrefix = fieldMatcher.MatchMetaType<byte>();
            _MetadataDictionary = new Dictionary<int, object>();

            while (payloadPrefix != 127)
            {

                //拆分这个byte
                //最低5比特（&0x1f）作为接下来的数据的标识（键）
                var key = payloadPrefix & 0x1f;
                //最高3比特(&0xe0)作为类型
                var type = (MetadataType)(payloadPrefix >> 5);

                var payload = default(object);
                if (IsMetaType(type))
                {
                    payload = fieldMatcher.MatchMetaType(GetObjectTypeFromMetadataType(type));
                }
                else
                {
                    switch (type)
                    {
                        case MetadataType.String:
                            payload = fieldMatcher.MatchPackageField(typeof(VarintPrefixedUTF8String)).ToString();
                            break;
                        case MetadataType.Slot:
                            payload = fieldMatcher.MatchPackageField(typeof(Slot));
                            break;
                        case MetadataType.XYZ:
                            var x = (int)fieldMatcher.MatchMetaType(typeof(int));
                            var y = (int)fieldMatcher.MatchMetaType(typeof(int));
                            var z = (int)fieldMatcher.MatchMetaType(typeof(int));
                            payload = new[] { x, y, z };
                            break;
                        case MetadataType.PitchYawRoll:
                            var pitch = (float)fieldMatcher.MatchMetaType(typeof(float));
                            var yaw = (float)fieldMatcher.MatchMetaType(typeof(float));
                            var roll = (float)fieldMatcher.MatchMetaType(typeof(float));
                            payload = new[] { pitch, yaw, roll };
                            break;
                    }
                }

                _MetadataDictionary.Add(key, payload);
                payloadPrefix = fieldMatcher.MatchMetaType<byte>();
            }

            
        }

        void IPackageField.AppendIntoPackageMaker(PackageMaker packageMaker)
        {
            foreach (var kvp in _MetadataDictionary)
            {
                var payloadKey = default(byte);
                var payloadType = GetObjectMetadataType(kvp.Value.GetType());
                payloadKey = (byte)((byte)payloadType << 5 | kvp.Key);

                if (IsMetaType(payloadType))
                {
                    packageMaker.AppendMetaType(payloadKey);
                }
                else
                {
                    switch (payloadType)
                    {
                        case MetadataType.String:
                            packageMaker.AppendPackageField(new VarintPrefixedUTF8String(kvp.Value.ToString()));
                            break;
                        case MetadataType.Slot:
                            packageMaker.AppendPackageField(kvp.Value as IPackageField);
                            break;
                        case MetadataType.XYZ:
                            foreach(var coord in kvp.Value as int[])
                                packageMaker.AppendMetaType(coord);
                            break;
                        case MetadataType.PitchYawRoll:
                            foreach (var element in kvp.Value as float[])
                                packageMaker.AppendMetaType(element);
                            break;
                    }
                }
            }

            packageMaker.AppendMetaType<byte>(127);
        }


        private bool IsMetaType(MetadataType metadataType)
            => new[] { MetadataType.Byte, MetadataType.Short, MetadataType.Int, MetadataType.Float }.Contains(metadataType);


        private MetadataType GetObjectMetadataType(Type type)
        => new Dictionary<Type, MetadataType>
        {
            { typeof(byte), MetadataType.Byte },
            { typeof(short), MetadataType.Short },
            { typeof(int), MetadataType.Int },
            { typeof(float), MetadataType.Float },
            { typeof(string), MetadataType.String },
            { typeof(Slot), MetadataType.Slot },
            { typeof(int[]), MetadataType.XYZ },
            { typeof(float[]), MetadataType.PitchYawRoll },

        }[type];

        private Type GetObjectTypeFromMetadataType(MetadataType type)
        => new Dictionary<MetadataType, Type>
        {
            { MetadataType.Byte, typeof(byte) },
            { MetadataType.Short, typeof(short) },
            { MetadataType.Int, typeof(int) },
            { MetadataType.Float, typeof(float) },
            { MetadataType.String, typeof(string) },
            { MetadataType.Slot, typeof(Slot) },
            { MetadataType.XYZ, typeof(int[]) },
            { MetadataType.PitchYawRoll, typeof(float[]) },

        }[type];



        public override string ToString()
        {
            var sb = new StringBuilder();
            foreach (var kvp in _MetadataDictionary)
            {
                sb.AppendLine($"[{kvp.Key}] => {kvp.Value}");
            }
            return sb.ToString();
        }

    }
}
