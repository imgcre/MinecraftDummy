using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mojang.Minecraft.Protocol.Providers;

namespace Mojang.Minecraft
{
    public class BlockState : IPackageField
    {
        private int _BlockState;
        public int Id => _BlockState >> 4;
        public int Damage => _BlockState & 15;

        void IPackageField.AppendIntoField(FieldMaker fieldMaker)
        {
            throw new NotImplementedException();
        }

        void IPackageField.FromField(FieldMatcher fieldMatcher)
        {
            _BlockState = fieldMatcher.Match<VarInt>();
        }
    }
}
