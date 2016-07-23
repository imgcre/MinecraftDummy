using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mojang.Minecraft.Protocol.Providers;

namespace Mojang.Minecraft
{
    public class BlockRecord : IPackageField
    {
        public int X { get; private set; }
        public int Y { get; private set; }
        public int Z { get; private set; }
        public BlockState BlockState { get; private set; }

        void IPackageField.AppendIntoField(FieldMaker fieldMaker)
        {
            throw new NotImplementedException();
        }

        void IPackageField.FromField(FieldMatcher fieldMatcher)
        {
            var byt = fieldMatcher.Match<byte>();
            X = byt >> 4;
            Z = byt & 15;
            Y = fieldMatcher.Match<byte>();
            BlockState = fieldMatcher.Match<BlockState>();
        }
    }
}
