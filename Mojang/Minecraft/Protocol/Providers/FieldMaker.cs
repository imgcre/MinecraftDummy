using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mojang.Minecraft.Protocol.Providers
{
    internal class FieldMaker
    {
        public readonly EssentialClient Owner;
        private List<byte> _EntityList = new List<byte>();

        public byte[] GetBytes()
        {
            return _EntityList.ToArray();
        }


        public void AppendBytes(IEnumerable<byte> bytes)
        {
            _EntityList.AddRange(bytes);
        }

        public void AppendPackageField<T>(T o)
            where T : IPackageField
            => o.AppendIntoField(this);


        public void AppendPackageField(IPackageField o)
            => o.AppendIntoField(this);


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
    }
}
