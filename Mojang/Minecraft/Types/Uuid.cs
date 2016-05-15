﻿using Mojang.Minecraft.Protocol.Providers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Mojang.Minecraft
{
    /// <summary>
    /// 由两个长整形组成
    /// </summary>
    //In an example UUID, xxxxxxxx-xxxx-Yxxx-xxxx-xxxxxxxxxxxx
    //uint A, ushrot B, ushort C, byte D, byte E, byte F, byte G, byte H, byte I, byte J, byte K
    public sealed class Uuid : IPackageField
    {
        public int A { get; private set; }
        public short B { get; private set; }
        public short C { get; private set; }
        public byte D { get; private set; }
        public byte E { get; private set; }
        public byte F { get; private set; }
        public byte G { get; private set; }
        public byte H { get; private set; }
        public byte I { get; private set; }
        public byte J { get; private set; }
        public byte K { get; private set; }


        public Uuid()
        {

        }


        public Uuid(int a, short b, short c, byte d, byte e, byte f, byte g, byte h, byte i, byte j, byte k)
        {
            A = a;
            B= b;
            C= c;
            D= d;
            E = e;
            F = f;
            G = g;
            H = h;
            I = i;
            J = j;
            K = k;
        }


        void IPackageField.FromFieldMatcher(FieldMatcher fieldMatcher)
        {
            A = fieldMatcher.MatchMetaType<int>();
            B = fieldMatcher.MatchMetaType<short>();
            C = fieldMatcher.MatchMetaType<short>();
            D = fieldMatcher.MatchMetaType<byte>();
            E = fieldMatcher.MatchMetaType<byte>();
            F = fieldMatcher.MatchMetaType<byte>();
            G = fieldMatcher.MatchMetaType<byte>();
            H = fieldMatcher.MatchMetaType<byte>();
            I = fieldMatcher.MatchMetaType<byte>();
            J = fieldMatcher.MatchMetaType<byte>();
            K = fieldMatcher.MatchMetaType<byte>();
        }


        void IPackageField.AppendIntoPackageMaker(PackageMaker packageMaker)
        {
            packageMaker.AppendMetaType(A);
            packageMaker.AppendMetaType(B);
            packageMaker.AppendMetaType(C);
            packageMaker.AppendMetaType(D);
            packageMaker.AppendMetaType(E);
            packageMaker.AppendMetaType(F);
            packageMaker.AppendMetaType(G);
            packageMaker.AppendMetaType(H);
            packageMaker.AppendMetaType(I);
            packageMaker.AppendMetaType(J);
            packageMaker.AppendMetaType(K);
        }


        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            if (obj == this) return true;
            var uuidObj = obj as Uuid;
            if (uuidObj == null) return false;
            else
                if (uuidObj.A == A && uuidObj.B == B && uuidObj.C == C && uuidObj.D == D && uuidObj.E == E && uuidObj.F == F && uuidObj.G == G && uuidObj.H == H && uuidObj.I == I && uuidObj.J == J && uuidObj.K == K)
                    return true;
                else return false;
        }


        public override int GetHashCode() 
            => A.GetHashCode() ^ B.GetHashCode() ^ C.GetHashCode() ^ D.GetHashCode() ^ E.GetHashCode() ^ F.GetHashCode() ^ G.GetHashCode() ^ H.GetHashCode() ^ I.GetHashCode() ^ J.GetHashCode() ^ K.GetHashCode();


        public override string ToString()
            => new Guid(A, B, C, D, E, F, G, H, I, J, K).ToString();

    }
}
