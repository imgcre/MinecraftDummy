﻿using Mojang.Minecraft.Protocol.Providers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mojang.Minecraft
{
    /// <summary>
    /// 实现此接口的类型应包含一个以byte[]为参数的实例构造器
    /// </summary>
    internal interface IPackageField
    {

        void FromFieldMatcher(FieldMatcher fieldMatcher);

        /// <summary>
        /// 将对象序列化为byte[]
        /// </summary>
        /// <returns>序列化的结果</returns>
        void AppendIntoPackageMaker(PackageMaker packageMaker);
    }

    
    internal static class IBitConvertibleHelper
    {
        public static T TOIBitConvertible<T>(this byte[] data)
            where T : IPackageField, new()
        {
            var stream = new MemoryStream(data) { Position = 0 };
            var fieldMatcher = new FieldMatcher(stream);
            var o = new T();
            o.FromFieldMatcher(fieldMatcher);
            return o;
        }
    }
    

}
