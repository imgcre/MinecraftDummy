﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mojang.Minecraft.Protocol.Providers;
using fNbt;

namespace Mojang.Minecraft
{
    public sealed class Nbt : IPackageField
    {
        private NbtFile _NbtFile;
        public NbtCompound Root => _NbtFile.RootTag;

        public Nbt()
        {
            _NbtFile = new NbtFile();
        }


        public Nbt(NbtFile nbtFile)
        {
            _NbtFile = nbtFile;
        }


        void IPackageField.FromField(FieldMatcher fieldMatcher)
        {
            try
            {
                _NbtFile.LoadFromStream(fieldMatcher.Stream, NbtCompression.None);
            }
            catch (NbtFormatException)
            {
                //读出0, nbt字段为空
            }
            
        }


        void IPackageField.AppendIntoField(FieldMaker fieldMaker)
        {
            if (Root.Count == 0)
                fieldMaker.AppendMetaType((byte)0);
            else //packageMaker.AppendBytes(_NbtFile.SaveToBuffer(_NbtFile.FileCompression));
                fieldMaker.AppendBytes(_NbtFile.SaveToBuffer(NbtCompression.None));
        }
    }
}
