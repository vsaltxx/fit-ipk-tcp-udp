using System.Net;
using System.Net.Sockets;

namespace ipk24_chat_client
{
    public class UdpClientWrapper : IDisposable
    {
        private readonly UdpClient _udpClient = new();
        private IPEndPoint? _serverEndPoint;

        private bool _disposed = false;
        public UdpClientWrapper()
        {
            Console.CancelKeyPress += (_, _) =>
            {
                if (_disposed) return;
                Dispose();
                Environment.Exit(0);
            };
        }
        
        
        public void Bind()
        {
            _udpClient.Client.Bind(new IPEndPoint(IPAddress.Any, 0));
        }
        
        public async Task SendAsync(byte[] messageBuffer)
        {
            try
            {
                if (_serverEndPoint != null) _udpClient.Client.SendTo(messageBuffer, _serverEndPoint);
                Console.WriteLine("Message sent successfully via UDP.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending message via UDP: {ex.Message}");
            }
        }

        private bool _confirmSent = false;

        public async Task ReceiveAsync()
        {
            try
            {
                while (!_confirmSent)
                {
                    var receiveResult = await _udpClient.ReceiveAsync();
                    var receivedBytes = receiveResult.Buffer;
                    _serverEndPoint = receiveResult.RemoteEndPoint;
                    //Console.WriteLine($"Received first byte of the message via UDP from {_serverEndPoint}: {receivedBytes[0]}");
                    
                    var messageType = (MessageType)receivedBytes[0];
                    UInt16 refId = BitConverter.ToUInt16(receivedBytes, 1);

                    if (messageType == MessageType.CONFIRM)
                    {
                        // todo something
                    }
                    else
                    {
                        var confirmMessage = UdpMessageBuilder.BuildConfirm(refId);
                        _udpClient.Client.SendTo(confirmMessage, _serverEndPoint);
                        Console.WriteLine("Confirmation message sent via UDP.");
                    }
                    
                    switch (messageType)
                    {
                        case MessageType.REPLY:
                            Console.WriteLine("Received REPLY message via UDP");
                            UdpMessagePrinter.PrintMessage(receivedBytes);
                            break;
                        case MessageType.MSG:
                            UdpMessagePrinter.PrintMessage(receivedBytes);
                            break;
                        case MessageType.ERR:
                            UdpMessagePrinter.PrintMessage(receivedBytes);
                            break;
                        case MessageType.BYE:
                            UdpMessagePrinter.PrintMessage(receivedBytes);
                            break;
                        default:
                            Console.WriteLine("Got unknown message type via UDP");
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error receiving message via UDP: {ex.Message}");
            }
        }

        public async void ResolveServerAddress(string serverIpAddress, int serverPort, ServerSettings serverSettings)
        {
            try
            {
                IPAddress[] addresses = await Dns.GetHostAddressesAsync(serverIpAddress);
                if (addresses.Length == 0)
                {
                    throw new ArgumentException("Failed to resolve the host name.");
                }

                // Use the first obtained IP address
                IPAddress ipAddress = addresses[0];
                _serverEndPoint = new IPEndPoint(ipAddress, serverPort);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error resolving server address: {ex.Message}");
                throw;
            }
        }
        
        
        public void Dispose()
        {
            if (!_disposed)
            {
                _udpClient.Close();
                _disposed = true;
            }
        }
    }
}
