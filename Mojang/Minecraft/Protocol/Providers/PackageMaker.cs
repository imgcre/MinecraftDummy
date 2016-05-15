using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mojang.Minecraft.Protocol.Providers
{
    internal sealed class PackageMaker
    {
        private List<byte> _EntityList;
        private int _TypeCode;


        public PackageMaker(int typeCode) : this()
        {
            _TypeCode = typeCode;
        }


        private PackageMaker()
        {
            _EntityList = new List<byte>();
        }


        public void AppendVarintLengthPrefixedBytes(IEnumerable<byte> bytes)
        {
            var length = PackageFieldToByteList(new VarInt((uint)bytes.Count()));
            _EntityList.AddRange(length);
            _EntityList.AddRange(bytes);
        }


        public void AppendBytes(IEnumerable<byte> bytes)
        {
            _EntityList.AddRange(bytes);
        }


        public void AppendPackageField<T>(T o)
            where T : IPackageField
            => o.AppendIntoPackageMaker(this);


        public void AppendPackageField(IPackageField o)
            => o.AppendIntoPackageMaker(this);


        public void AppendMetaType<T>(T obj)
            => AppendMetaType((object)obj);

        public void AppendMetaType(object o)
        {
            if (!IsMetaType(o.GetType()))
                throw new ArgumentException("指定的类型错误");

            if (o.GetType() == typeof(bool))
                _EntityList.Add((byte)((bool)o ? 1 : 0));
            else if (o.GetType() == typeof(byte) || o.GetType() == typeof(byte))
                _EntityList.Add((byte)o);
            else
                _EntityList.AddRange(BigEndianBitConverter.GetBytes(o as dynamic));

        }


        public static bool IsMetaType(Type t)
            => FieldMatcher.IsMetaType(t);


        public byte[] MakePackage()
        {
            var packageList = new List<byte>();

            var packageTypeCode = new VarInt((uint)_TypeCode);
            packageList.AddRange(PackageFieldToByteList(packageTypeCode));
            packageList.AddRange(_EntityList);

            var entityLength = new VarInt((uint)packageList.Count);
            packageList.InsertRange(0, PackageFieldToByteList(entityLength));

            return packageList.ToArray();
        }


        private static List<byte> PackageFieldToByteList(IPackageField bitConvertible)
        {
            var packageMaker = new PackageMaker();
            bitConvertible.AppendIntoPackageMaker(packageMaker);
            return packageMaker._EntityList;
        }


        public static int GetVariablePackageFieldLength(IPackageField bitConvertible)
        {
            return PackageFieldToByteList(bitConvertible).Count;
        }

    }
}
