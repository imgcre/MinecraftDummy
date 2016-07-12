using Mojang.Minecraft.Protocol.Providers;
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

        void FromField(FieldMatcher fieldMatcher);

        /// <summary>
        /// 将对象序列化为byte[]
        /// </summary>
        /// <returns>序列化的结果</returns>
        void AppendIntoField(FieldMaker fieldMaker);
    }

}
