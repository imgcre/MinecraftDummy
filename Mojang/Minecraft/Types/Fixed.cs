using Mojang.Minecraft.Protocol.Providers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Mojang.Minecraft
{
    public class Fixed : IPackageField
    {
        public int Fraction => Raw & 0x1f;

        public int Integer => Raw >> 5;

        private int Raw;


        void IPackageField.FromFieldMatcher(FieldMatcher fieldMatcher)
            => Raw = fieldMatcher.MatchMetaType<int>();
        

        void IPackageField.AppendIntoPackageMaker(PackageMaker packageMaker) 
            => packageMaker.AppendMetaType(Raw);


        public static implicit operator double (Fixed f)
        {
            var fraction = default(double);
            if (f.Fraction != 0)
            {
                fraction = 1 / (double)f.Fraction;
            }
            return f.Integer + fraction;
        }


        public override string ToString() => ((double)this).ToString();

    }
}
