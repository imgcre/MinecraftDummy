using Mojang.Minecraft.Protocol.Providers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Mojang.Minecraft
{
    public struct Position: IPackageField
    {
        /// <summary>
        /// 原点坐标
        /// </summary>
        public static Position Origin = new Position(0, 0, 0);

        /// <summary>
        /// x: 26位最高有效位
        /// z: 26位最低有效位
        /// y: 其中的12位
        /// </summary>
        public long X { get; set; }
        public long Y { get; set; }
        public long Z { get; set; }


        /// <summary>
        /// 从字符串中解析Position, 格式X,Y,Z
        /// </summary>
        /// <param name="s"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        public static bool TryParse(string s, out Position result)
        {
            try
            {
                result = Parse(s);
            }
            catch (FormatException)
            {
                result = new Position();
                return false;
            }
            return true;
        }


        public static Position Parse(string s)
        {
            var stringArray = s.Split(',');
            if (stringArray.Length != 3)
                throw new FormatException();
            return new Position(int.Parse(stringArray[0]), int.Parse(stringArray[1]), int.Parse(stringArray[2]));
        }


        public Position(long x, long y, long z)
        {
            X = x;
            Y = y;
            Z = z;
        }


        public Position(long position)
        {
            X = position >> 38;
            Y = (position >> 26) & 0xFFF;
            Z = position & 0x3ffffff;
        }


        public override string ToString() => $"{X},{Y},{Z}";


        public long ToLong()
        {
            return ((X & 0x3FFFFFF) << 38) | ((Y & 0xFFF) << 26) | (Z & 0x3FFFFFF);
        }


        /*
        internal byte[] ToBytes()
        {
            return BigEndianBitConverter.GetBytes(ToLong());
        }
        */

        void IPackageField.AppendIntoField(FieldMaker fieldMaker)
            => fieldMaker.AppendMetaType(ToLong());
        

        void IPackageField.FromField(FieldMatcher fieldMatcher)
        {
            this = new Position(fieldMatcher.MatchMetaType<long>());
        }


        public static implicit operator long (Position position)
            => position.ToLong();


        public static implicit operator Position (long position)
            => new Position(position);

    }
}
