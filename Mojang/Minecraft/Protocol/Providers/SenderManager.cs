using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Mojang.Minecraft.Protocol.Providers
{
    internal abstract class SenderManager<TOwner, TSenderAttribute, TSenderKey>
        where TSenderAttribute : Attribute, ISenderAttribute<TSenderKey>
    {
        protected MethodBase _CurrentSender { get; private set; }
        protected TSenderAttribute _CurrentAttribute { get; private set; }

        protected readonly TOwner OwnerInstance;


        public SenderManager(TOwner ownerInstance)
        {
            OwnerInstance = ownerInstance;
        }

        /*
         * 当某方法调用Send方法时，通过调用堆栈获得caller签名，判断caller签名是否合法
         * 通过caller签名构造TPackageMaker，将实参通过AppendIntoPackage转为字节流
         * 打包字节流(MakePackage)，完成对封包的构造
         * 
         * 对于顶级封包，需要驱动方法来完成封包的发送
         */

        protected virtual bool IdentifySender(TSenderAttribute attribute)
            => attribute != null;


        protected virtual async Task<FieldMaker> SendInternal(MethodBase sender, TSenderAttribute attribute, params object[] fields)
        {
            return await Task.Run(() => 
            {
                //只有应用了[PackageSender]的方法才能调用此方法
                //发送方法中不能有byte[]参数
                if (!IdentifySender(attribute))
                    throw new ArgumentException("非法的调用");

                //检查object[]中的类型是否与sender的参数类型相匹配
                if (sender.GetParameters().Length != fields.Length)
                    throw new ArgumentException("传入的参数数组长度与sender形参数量不同");
                foreach (var parameter in sender.GetParameters())
                {
                    if (fields[parameter.Position] == null)
                        break;

                    var baredParamType = parameter.ParameterType;

                    if (parameter.ParameterType.IsGenericType && parameter.ParameterType.GetGenericTypeDefinition() == typeof(Nullable<>))
                    {
                        baredParamType = parameter.ParameterType.GetGenericArguments().First();
                    }

                    if (fields[parameter.Position].GetType() != baredParamType)
                    {
                        throw new ArgumentException("传入的参数类型与sender形参类型不同");
                    }
                }

                var fieldMaker = new FieldMaker();

                foreach (var parameter in sender.GetParameters())
                {
                    var parameterType = parameter.ParameterType;
                    if (parameterType.IsGenericType && parameterType.GetGenericTypeDefinition() == typeof(Nullable<>))
                    {
                        parameterType = parameterType.GetGenericArguments().First();
                    }

                    AppendIntoField(fieldMaker, fields[parameter.Position], parameterType, parameter.GetCustomAttributes());

                }

                return fieldMaker;
            });
        }


        public async Task Send(params object[] fields)
        {
            //只有应用了[PackageSender]的方法才能调用此方法
            //发送方法中不能有byte[]参数
            var sender = new StackTrace().GetFrame(5).GetMethod();
            var senderAttribute = sender.GetCustomAttribute<TSenderAttribute>();

            await SendInternal(sender, senderAttribute, fields);
        }


        protected virtual void AppendIntoField(FieldMaker fieldMaker, object actualParameter, Type actualParameterType, IEnumerable<Attribute> formalParameterAttributes)
        {

            if (formalParameterAttributes.Contains(new OptionalAttribute()) && actualParameter == null)
            {
                return;
            }

            if (actualParameterType.IsArray)
            {
                Array array = actualParameter as Array;
                if (formalParameterAttributes.Contains(new VariableCountAttribute()))
                {
                    fieldMaker.AppendPackageField(new VarInt((uint)array.Length));
                }
                else
                {
                    fieldMaker.AppendMetaType(array.Length);
                }

                foreach (var element in array)
                {
                    AppendIntoField(fieldMaker, element, actualParameterType.GetElementType(), formalParameterAttributes);
                }

            }
            else if (formalParameterAttributes.Contains(new VariableAttribute()))
            {
                if (actualParameterType == typeof(int))
                    fieldMaker.AppendPackageField(new VarInt((uint)(int)actualParameter));
                else if (actualParameterType == typeof(string))
                    fieldMaker.AppendPackageField(new PString((string)actualParameter));
                else throw new ArgumentException("参数应用了[Variable], 却不为int或string");
            }
            else if (FieldMatcher.IsMetaType(actualParameterType))
            {
                fieldMaker.AppendMetaType(actualParameter);
            }
            else if (actualParameterType.GetInterfaces().Any((I) => I == typeof(IPackageField)))
                fieldMaker.AppendPackageField((IPackageField)actualParameter);
            else if (actualParameterType.IsEnum)
            {
                if (actualParameterType.IsDefined(typeof(VarIntUnderlyingAttribute)))
                {
                    if (actualParameterType.GetEnumUnderlyingType() == typeof(int))
                    {
                        var val = (int)actualParameter;
                        var variableVal = new VarInt((uint)val);
                        fieldMaker.AppendPackageField(variableVal);
                    }
                    else throw new ArgumentException("参数类型(枚举)应用了[VarIntUnderlying], 其基础类型却不为int");
                }
                else if (actualParameterType.IsDefined(typeof(StringUnderlyingAttribute)))
                    //如果目标实参在枚举类型中的字段应用了Renamed特性，则使用旧名称，否则使用枚举字段名称
                    fieldMaker.AppendPackageField<PString>
                    (
                        Array.Find(actualParameterType.GetField(actualParameter.ToString()).CustomAttributes.ToArray(), custom => custom.AttributeType == typeof(RenamedAttribute))
                        ?.ConstructorArguments.First().Value.ToString() ?? actualParameter.ToString()
                    );
                else //匹配对应的基础类型值并将其转换为对应枚举类型的实例
                {
                    //如果参数的枚举类型基础类型被重定义，则将枚举类型转为此重定义类型
                    var underlyingType = default(Type);
                    var underlyingTypeRedefinedAttributes = formalParameterAttributes.TakeWhile(attribute => attribute.GetType().Equals(typeof(UnderlyingTypeRedefinedAttribute)));
                    if (underlyingTypeRedefinedAttributes.Count() > 0)
                        underlyingType = (underlyingTypeRedefinedAttributes.First() as UnderlyingTypeRedefinedAttribute).NewUnderlyingType;
                    else
                        underlyingType = Enum.GetUnderlyingType(actualParameterType);


                    var val = Array.Find(
                        typeof(Convert).GetMethods(BindingFlags.Static | BindingFlags.Public),
                        m => m.ReturnParameter.ParameterType == underlyingType && m.GetParameters().Length == 1)
                        .Invoke(null, new object[] { actualParameter });
                    fieldMaker.AppendMetaType(val);
                }
            }
            //TODO:宽度为128的整数发送侧
            else throw new ArgumentOutOfRangeException("参数类型超出了预计的范围");
        }
    }

    internal interface ISenderAttribute<TSenderKey>
    {
        TSenderKey SenderKey { get; }

    }



}
