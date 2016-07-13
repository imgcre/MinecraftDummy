using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Mojang.Minecraft.Protocol.Providers
{
    internal abstract class HandlerManager<TOwner, THandlerAttribute, THandlerKey>
        where THandlerAttribute : Attribute, IHandlerAttribute<THandlerKey>
    {
        protected static readonly Dictionary<THandlerKey, HandlerInfo<TOwner, THandlerAttribute, THandlerKey>> Handlers =
            (from method in typeof(TOwner).GetMethods(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance)
             let attribute = method.GetCustomAttribute<THandlerAttribute>()
             where attribute != null
             select new HandlerInfo<TOwner, THandlerAttribute, THandlerKey>(method, attribute)
             ).ToDictionary(handlerInfo => handlerInfo.HandlerAttribute.HandlerKey, handlerInfo => handlerInfo);
        protected readonly TOwner OwnerInstance;


        public HandlerManager(TOwner ownerInstance)
        {
            OwnerInstance = ownerInstance;
        }


        /// <summary>
        /// 同步构造参数，异步调用方法
        /// </summary>
        public void InvokeHandler(THandlerKey handlerKey, FieldMatcher fieldMatcher)
        {
            if (!Handlers.Keys.Contains(handlerKey))
            {
                fieldMatcher.Clear();
                throw new HandlerNotFoundException { };
            }

            var actualParameters = GetActualParameters(Handlers[handlerKey], fieldMatcher);
            Task.Run(() => Handlers[handlerKey][OwnerInstance](actualParameters));
        }


        virtual protected object[] GetActualParameters(HandlerInfo<TOwner, THandlerAttribute, THandlerKey> handler, FieldMatcher fieldMatcher)
        {
            var actualParameterList = new List<object>();
            foreach (var field in handler.Fields)
                actualParameterList.Add(GetActualParameter(fieldMatcher, field.FieldType, field.Attributes, actualParameterList));
            var actualParameters = actualParameterList.ToArray();

            return actualParameters;
        }


        virtual protected object GetActualParameter(FieldMatcher fieldMatcher, Type formalParameterType, IEnumerable<Attribute> formalParameterAttributes, List<object> context)
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
                    return (string)fieldMatcher.MatchPackageField<PString>();
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
                    var currentFieldName = ((string)(fieldMatcher.MatchPackageField<PString>())).Split(':').Last();

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
            //TODO:宽度为128位的整数
            else throw new ArgumentOutOfRangeException("参数类型超出了预计的范围");
        }
    }


    internal class HandlerNotFoundException : Exception
    {

    }


    internal interface IHandlerAttribute<THandlerKey>
    {
        THandlerKey HandlerKey { get; }

    }


    sealed class HandlerFieldInfo
    {
        public readonly string Name;
        public readonly Type FieldType;
        public readonly IEnumerable<Attribute> Attributes;

        public HandlerFieldInfo(ParameterInfo parameter)
        {
            Name = parameter.Name;
            FieldType = parameter.ParameterType;
            Attributes = parameter.GetCustomAttributes();
        }

    }


    internal class HandlerInfo<TOwner, THandlerAttribute, THandlerKey>
        where THandlerAttribute : IHandlerAttribute<THandlerKey>
    {
        public delegate void Handler(params object[] obj);

        public readonly THandlerAttribute HandlerAttribute;
        public HandlerFieldInfo[] Fields => Array.ConvertAll(Method.GetParameters(), param => new HandlerFieldInfo(param));
        public string Name => Method.Name;
        private readonly MethodInfo Method;

        public Handler this[TOwner clientInstance] => obj => Method.Invoke(clientInstance, obj);

        public HandlerInfo(MethodInfo method, THandlerAttribute attribute)
        {
            Method = method;
            HandlerAttribute = attribute;
        }
    }

}