using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using fNbt;
using Mojang.Minecraft.Protocol.Providers;

namespace Mojang.Minecraft
{
    public sealed class Slot : IPackageField
    {

        /// <summary>
        /// 如果为-1则表示此slot结构为空
        /// </summary>
        public short ItemId { get; private set; }
        public byte ItemCount { get; private set; }
        public short ItemDamage { get; private set; }

        private Nbt _Nbt;
        public NbtCompound Root => _Nbt.Root;


        public Slot()
        {
            _Nbt = new Nbt();
        }


        public Slot(short itemId) : this()
        {
            ItemId = itemId;
            ItemCount = 1;
        }

        public Slot(NbtCompound nbtCompound)
        {
            ItemId = 0;
            ItemCount = 1;
            _Nbt = new Nbt(new NbtFile(nbtCompound));
        }


        void IPackageField.FromFieldMatcher(FieldMatcher fieldMatcher)
        {
            ItemId = fieldMatcher.MatchMetaType<short>();
            if (ItemId == -1) return;

            ItemCount = fieldMatcher.MatchMetaType<byte>();

            ItemDamage = fieldMatcher.MatchMetaType<short>();

            _Nbt = fieldMatcher.MatchPackageField<Nbt>();
        }


        void IPackageField.AppendIntoPackageMaker(PackageMaker packageMaker)
        {
            if (ItemId == -1)
            {
                packageMaker.AppendMetaType(ItemId);
                return;
            }

            packageMaker.AppendMetaType(ItemId);
            packageMaker.AppendMetaType(ItemCount);
            packageMaker.AppendMetaType(ItemDamage);
            packageMaker.AppendPackageField(_Nbt);
        }


        public override string ToString()
        {
            return string.Format("itemId={0}&itemCount={1}&itemDamage={2}&nbtRoot={3}", ItemId, ItemCount, ItemDamage, Root);
        }

    }
}
