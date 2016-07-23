using Mojang.Minecraft.Protocol;
using Mojang.Minecraft.Protocol.Providers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

namespace Mojang.Minecraft
{

    public class Channel : IPackageField
    {
        private string _ChannelName;
        private byte[] _ChannelData;
        private bool _ChannelSenderInvoked;

        public Channel()
        {

        }


        internal Channel(string channelName, byte[] channelData)
        {
            _ChannelName = channelName;
            _ChannelData = channelData;
            _ChannelSenderInvoked = true;
        }


        void IPackageField.AppendIntoField(FieldMaker fieldMaker)
        {
            if (!_ChannelSenderInvoked)
                throw new NotSupportedException("只能通过调用channel sender来隐式构造channel");

            fieldMaker.AppendPackageField<PString>(_ChannelName);
            fieldMaker.AppendBytes(_ChannelData);
            
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

    internal class ChannelSenderManager : SenderManager<EssentialClient, ChannelSenderAttribute, string>
    {
        public ChannelSenderManager(EssentialClient ownerInstance) : base(ownerInstance)
        {

        }

        protected override void AppendIntoField(FieldMaker fieldMaker, object actualParameter, Type actualParameterType, IEnumerable<Attribute> formalParameterAttributes)
        {
            //TODO: 前条件失效
            var stringArray = actualParameter as string[];

            if (!(stringArray == null || formalParameterAttributes.Contains(new VariableAttribute())))
            {
                if (stringArray.Length == 0)
                    throw new ArgumentException("传入参数无元素");

                Array.ForEach(stringArray.Take(stringArray.Length - 1).ToArray(), str => fieldMaker.AppendBytes(Encoding.UTF8.GetBytes(str).Concat(new byte[] { 0 })));
                fieldMaker.AppendBytes(Encoding.UTF8.GetBytes(stringArray.Last()));
                return;
            }

            base.AppendIntoField(fieldMaker, actualParameter, actualParameterType, formalParameterAttributes);
        }

        protected override async Task<FieldMaker> SendInternal(MethodBase sender, ChannelSenderAttribute attribute, params object[] fields)
        {
            var fieldMatcher = await base.SendInternal(sender, attribute, fields);
            var channel = new Channel(attribute.SenderKey, fieldMatcher.GetBytes());
            await OwnerInstance.PluginMessage(channel);
            return null;
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


    [AttributeUsage(AttributeTargets.Method)]
    internal class ChannelSenderAttribute : Attribute, ISenderAttribute<string>
    {
        public string SenderKey { get; set; }

        public ChannelSenderAttribute(string channelName)
        {
            SenderKey = channelName;
        }
    }
}
