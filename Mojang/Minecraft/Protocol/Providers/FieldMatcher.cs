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
        //TODO:private
        public readonly int PackageBodyLength;
        public readonly bool IsNullPackage;
        private int UsedBodyLength;


        //FIXME: 如果直接返回FieldMatcher内部的stream，则将脱离fieldMatcher对于封包状态的管理
        public Stream Stream => new FieldMatcherStream(this);

        private int ResidualPackageBodyLength => PackageBodyLength - UsedBodyLength;
        

        public FieldMatcher(Stream stream)
        {
            // 获得封包长度，注意此处未赋值封包长度
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


        public T Match<T>()
            => (T)Match(typeof(T));

        public object Match(Type t)
        {
            if (t.GetInterface(nameof(IPackageField)) != null)
            {
                return MatchPackageField(t);
            }
            else
            {
                return MatchMetaType(t);
            }
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
            if (count < 0)
                throw new ArgumentException("读取的元素数必须大于0");

            if (disposedValue)
                throw new ArgumentException("FieldMatcher对象已被dispose");

            if (PackageBodyLength != 0)
            {
                UsedBodyLength += count;
                if (ResidualPackageBodyLength < 0) // 等于0则恰好读完
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
                        //Console.WriteLine($"未实现的封包: {PackageTypeCode:x}");
                        if (UsedBodyLength != 0)
                            throw new ArgumentException($"封包id为0x{PackageTypeCode:x2}的handler签名与协议不匹配");

                        //如果已使用的封包数据长度为0， 则表示未实现此封包的handler，此时需要FieldMatcher来平衡stream
                        var bytes = ReadBytes(ResidualPackageBodyLength);
                    }
                }
                disposedValue = true;
            }
        }

        public void Dispose()
            => Dispose(true);

        #endregion

    }


    public class FieldMatcherStream : Stream
    {
        private FieldMatcher _FieldMatcher;

        internal FieldMatcherStream(FieldMatcher fieldMatcher)
        {
            _FieldMatcher = fieldMatcher;
        }


        public override bool CanRead => true;

        public override bool CanSeek => false;

        public override bool CanWrite => false;

        public override long Length => _FieldMatcher.PackageBodyLength;

        public override long Position
        {
            get
            {
                throw new NotSupportedException();
            }

            set
            {
                throw new NotSupportedException();
            }
        }

        public override void Flush()
        {
            throw new NotSupportedException();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            Array.Copy(_FieldMatcher.ReadBytes(count), 0, buffer, offset, count);
            return count;
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotSupportedException();
        }

        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException();
        }
    }

}
