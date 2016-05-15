using Mojang.Minecraft.Protocol.Providers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mojang.Minecraft
{
    /// <summary>
    /// UTF-8编码的字符串，首位是字符串长度，长于2，短于240
    /// </summary>
    public class VarintPrefixedUTF8String : IPackageField
    {
        protected string _InnerString;


        public VarintPrefixedUTF8String()
        {
        }


        public VarintPrefixedUTF8String(string str)
        {
            _InnerString = str ?? string.Empty;
        }


        public override string ToString() => _InnerString;


        internal virtual void AppendIntoPackageMaker(PackageMaker packageMaker)
        {
            packageMaker.AppendVarintLengthPrefixedBytes(Encoding.UTF8.GetBytes(_InnerString));
        }


        void IPackageField.AppendIntoPackageMaker(PackageMaker packageMaker)
        {
            AppendIntoPackageMaker(packageMaker);
        }

        void IPackageField.FromFieldMatcher(FieldMatcher fieldMatcher)
        {
            FromFieldMatcher(fieldMatcher);
        }


        internal virtual void FromFieldMatcher(FieldMatcher fieldMatcher)
        {
            var stringLength = fieldMatcher.MatchPackageField<VarInt>();
            var stringData = fieldMatcher.ReadBytes(stringLength);
            if (stringData.Length != stringLength)
                throw new ArgumentException("字符串长度错误");
            _InnerString = Encoding.UTF8.GetString(stringData);
        }



        public static implicit operator string (VarintPrefixedUTF8String str)
            => str.ToString();


        public static implicit operator VarintPrefixedUTF8String (string str)
            => new VarintPrefixedUTF8String(str);

    }
}
