using Mojang.Minecraft.Protocol.Providers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mojang.Minecraft
{
    /// <summary>
    /// http://zlx19900228.iteye.com/blog/1058659
    /// </summary>
    internal class VarInt : IPackageField
    {
        public const int MaxLength = 5;

        private uint _InnerInteger;


        public VarInt() { }


        public VarInt(uint integer)
        {
            _InnerInteger = integer;
        }

        private bool GetByteMsb(int b) 
            => b >> 7 == 1;

        private bool IsEndOfStream(int b) 
            => b == -1;


        public override string ToString()
            => _InnerInteger.ToString();


        public uint ToUint()
            => _InnerInteger;


        void IPackageField.AppendIntoField(FieldMaker fieldMaker)
        {
            var list = new List<byte>();
            var integer = _InnerInteger;
            do
            {
                list.Add(Convert.ToByte((integer & 0x7f) | 0x80));
                integer >>= 7;
            }
            while (integer != 0);
            list[list.Count - 1] &= 0x7f;
            fieldMaker.AppendBytes(list);
        }

        void IPackageField.FromField(FieldMatcher fieldMatcher)
        {
            //从stream中获得varint
            //获得一个字节，判断该字节的msb是否为1，如果为1，则存在下一个字节，如果超过msb为1的字节超过4个，则参数有问题
            //如果发生问题，抛出异常，不负责恢复状态

            var varintByteList = new List<byte>();

            var currentByte = default(int);
            do
            {
                currentByte = fieldMatcher.ReadByte();
                if (IsEndOfStream(currentByte))
                    throw new ArgumentException("varint被意外截断");

                varintByteList.Add((byte)currentByte);
                //如果集合中已有元素超过5个，则varint过大
                if (varintByteList.Count > 5)
                    throw new ArgumentException("varint长度过长");

            } while (GetByteMsb((byte)currentByte));

            var integer = default(uint);
            var i = 0;
            do
            {
                var currentSwitchLength = i == 4 ? 4 : 7;
                integer >>= currentSwitchLength;
                integer |= ((uint)varintByteList[i] & 0x7f) << (32 - currentSwitchLength);
            }
            //前面已经可以保证数组最后一个元素&0x80的值为0
            //当前元素&0x80的值为0时，循环即可终止
            while ((varintByteList[i++] & 0x80) != 0);

            var leftSwitchCount = 32 - i * 7;
            if (leftSwitchCount < 0)
                leftSwitchCount = 0;
            integer >>= leftSwitchCount;
            _InnerInteger = integer;
        }


        public int Length
        {
            get
            {
                var i = 0;
                while ((ulong)_InnerInteger >> (7 * ++i) != 0) ;
                return i;
            }
        }


    public static implicit operator uint (VarInt varInt) 
            => varInt._InnerInteger;


        public static implicit operator VarInt (uint integer)
            => new VarInt { _InnerInteger = integer };


        public static implicit operator int (VarInt varInt)
            => (int)varInt._InnerInteger;


        public static implicit operator VarInt(int integer)
            => new VarInt { _InnerInteger = (uint)integer };
    }
}
