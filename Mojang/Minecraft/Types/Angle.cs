using Mojang.Minecraft.Protocol.Providers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Mojang.Minecraft
{
    /// <summary>
    /// 一个用于表示角度的精确度较低的结构
    /// </summary>
    public class Angle : IPackageField
    {
        //0 = 0度, 255 = 360度
        private byte _Vaule;

        public Angle()
        {

        }
        

        void IPackageField.FromFieldMatcher(FieldMatcher fieldMatcher)
        {
            _Vaule = fieldMatcher.MatchMetaType<byte>();
        }


        void IPackageField.AppendIntoPackageMaker(PackageMaker packageMaker) 
            => packageMaker.AppendMetaType(_Vaule);


        public static implicit operator float(Angle angle) => angle._Vaule / 255f * 360;


        public static implicit operator Angle (float angle) => new Angle { _Vaule = (byte)(Math.Abs(angle % 360) / 360 * 255) };


        public override string ToString()
            => ((float)this).ToString();

    }
}
