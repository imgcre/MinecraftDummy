﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mojang.Minecraft.Protocol.Providers;

namespace Mojang.Minecraft
{
    internal class ChannelDataReciever : IPackageField
    {

        public FieldMatcher ChannelDataMatcher{ get; private set; }

        void IPackageField.FromFieldMatcher(FieldMatcher fieldMatcher)
        {
            //ChannelDataMatcher = new FieldMatcher(fieldMatcher.ReadElementsFromStream(fieldMatcher.Count).ToList());
        }


        void IPackageField.AppendIntoPackageMaker(PackageMaker packageMaker)
        {
            throw new NotImplementedException();
        }

    }



}

