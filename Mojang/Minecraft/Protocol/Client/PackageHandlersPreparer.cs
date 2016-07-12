using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Mojang.Minecraft.Protocol
{
    
    public partial class EssentialClient
    {

        /*
        private static readonly Dictionary<State, Dictionary<int, PackageHandlerInfo>> PackageHandlers =
            (from handler in from method in typeof(EssentialClient).GetMethods(BindingFlags.NonPublic | BindingFlags.Instance)
                             let attribute = method.GetCustomAttribute<PackageHandlerAttribute>()
                             where attribute != null
                             select new { method, attribute }
             group handler by handler.attribute.State into result
             select new
             {
                 State = result.Key,
                 handlersPerState = result.ToDictionary(handler => handler.attribute.TypeCode, handler => new PackageHandlerInfo(handler.method, handler.attribute))
             }).ToDictionary(handlersPerStateInfo => handlersPerStateInfo.State, handlersPerStateInfo => handlersPerStateInfo.handlersPerState);


        private sealed class HandlerFieldInfo
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

        private sealed class PackageHandlerInfo
        {
            public delegate void PackageHandler(params object[] obj);

            public readonly PackageHandlerAttribute HandlerAttribute;
            public HandlerFieldInfo[] Fields => Array.ConvertAll(Method.GetParameters(), param => new HandlerFieldInfo(param));
            public string Name => Method.Name;
            private readonly MethodInfo Method;

            public PackageHandler this[EssentialClient clientInstance] => obj => Method.Invoke(clientInstance, obj);

            public PackageHandlerInfo(MethodInfo method, PackageHandlerAttribute attribute)
            {
                Method = method;
                HandlerAttribute = attribute;
            }
        }
        */
    }

}
