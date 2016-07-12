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
        private void OnPackageReceived(FieldMatcher fieldMatcher)
       {
            try
            {
                _PackageHandlerManager.InvokeHandler(new PackageHandlerKey(fieldMatcher.PackageTypeCode, _ConnectState), fieldMatcher);
            }
            catch (ArgumentException)
            {

            }
            catch (HandlerNotFoundException)
            {

            }
        }


        /// <summary>
        /// 在控制台中输出指定handler的签名及实参
        /// </summary>
        [Conditional("DEBUG")]
        private void ShowPackageHandlerInvokeMessage(HandlerInfo<EssentialClient, PackageHandlerAttribute, PackageHandlerKey> handler, object[] actualParameters, string[] handlerRange)
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

    }
}
