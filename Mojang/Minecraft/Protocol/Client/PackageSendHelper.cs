using Mojang.Minecraft.Protocol.Providers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Mojang.Minecraft.Protocol
{
    public partial class EssentialClient
    {
        


        [AttributeUsage(AttributeTargets.Method)]
        private sealed class PackageSenderAttribute : Attribute
        {
            public readonly int TypeCode;
            public readonly State State;

            /// <summary>
            /// 指定此封包发送方法所适用的封包id
            /// </summary> 
            public PackageSenderAttribute(int typeCode, State state = State.Play)
            {
                TypeCode = typeCode;
                State = state;
            }
        }

        public class ConnectStateException : Exception
        {
            public ConnectStateException(string message) : base(message)
            {
                
            }
        }


        protected async Task SendPackage(params object[] fields)
        {
            //只有应用了[PackageSender]的方法才能调用此方法
            //发送方法中不能有byte[]参数
            var sender = new StackTrace().GetFrame(5).GetMethod();
            var packageSenderAttribute = sender.GetCustomAttribute<PackageSenderAttribute>();
            if (packageSenderAttribute == null)
                throw new ArgumentException("调用方未应用[PackageSender]");

            if (packageSenderAttribute.State != _ConnectState)
                throw new ConnectStateException("当前状态无法满足目标发送器所需状态");

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

            var typeCode = (packageSenderAttribute as PackageSenderAttribute).TypeCode;
            var packageMaker = new PackageMaker(typeCode);

            foreach (var parameter in sender.GetParameters())
            {
                var parameterType = parameter.ParameterType;
                if (parameterType.IsGenericType && parameterType.GetGenericTypeDefinition() == typeof(Nullable<>))
                {
                    parameterType = parameterType.GetGenericArguments().First();
                }

                AppendToPackageMaker(packageMaker, fields[parameter.Position], parameterType, parameter.GetCustomAttributes());

            }
            await _ConnectProvider.Send(packageMaker.MakePackage());
        }


        void AppendToPackageMaker(PackageMaker packageMaker, object actualParameter, Type actualParameterType, IEnumerable<Attribute> formalParameterAttributes)
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
                    packageMaker.AppendPackageField(new VarInt((uint)array.Length));
                }
                else
                {
                    packageMaker.AppendMetaType(array.Length);
                }

                foreach (var element in array)
                {
                    AppendToPackageMaker(packageMaker, element, actualParameterType.GetElementType(), formalParameterAttributes);
                }

            }
            else if (formalParameterAttributes.Contains(new VariableAttribute()))
            {
                if (actualParameterType == typeof(int))
                    packageMaker.AppendPackageField(new VarInt((uint)(int)actualParameter));
                else if (actualParameterType == typeof(string))
                    packageMaker.AppendPackageField(new VarintPrefixedUTF8String((string)actualParameter));
                else throw new ArgumentException("参数应用了[Variable], 却不为int或string");
            }
            else if (FieldMatcher.IsMetaType(actualParameterType))
            {
                packageMaker.AppendMetaType(actualParameter);
            }
            else if (actualParameterType.GetInterfaces().Any((I) => I == typeof(IPackageField)))
                packageMaker.AppendPackageField((IPackageField)actualParameter);
            else if (actualParameterType.IsEnum)
            {
                if (actualParameterType.IsDefined(typeof(VarIntUnderlyingAttribute)))
                {
                    if (actualParameterType.GetEnumUnderlyingType() == typeof(int))
                    {
                        var val = (int)actualParameter;
                        var variableVal = new VarInt((uint)val);
                        packageMaker.AppendPackageField(variableVal);
                    }
                    else throw new ArgumentException("参数类型(枚举)应用了[VarIntUnderlying], 其基础类型却不为int");
                }
                else if (actualParameterType.IsDefined(typeof(StringUnderlyingAttribute)))
                    //如果目标实参在枚举类型中的字段应用了Renamed特性，则使用旧名称，否则使用枚举字段名称
                    packageMaker.AppendPackageField<VarintPrefixedUTF8String>
                    (
                        Array.Find(actualParameterType.GetField(actualParameter.ToString()).CustomAttributes.ToArray(), custom => custom.AttributeType == typeof(RenamedAttribute))
                        ?.ConstructorArguments.First().Value.ToString() ?? actualParameter.ToString()
                    );
                else //匹配对应的基础类型值并将其转换为对应枚举类型的实例
                {
                    var val = Array.Find(
                        typeof(Convert).GetMethods(BindingFlags.Static | BindingFlags.Public),
                        m => m.ReturnParameter.ParameterType == Enum.GetUnderlyingType(actualParameterType) && m.GetParameters().Length == 1)
                        .Invoke(null, new object[] { actualParameter });
                    packageMaker.AppendMetaType(val);
                }
            }
            //TODO:128
            else throw new ArgumentOutOfRangeException("参数类型超出了预计的范围");
        }


    }
}
