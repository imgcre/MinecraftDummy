using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Mojang.Minecraft.Protocol.Providers
{
    internal sealed class FieldMatcher : IDisposable
    {
        private Stream _Stream;
        public readonly int PackageTypeCode;
        public bool Empty => ResidualPackageBodyLength <= 0;
        private readonly int PackageLength;
        private readonly int PackageBodyLength;
        public readonly bool IsNullPackage;
        private int UsedBodyLength;

        public Stream Stream => _Stream;

        private int ResidualPackageBodyLength => PackageBodyLength - UsedBodyLength;
        

        public FieldMatcher(Stream stream)
        {
            // TODO: 重构

            // 获得封包长度，注意此处未为封包长度赋值
            _Stream = stream;
            var packageLength = MatchPackageField<VarInt>();
            PackageLength = packageLength;
            if (PackageLength == 0)
            {
                IsNullPackage = true;
                return;
            }

            var packageTypeCode = MatchPackageField<VarInt>();
            PackageTypeCode = packageTypeCode;
            PackageBodyLength = PackageLength - packageTypeCode.Length;

        }


        public T MatchPackageField<T>()
            where T : IPackageField, new()
        {
            var obj = new T();
            obj.FromFieldMatcher(this);
            return obj;
        }


        public IPackageField MatchPackageField(Type t)
        {
            var obj = Activator.CreateInstance(t, null) as IPackageField;
            obj.FromFieldMatcher(this);
            return obj;
        }


        public T MatchMetaType<T>()
            => (T)MatchMetaType(typeof(T));


        public object MatchMetaType(Type t)
        {
            if (!IsMetaType(t))
                throw new ArgumentException("指定的类型错误");

            if (t == typeof(bool))
                return ReadByte() != 0;
            else if (t == typeof(byte))
                return ReadByte();
            else if (t == typeof(sbyte))
                return (sbyte)ReadByte();
            else
                return Array.Find(typeof(BigEndianBitConverter)
                    .GetMethods(BindingFlags.Public | BindingFlags.Static), m => m.ReturnParameter.ParameterType == t)
                    .Invoke(null, new object[] { ReadBytes(Marshal.SizeOf(t)), 0 });
        }


        public static bool IsMetaType(Type t)
            => new[] {
                    typeof(bool), typeof(byte), typeof(sbyte), typeof(short), typeof(ushort)
                    , typeof(int), typeof(uint), typeof(long), typeof(ulong), typeof(float), typeof(double)
            }.Any((element) => element == t);


        public byte[] ReadBytes(int count)
        {
            if (count <= 0)
                throw new ArgumentException("读取的元素数必须大于0");

            if (disposedValue)
                throw new ArgumentException("FieldMatcher对象已被dispose");

            if (PackageBodyLength != 0)
            {
                UsedBodyLength += count;
                if (ResidualPackageBodyLength < 0) // 等于0为刚好读完
                    throw new ArgumentException("剩余字段长度不足");
            }

            var bytes = new byte[count];
            _Stream.Read(bytes, 0, count);
            return bytes;




        }


        public byte ReadByte()
            => ReadBytes(1).First();


        #region IDisposable Support
        private bool disposedValue = false;

        private void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    if (ResidualPackageBodyLength != 0)
                    {
                        Console.WriteLine($"未实现的封包: {PackageTypeCode:x}");
                        if (UsedBodyLength != 0)
                            throw new ArgumentException($"封包id为{PackageTypeCode}的handler签名与协议不匹配");

                        ReadBytes(ResidualPackageBodyLength);
                    }
                }
                disposedValue = true;
            }
        }

        public void Dispose()
            => Dispose(true);

        #endregion

    }
}
