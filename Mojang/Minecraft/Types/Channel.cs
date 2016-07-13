using Mojang.Minecraft.Protocol;
using Mojang.Minecraft.Protocol.Providers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mojang.Minecraft
{

    public class Channel : IPackageField
    {
        void IPackageField.AppendIntoField(FieldMaker fieldMaker)
        {
            throw new NotImplementedException();
        }

        void IPackageField.FromField(FieldMatcher fieldMatcher)
        {
            var channelName = fieldMatcher.Match<PString>();
            var channelHandlerManager = new ChannelHandlerManager(fieldMatcher.Owner);

            try
            {
                channelHandlerManager.InvokeHandler(channelName, fieldMatcher);
            }
            catch(HandlerNotFoundException)
            {
            }
        }


        private class ChannelHandlerManager : HandlerManager<EssentialClient, ChannelHandlerAttribute, string>
        {
            public ChannelHandlerManager(EssentialClient ownerInstance) : base(ownerInstance)
            {

            }


            protected override object GetActualParameter(FieldMatcher fieldMatcher, Type formalParameterType, IEnumerable<Attribute> formalParameterAttributes, List<object> context)
            {
                if (!formalParameterAttributes.Contains(new VariableAttribute()))
                {
                    if (formalParameterType == typeof(string))
                        return ReadStringFromStream(fieldMatcher.Stream);

                    if (formalParameterType == typeof(string[]))
                    {
                        var stringList = new List<string>();
                        for (var s = GetActualParameter(fieldMatcher, typeof(string), formalParameterAttributes, context); s != null; s = GetActualParameter(fieldMatcher, typeof(string), formalParameterAttributes, context))
                            stringList.Add(s as string);
                        return stringList.ToArray();
                    }
                }

                return base.GetActualParameter(fieldMatcher, formalParameterType, formalParameterAttributes, context);
            }


            static string ReadStringFromStream(Stream stream)
            {
                var first = stream.ReadByte();

                if (first == -1)
                    return null;
                else if (first == 0)
                    return string.Empty;

                var list = new List<byte>();
                list.Add((byte)first);
                var current = default(int);
                do
                {
                    current = stream.ReadByte();
                    if (current != -1)
                    {
                        list.Add((byte)current);
                    }
                }
                while (current != 0 && current != -1);
                return Encoding.UTF8.GetString(list.ToArray());
            }
        }
    }

    [AttributeUsage(AttributeTargets.Method)]
    internal class ChannelHandlerAttribute : Attribute, IHandlerAttribute<string>
    {
        public string HandlerKey { get; set; }

        public ChannelHandlerAttribute(string channelName)
        {
            HandlerKey = channelName;
        }
    }
}
