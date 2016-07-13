using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mojang.Minecraft.Protocol.Providers
{
    [AttributeUsage(AttributeTargets.Parameter)]
    internal class VariableCountAttribute : Attribute { }


    public enum FixedCountType
    {
        Byte,
        Short,
        Int,
    }

    /// <summary>
    /// 此数组参数前缀长度字段定宽, 且为byte, short或int中的一种类型
    /// </summary>
    [AttributeUsage(AttributeTargets.Parameter)]
    internal class FixedCountAttribute : Attribute
    {
        public Type CountType;
        public FixedCountAttribute()
        {
            CountType = typeof(int);
        }




        /// <summary>
        /// 
        /// </summary>
        /// <param name="type">前缀长度字段类型</param>
        public FixedCountAttribute(FixedCountType type)
        {
            CountType = new[] { typeof(byte), typeof(short), typeof(int) }[(int)type];
        }

    }

    /// <summary>
    /// 此参数是变宽的
    /// </summary>
    [AttributeUsage(AttributeTargets.Parameter)]
    internal class VariableAttribute : Attribute { }

    /// <summary>
    /// 此参数是可选的
    /// </summary>
    [AttributeUsage(AttributeTargets.Parameter)]
    internal class OptionalAttribute : Attribute
    {
        public readonly Enum[] Context;

        public OptionalAttribute()
        {

        }

        /// <summary>
        /// 构造有条件的可选特性
        /// </summary>
        /// <param name="enumType">枚举类型</param>
        /// <param name="enumNames">枚举类型的字段名称</param>
        public OptionalAttribute(params object[] enums)
        {
            Context = Array.ConvertAll(enums, enu => (Enum)enu);
        }

        

    }

    internal enum UnderlyingType
    {
        Byte,
        Short,
        Int,
    }

    /// <summary>
    /// 此参数的类型以新枚举类型的基础类型获得字段，并将其转换为枚举类型
    /// </summary>
    [AttributeUsage(AttributeTargets.Parameter)]
    internal class UnderlyingTypeRedefinedAttribute : Attribute
    {
        public readonly Type NewUnderlyingType;

        public UnderlyingTypeRedefinedAttribute(UnderlyingType newUnderlyingType)
        {
            NewUnderlyingType = new Type[] { typeof(byte), typeof(short), typeof(int) }[(int)newUnderlyingType];
        }

    }


    /// <summary>
    /// 此枚举的基础类型为Varint
    /// </summary>
    [AttributeUsage(AttributeTargets.Enum)]
    internal class VarIntUnderlyingAttribute : Attribute { }


    /// <summary>
    /// 此枚举的基础类型为VarintPrefixedUTF8String, 忽略大小写, 如果有比号, 则为比号之后的内容
    /// </summary>
    [AttributeUsage(AttributeTargets.Enum)]
    internal class StringUnderlyingAttribute : Attribute { }


    /// <summary>
    /// 此枚举字段具有原始名称，区分大小写，且现有代码已经对其进行了重命名
    /// <para>要求含有此字段的枚举必须应用StringUnderlying特性</para>
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    internal class RenamedAttribute : Attribute
    {
        public RenamedAttribute(string oldName)
        {

        }
    }

}
