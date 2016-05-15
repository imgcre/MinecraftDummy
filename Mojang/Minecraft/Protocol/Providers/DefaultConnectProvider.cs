using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Mojang.Minecraft.Protocol.Providers
{
    public class DefaultConnectProvider : IConnectProvider
    {
        private AutoResetEvent _AutoResetEvent = new AutoResetEvent(false);

        private TcpClient _TcpClient;
        private byte[] _AsyncBuffer;

        private List<byte> _DataBuffer = new List<byte>();
        private object _ListLock = new object();


        public DefaultConnectProvider(string serverAddress, ushort serverPort)
        {
            _TcpClient = new TcpClient(serverAddress, serverPort);
            _AsyncBuffer = new byte[_TcpClient.ReceiveBufferSize];

            ServerAddress = serverAddress;
            ServerPort = serverPort;

            Dispatch();
        }


        public string ServerAddress { get; private set; }


        public ushort ServerPort { get; private set; }


        private async void Dispatch()
        {
            var receivedLength = await _TcpClient.GetStream().ReadAsync(_AsyncBuffer, 0, _AsyncBuffer.Length);
            lock(_ListLock)
            {
                _DataBuffer.AddRange(_AsyncBuffer.Take(receivedLength));
            }
            _AutoResetEvent.Set();
            Dispatch();
        }


        private object _ReceiveLock = new object();

        public async Task<byte[]> Receive(int length)
        {
            return await Task.Run(() => 
            {
                lock (_ReceiveLock)
                {
                    while (length > _DataBuffer.Count)
                        _AutoResetEvent.WaitOne();

                    lock (_ListLock)
                    {
                        var result = _DataBuffer.Take(length).ToArray();
                        _DataBuffer.RemoveRange(0, length);
                        return result;
                    }
                }
            });
            
        }


        public async Task Send(byte[] data)
        {
            await _TcpClient.GetStream().WriteAsync(data, 0, data.Length);
        }
    }
}
