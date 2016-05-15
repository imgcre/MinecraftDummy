using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Mojang.Minecraft.Protocol.Providers
{
    public static class BigEndianBitConverter
    {
        public static byte[] GetBytes(bool value)
            => BitConverter.GetBytes(value).ReverseBytes();

        public static byte[] GetBytes(char value)
            => BitConverter.GetBytes(value).ReverseBytes();

        public static byte[] GetBytes(double value)
            => BitConverter.GetBytes(value).ReverseBytes();

        public static byte[] GetBytes(float value)
            => BitConverter.GetBytes(value).ReverseBytes();

        public static byte[] GetBytes(int value)
            => BitConverter.GetBytes(value).ReverseBytes();
        public static byte[] GetBytes(uint value)
            => BitConverter.GetBytes(value).ReverseBytes();

        public static byte[] GetBytes(long value)
            => BitConverter.GetBytes(value).ReverseBytes();

        public static byte[] GetBytes(short value)
            => BitConverter.GetBytes(value).ReverseBytes();

        public static byte[] GetBytes(ushort value)
            => BitConverter.GetBytes(value).ReverseBytes();

        public static byte[] GetBytes(ulong value)
            => BitConverter.GetBytes(value).ReverseBytes();

        /*
        利用ide的正则替换
        将
        (public static (\w*?) To(\w*?)\(byte\[\] value, int startIndex\));
        替换为
        $1\r\n             => BitConverter.To$3(value.TakeLittleEndianBytes<$2>(startIndex), 0);
        */
        public static bool ToBoolean(byte[] value, int startIndex)
            => BitConverter.ToBoolean(value.TakeLittleEndianBytes<bool>(startIndex), 0);

        public static char ToChar(byte[] value, int startIndex)
            => BitConverter.ToChar(value.TakeLittleEndianBytes<char>(startIndex), 0);
        
        public static double ToDouble(byte[] value, int startIndex)
            => BitConverter.ToDouble(value.TakeLittleEndianBytes<double>(startIndex), 0);
        
        public static short ToInt16(byte[] value, int startIndex)
            => BitConverter.ToInt16(value.TakeLittleEndianBytes<short>(startIndex), 0);
         
        public static int ToInt32(byte[] value, int startIndex)
            => BitConverter.ToInt32(value.TakeLittleEndianBytes<int>(startIndex), 0);

        public static long ToInt64(byte[] value, int startIndex)
            => BitConverter.ToInt64(value.TakeLittleEndianBytes<long>(startIndex), 0);

        public static float ToSingle(byte[] value, int startIndex)
            => BitConverter.ToSingle(value.TakeLittleEndianBytes<float>(startIndex), 0);

        public static ushort ToUInt16(byte[] value, int startIndex)
            => BitConverter.ToUInt16(value.TakeLittleEndianBytes<uint>(startIndex), 0);
        
        public static uint ToUInt32(byte[] value, int startIndex)
            => BitConverter.ToUInt32(value.TakeLittleEndianBytes<uint>(startIndex), 0);

        public static ulong ToUInt64(byte[] value, int startIndex)
            => BitConverter.ToUInt64(value.TakeLittleEndianBytes<ulong>(startIndex), 0);


        private static byte[] ReverseBytes(this byte[] input)
        {
            var output = input.Clone() as byte[];
            Array.Reverse(output);
            return output;
        }


        private static byte[] TakeLittleEndianBytes<T>(this byte[] input, int startIndex)
        {
            var output = new byte[Marshal.SizeOf<T>()];
            Array.Copy(input, startIndex, output, 0, output.Length);
            return output.ReverseBytes();
        }

    }
}
