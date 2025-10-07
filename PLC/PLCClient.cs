

using System.Net.Sockets;

namespace PLC
{
    internal class PLCClient : IDisposable
    {
        private string _ipAddress;
        private int _port;
        private string _name;
        private TcpClient _client;
        private CancellationTokenSource _cts;
        private CancellationToken _token;

        public PLCClient(string ipAddress, int port, string name)
        {
            _ipAddress = ipAddress;
            _port = port;
            _name = name;
            _cts = new CancellationTokenSource();
            _token = _cts.Token;
        }

        public async Task ConnectAsync()
        {
            try
            {
                _client = new TcpClient();
                await _client.ConnectAsync(_ipAddress, _port, _token);
                Console.WriteLine($"{_name} client connected to server at ip address: {_ipAddress} port: {_port}");
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine($"{_name} client: Connect operation canceled while trying to connect to server ip address: {_ipAddress} port: {_port}.");
            }
            catch (Exception e)
            {

                Console.WriteLine($"{_name} client encountered an error connecting to server ip address: {_ipAddress} port: {_port}. {e.Message}");
            }
        }

        public async Task<string> SendReceiveAsync(string data)
        {
            try
            {
                if (_client == null || !_client.Connected)
                {
                    Console.WriteLine($"{_name} client is not connected to the server.");
                    return null;
                }
                NetworkStream stream = _client.GetStream();
                stream.ReadTimeout = 5000;
                // Send data
                byte[] sendData = System.Text.Encoding.ASCII.GetBytes(data);
                await stream.WriteAsync(sendData, 0, sendData.Length, _token);
                Console.WriteLine($"{_name} client sent: {data}");
                // Receive response
                byte[] receiveData = new byte[256];
                int bytesRead = await stream.ReadAsync(receiveData, 0, receiveData.Length, _token);
                string responseData = System.Text.Encoding.ASCII.GetString(receiveData, 0, bytesRead);
                Console.WriteLine($"{_name} client received: {responseData}");
                return responseData;

            }
            catch (OperationCanceledException)
            {
                Console.WriteLine($"{_name} client: Send/Receive operation canceled.");
            }
            catch (Exception e)
            {
                Console.WriteLine($"{_name} client encountered an error during Send/Receive. {e.Message}");
            }

            return null;
        }

        public bool IsConnected()
        {
            return _client != null && _client.Connected;
        }

        public void Dispose()
        {
            _client?.Close();
            _cts.Cancel();
            _cts.Dispose();
        }
    }
}
