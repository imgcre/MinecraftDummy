using Microsoft.CSharp.RuntimeBinder;
using Mojang.Minecraft;
using Mojang.Minecraft.Protocol;
using Mojang.Minecraft.Protocol.Providers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace TestConsole
{
    partial class TestClient : PlayerClient
    {
        private const int FIRST_ITEM = 1;
        private const char SPACE = ' ';
        private const char SPLITTER = SPACE;

        public TestClient(string serverAddress, ushort serverPort, string username) : base(serverAddress, serverPort, username)
        {

        }

        public TestClient(Socks5TcpConnectProvider socksConnect, string username) : base(socksConnect, username)
        {

        }


        bool _FirstChat = true;

        protected override void OnChatted(Chat jsonData, ChatPosition chatPosition)
        {
            //jsonData: {"translate":"chat.type.text","with":[{"clickEvent":{"action":"suggest_command","value":"/msg SiNian "},"text":"SiNian"},"hi"]}
            //jsonData: {"translate":"chat.type.text","with":[{"clickEvent":{"action":"suggest_command","value":"/msg SiNian "},"text":"SiNian"},"hello"]}

            /*
            try
            {
                var sender = jsonData.Root.with[0].text;
                var message = jsonData.Root.with[1] as string;
                Console.WriteLine(message);

                if (sender == "SiNian")
                {
                    var instruction = message.Split(SPLITTER);
                    try
                    {
                        var result = InvokeCommand(instruction.First(), instruction.Skip(FIRST_ITEM).ToArray());
                        if (result != null)
                        {
                            Chat(result.ToString()).Wait();
                        }

                    }
                    catch (ArgumentException e)
                    {
                        Chat(e.Message).Wait();
                    }
                }
            }
            catch (RuntimeBinderException)
            {

            }
            */

            
            
            if (_FirstChat)
            {
                _FirstChat = false;
                ChangeState(StateAction.RequestStats).Wait();
            }

            try
            {
                var sb = new StringBuilder();
                foreach (var messageSegment in Array.ConvertAll(jsonData.Root.extra, new Converter<dynamic, string>(o => o is string ? o : o.text)))
                    sb.Append(messageSegment);
                var message = sb.ToString();

                Console.WriteLine(message);

                if (message.Contains("注册"))
                    Chat("/reg sinian sinian").Wait();
                else if(message.Contains("登录") || message.Contains("登陆"))
                    Chat("/login sinian sinian").Wait();

                var playerMessageComponentRegex = new Regex(@"^(?:\[(?#place)(.*?)\])?<(?#sender)(.*?)> (?:(?#reciever)(\w*?)\.)?(?#message)(.*)$");
                if (playerMessageComponentRegex.IsMatch(message))
                {
                    var match = playerMessageComponentRegex.Match(message);
                    var sender = match.Groups[2].ToString();
                    var reciever = match.Groups[3].ToString();
                    var playerMessage = match.Groups[4].ToString();

                    if (!(string.IsNullOrEmpty(reciever) || reciever == Username))
                        return;

                    if (sender == "imgcre")
                    {
                        var instruction = playerMessage.Split(SPLITTER);
                        try
                        {
                            var result = InvokeCommand(instruction.First(), instruction.Skip(FIRST_ITEM).ToArray());
                            if (result != null)
                            {
                                Chat(result.ToString()).Wait();
                            }
                                
                        }
                        catch (ArgumentException e)
                        {
                            Chat(e.Message).Wait();
                        }
                    }

                }
            }
            catch(RuntimeBinderException)
            {

            }
            
        }

    }
}
