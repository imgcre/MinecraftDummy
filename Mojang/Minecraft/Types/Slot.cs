using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using fNbt;
using Mojang.Minecraft.Protocol.Providers;
using System.Text.RegularExpressions;

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

        public Slot(short itemId, byte itemCount, short itemDamage) : this()
        {
            ItemId = itemId;
            ItemCount = itemCount;
            ItemDamage = itemDamage;
        }

        public Slot(NbtCompound nbtCompound)
        {
            ItemId = 0;
            ItemCount = 1;
            _Nbt = new Nbt(new NbtFile(nbtCompound));
        }


        void IPackageField.FromField(FieldMatcher fieldMatcher)
        {
            ItemId = fieldMatcher.MatchMetaType<short>();
            if (ItemId == -1) return;

            ItemCount = fieldMatcher.MatchMetaType<byte>();

            ItemDamage = fieldMatcher.MatchMetaType<short>();

            _Nbt = fieldMatcher.MatchPackageField<Nbt>();
        }


        void IPackageField.AppendIntoField(FieldMaker fieldMaker)
        {
            if (ItemId == -1)
            {
                fieldMaker.AppendMetaType(ItemId);
                return;
            }

            fieldMaker.AppendMetaType(ItemId);
            fieldMaker.AppendMetaType(ItemCount);
            fieldMaker.AppendMetaType(ItemDamage);
            fieldMaker.AppendPackageField(_Nbt);
        }


        public override string ToString()
        {
            return string.Format("itemId={0}&itemCount={1}&itemDamage={2}&nbtRoot={3}", ItemId, ItemCount, ItemDamage, Root);
        }


        public static bool TryParse(string s, out Slot result)
        {
            try
            {
                result = Parse(s);
            }
            catch (FormatException)
            {
                result = new Slot();
                return false;
            }
            return true;
        }


        /*Slot语法
         * id:count:damage

        */
        public static Slot Parse(string s)
        {
            var regex = new Regex(@"^(\d+):(\d+):(\d+)$");
            if(!regex.IsMatch(s))
                throw new FormatException();

            var match = regex.Match(s);
            var slot = new Slot
            {
                ItemId = short.Parse($"{match.Groups[1]}"),
                ItemCount = byte.Parse($"{match.Groups[2]}"),
                ItemDamage = short.Parse($"{match.Groups[3]}"),
            };

            return slot;
        }

    }
}
