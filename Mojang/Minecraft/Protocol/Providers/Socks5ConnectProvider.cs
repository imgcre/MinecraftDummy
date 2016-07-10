using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mojang.Minecraft.Protocol.Providers
{
    //http://www.cnblogs.com/zhujiechang/archive/2008/10/21/1316308.html
    public class Socks5TcpConnectProvider : DefaultConnectProvider
    {
        private const byte Version = 5;
        public Socks5TcpConnectProvider(string proxyServerAddress, ushort proxyServerPort, string targetServerAddress, ushort targetServerPort) 
            : base(proxyServerAddress, proxyServerPort)
        {
            Negotiate(ProxyMethod.Anonymous).Wait();
            var connectState = ConnectTargetByDomainName(Command.Connect, targetServerAddress, targetServerPort).Result;
            if (connectState != RequestResponse.Succeed)
                throw new Exception("连接失败");
             
        }


        [Flags]
        private enum ProxyMethod
        {
            Anonymous,
            Confirmation,
        }


        private async Task<ProxyMethod> Negotiate(ProxyMethod proxyMethod)
        {
            var list = new List<byte>();

            if (proxyMethod.HasFlag(ProxyMethod.Anonymous))
                list.Add(0);
            if (proxyMethod.HasFlag(ProxyMethod.Confirmation))
                list.Add(2);
            list.Insert(0, (byte)list.Count);
            list.Insert(0, Version);

            await Send(list.ToArray());
            var result = (await Receive(2));
            if (result.First() != Version)
                throw new Exception("无效的代理服务器版本号");

            if (result.Last() == 0)
                return ProxyMethod.Anonymous;
            if (result.Last() == 2)
                return ProxyMethod.Confirmation;
            throw new Exception("选中了未知的方法");
        }


        private enum Command : byte
        {
            Connect = 1,
            Bind,
            UdpAssociate,
        }


        private enum AddressType : byte
        {
            Ipv4 = 1,
            DomainName = 3,
            Ipv6,
        }


        private enum RequestResponse : byte
        {
            Succeed = 0,
            普通的SOCKS服务器请求失败,
            现有的规则不允许的连接,
            网络不可达,
            主机不可达,
            连接被拒,
            TTL超时,
            不支持的命令,
            不支持的地址类型,
            未定义,
        }


        private async Task<RequestResponse> ConnectTargetByDomainName(Command command, string targetAddress, ushort port)
        {
            var list = new List<byte>();
            list.Add(Version);
            list.Add((byte)command);
            list.Add(0);//保留字节
            list.Add((byte)AddressType.DomainName);

            var domainName = Encoding.ASCII.GetBytes(targetAddress);
            list.Add((byte)domainName.Length);
            list.AddRange(domainName);

            list.AddRange(BitConverter.GetBytes(port).Reverse());
            await Send(list.ToArray());
            //var result = await Receive(list.Count);
            var result = await Receive(10);
            if (result.First() != Version)
                throw new Exception("无效的代理服务器版本号");
            return (RequestResponse)result[1];
        }
    }
}
