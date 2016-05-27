using Mojang.Minecraft.Protocol.Providers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Mojang.Minecraft.Protocol
{
    public partial class EssentialClient
    {
        


        private enum State
        {
            Handshaking,
            Status,
            Login,
            Play,
        }


        /// <summary>
        /// 定义当前方法为PackageHandler
        /// </summary>
        [AttributeUsage(AttributeTargets.Method)]
        private sealed class PackageHandlerAttribute : Attribute
        {
            public readonly int TypeCode;
            public readonly State State;

            public PackageHandlerAttribute(int typeCode, State state = State.Play)
            {
                TypeCode = typeCode;
                State = state;
            }
        }

        /// <summary>
        /// 此数组参数前缀长度字段变宽
        /// </summary>
        [AttributeUsage(AttributeTargets.Parameter)]
        private class VariableCountAttribute : Attribute { }


        public enum PrefixLengthFieldType
        {
            Byte,
            Short,
            Int,
        }

        /// <summary>
        /// 此数组参数前缀长度字段定宽, 且为byte, short或int中的一种类型
        /// </summary>
        [AttributeUsage(AttributeTargets.Parameter)]
        private class FixedCountAttribute : Attribute
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
            public FixedCountAttribute(PrefixLengthFieldType type)
            {
                CountType = new[] { typeof(byte), typeof(short), typeof(int) }[(int)type];
            }

        }

        /// <summary>
        /// 此参数是变宽的
        /// </summary>
        [AttributeUsage(AttributeTargets.Parameter)]
        private class VariableAttribute : Attribute { }

        //TODO
        /// <summary>
        /// 此参数是可选的
        /// </summary>
        [AttributeUsage(AttributeTargets.Parameter)]
        private class OptionalAttribute : Attribute
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
            public OptionalAttribute(Type enumType, params string[] enumNames)
            {
                Context = Array.ConvertAll(enumNames, name => (Enum)Enum.Parse(enumType, name));
            }

        }


        private enum UnderlyingType
        {
            Byte,
            Short,
            Int,
        }


        /// <summary>
        /// 此参数的类型以新枚举类型的基础类型获得字段，并将其转换为枚举类型
        /// </summary>
        [AttributeUsage(AttributeTargets.Parameter)]
        private class UnderlyingTypeRedefinedAttribute : Attribute
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
        private class VarIntUnderlyingAttribute : Attribute { }


        /// <summary>
        /// 此枚举的基础类型为VarintPrefixedUTF8String, 忽略大小写, 如果有比号, 则为比号之后的内容
        /// </summary>
        [AttributeUsage(AttributeTargets.Enum)]
        private class StringUnderlyingAttribute : Attribute { }


        /// <summary>
        /// 此枚举字段具有原始名称，区分大小写，且现有代码已经对其进行了重命名
        /// <para>要求含有此字段的枚举必须应用StringUnderlying特性</para>
        /// </summary>
        [AttributeUsage(AttributeTargets.Field)]
        public class RenamedAttribute : Attribute
        {
            public RenamedAttribute(string oldName)
            {

            }
        }


        private void OnPackageReceived(FieldMatcher fieldMatcher)
       {
            var typeCode = fieldMatcher.PackageTypeCode;

            if (!PackageHandlers[_ConnectState].Keys.Contains(typeCode))
                return;

            try
            {
                InvokePackageHandlerWithBytesFields(PackageHandlers[_ConnectState][typeCode], fieldMatcher);
            }
            catch (ArgumentException)
            {

            }

        }


        /// <summary>
        ///  从FieldMatcher中获取handler的实参，并异步调用handler
        /// </summary>
        /// <param name="handler"></param>
        /// <param name="fieldMatcher"></param>
        private void InvokePackageHandlerWithBytesFields(PackageHandlerInfo handler, FieldMatcher fieldMatcher)
        {
            var actualParameterList = new List<object>();
            foreach (var field in handler.Fields)
                actualParameterList.Add(GetActualParameter(fieldMatcher, field.FieldType, field.Attributes, actualParameterList));
            var actualParameters = actualParameterList.ToArray();
            Task.Run(() => handler[this](actualParameters));

            ShowPackageHandlerInvokeMessage(handler, actualParameters, new[] {
                nameof(OnSucessfullyLogined),
                nameof(OnDisconnected),
                nameof(OnRespawned),
            });
        }


        /// <summary>
        /// 在控制台中输出指定handler的签名及实参
        /// </summary>
        //[Conditional("Debug")]
        private void ShowPackageHandlerInvokeMessage(PackageHandlerInfo handler, object[] actualParameters, string[] handlerRange)
        {
            if (handlerRange.Contains(handler.Name))
            {
                var output = new StringBuilder($"handler's invoking info: {handler.Name}");
                output.AppendLine();

                for (var i = 0; i < actualParameters.Length; i++)
                    output.AppendLine($"{handler.Fields[i].Name}: {actualParameters[i]?.ToString() ?? "null"}");
                Console.WriteLine(output);
            }
        }


        /// <summary>
        /// 通过参数类型及参数应用的特性, 利用FieldMatcher构造实参
        /// </summary>
        /// <param name="fieldMatcher"></param>
        /// <param name="formalParameterType"></param>
        /// <param name="formalParameterAttributes"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        private object GetActualParameter(FieldMatcher fieldMatcher, Type formalParameterType, IEnumerable<Attribute> formalParameterAttributes, List<object> context)
        {
            var optionalAttributes = formalParameterAttributes.OfType<OptionalAttribute>();
            var optionalAttribute = default(OptionalAttribute);
            if (optionalAttributes.Count() > 0)
            {
                optionalAttribute = optionalAttributes.First();
            }

            if (optionalAttribute?.Context != null)
            {
                if (context.Intersect(optionalAttribute.Context).Count() == 0)
                    return null;
            }
            else
            {
                if (fieldMatcher.Empty)
                    return null;
            }


            if (formalParameterType.IsArray)
            {
                var arrayCount = default(int);

                var fixedCountAttribute = formalParameterAttributes.Where(attribute => attribute.GetType().Equals(typeof(FixedCountAttribute)));
                if (formalParameterAttributes.Contains(new VariableCountAttribute()))
                    arrayCount = fieldMatcher.MatchPackageField<VarInt>();
                else if (fixedCountAttribute.Count() != 0)
                {
                    arrayCount = (fieldMatcher.MatchMetaType((fixedCountAttribute.First() as FixedCountAttribute).CountType) as IConvertible).ToInt32(null);
                }
                else
                {
                    throw new NotImplementedException("handler未指定数组长度解析方式");
                }
                
                var elementType = formalParameterType.GetElementType();
                var actualArray = Array.CreateInstance(elementType, arrayCount);
                for (var i = 0; i < arrayCount; i++)
                {
                    actualArray.SetValue(GetActualParameter(fieldMatcher, elementType, formalParameterAttributes, context), i);
                }
                return actualArray;
            }
            else if (formalParameterAttributes.Contains(new VariableAttribute()))
            {
                if (formalParameterType == typeof(int))
                    return (int)fieldMatcher.MatchPackageField<VarInt>();
                else if (formalParameterType == typeof(string))
                    return (string)fieldMatcher.MatchPackageField<VarintPrefixedUTF8String>();
                else throw new ArgumentException("参数应用了[Variable], 却不为int或string");
            }
            else if (FieldMatcher.IsMetaType(formalParameterType))
            {
                return fieldMatcher.MatchMetaType(formalParameterType);
            }
            else if (formalParameterType.GetInterfaces().Any((I) => I == typeof(IPackageField)))
                return fieldMatcher.MatchPackageField(formalParameterType);
            else if (formalParameterType.IsEnum)
            {
                //如果指定了[VarIntUnderlying]或[StringUnderlying]则视为变宽类型
                if (formalParameterType.IsDefined(typeof(VarIntUnderlyingAttribute)))
                {
                    if (formalParameterType.GetEnumUnderlyingType() == typeof(int))
                    {
                        var val = (int)(fieldMatcher.MatchPackageField<VarInt>());
                        return Enum.ToObject(formalParameterType, val);
                    }
                    else throw new ArgumentException("参数类型(枚举)应用了[VarIntUnderlying], 其基础类型却不为int");
                }
                else if (formalParameterType.IsDefined(typeof(StringUnderlyingAttribute)))
                {
                    //由于是从string中解析枚举, 所以枚举的基础类型并不重要
                    var currentFieldName = ((string)(fieldMatcher.MatchPackageField<VarintPrefixedUTF8String>())).Split(':').Last();

                    //获得重命名字段旧名称与当前字段名称相同的字段
                    var renamedFields = from field in formalParameterType.GetFields()
                                        let renamedAttribute = Array.Find(field.CustomAttributes.ToArray(), custom => custom.AttributeType == typeof(RenamedAttribute))
                                        where renamedAttribute != null && renamedAttribute.ConstructorArguments.First().Value.Equals(currentFieldName)
                                        select field;

                    if (renamedFields.Count() > 1)
                        throw new ArgumentException("重命名枚举字段名称具有二义性");

                    if (renamedFields.Count() == 1)
                        currentFieldName = renamedFields.First().Name;

                    return Enum.Parse(formalParameterType, currentFieldName, true);
                }
                else //匹配对应的基础类型值并将其转换为对应枚举类型的实例
                {
                    //如果参数的枚举类型基础类型被重定义，则匹配新的基础类型
                    
                    var underlyingTypeRedefinedAttribute = formalParameterAttributes.TakeWhile(attribute => attribute.GetType().Equals(typeof(UnderlyingTypeRedefinedAttribute)));
                    var val = default(object);

                    if (underlyingTypeRedefinedAttribute.Count() > 0)
                        val = fieldMatcher.MatchMetaType((underlyingTypeRedefinedAttribute.First() as UnderlyingTypeRedefinedAttribute).NewUnderlyingType);
                    else
                        val = fieldMatcher.MatchMetaType(formalParameterType.GetEnumUnderlyingType());
                    return Enum.ToObject(formalParameterType, val);
                }
            }
            //TODO:128
            else throw new ArgumentOutOfRangeException("参数类型超出了预计的范围");
        }


    }
}
