using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mojang.Minecraft.Protocol.Providers
{
    internal sealed class PackageMaker
    {
        private FieldMaker _FieldMaker;
        private int _TypeCode;


        public PackageMaker(int typeCode, FieldMaker fieldMaker)
        {
            _TypeCode = typeCode;
            _FieldMaker = fieldMaker;
        }


        public byte[] MakePackage()
        {
            var packageList = new List<byte>();

            var packageTypeCode = new VarInt((uint)_TypeCode);
            packageList.AddRange(PackageFieldToByteList(packageTypeCode));
            packageList.AddRange(_FieldMaker.GetBytes());

            var entityLength = new VarInt((uint)packageList.Count);
            packageList.InsertRange(0, PackageFieldToByteList(entityLength));

            return packageList.ToArray();
        }

        private static List<byte> PackageFieldToByteList(IPackageField bitConvertible)
        {
            var fieldMaker = new FieldMaker();
            bitConvertible.AppendIntoField(fieldMaker);
            return new List<byte>(fieldMaker.GetBytes());
        }

        public static int GetVariablePackageFieldLength(IPackageField bitConvertible)
        {
            return PackageFieldToByteList(bitConvertible).Count;
        }

    }
}
