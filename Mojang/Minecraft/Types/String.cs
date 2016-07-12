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
    public class PString : IPackageField
    {
        protected string _InnerString;


        public PString()
        {
        }


        public PString(string str)
        {
            _InnerString = str ?? string.Empty;
        }


        public override string ToString() => _InnerString;


        internal virtual void AppendIntoField(FieldMaker fieldMaker)
        {
            var bytes = Encoding.UTF8.GetBytes(_InnerString);
            var length = new VarInt((uint)bytes.Length);

            fieldMaker.AppendPackageField(length);
            fieldMaker.AppendBytes(bytes);

            //fieldMaker.AppendVarintLengthPrefixedBytes(Encoding.UTF8.GetBytes(_InnerString));
        }


        void IPackageField.AppendIntoField(FieldMaker fieldMaker)
        {
            AppendIntoField(fieldMaker);
        }

        void IPackageField.FromField(FieldMatcher fieldMatcher)
        {
            FromField(fieldMatcher);
        }


        internal virtual void FromField(FieldMatcher fieldMatcher)
        {
            var stringLength = fieldMatcher.MatchPackageField<VarInt>();
            var stringData = fieldMatcher.ReadBytes(stringLength);
            if (stringData.Length != stringLength)
                throw new ArgumentException("字符串长度错误");
            _InnerString = Encoding.UTF8.GetString(stringData);
        }



        public static implicit operator string (PString str)
            => str.ToString();


        public static implicit operator PString (string str)
            => new PString(str);

    }
}
