using System.Net;
using System.Net.Sockets;
using Common;

namespace MES;

internal class PLCServer : IDisposable
{
    private IPAddress _ipAddress;
    private int _port;
    private TcpListener _listener;
    private TcpClient _client;
    private IPAddress _ip;
    private String _name;
    private CancellationToken _token;
    private CancellationTokenSource _cts;
    private Dictionary<string, string> _results = new Dictionary<string, string>();
    private string[] _validDataTypes = { "string", "int", "integer", "float", "real", "bool", "boolean" };

    //public PLCServer(string ipAddress, int port, string name)
    //{
    //    _ipAddress = ipAddress;
    //    _port = port;
    //    _ip = IPAddress.Parse(_ipAddress);
    //    _name = name;
    //    _cts = new CancellationTokenSource();
    //    _token = _cts.Token;
    //}

    public PLCServer(StationOptions options)
    {
        //_ipAddress = options.IpAddress;
        _port = int.Parse(options.Port);
        _ipAddress = IPAddress.Parse(options.IpAddress);
        _name = options.StationName;
        _cts = new CancellationTokenSource();
        _token = _cts.Token;
        _results = options.Results;
    }

    //private void ValidateConfiguration(StationOptions options)
    //{
    //    if (String.IsNullOrEmpty(options.StationName) || !char.IsLetter(options.StationName[0]))
    //    {
    //        throw new InvalidConfigurationException("Station name cannot be empty or whitespace. Check configuration in ServerStationConfig.json file");
    //    }


    //    if(!IPAddress.TryParse(options.IpAddress, out _))
    //    {
    //        throw new InvalidConfigurationException($"{options.StationName} IP address '{options.IpAddress}' is not valid. Check configuration in ServerStationConfig.json file");
    //    }

    //    if(!int.TryParse(options.Port, out _))
    //    {
    //        throw new InvalidConfigurationException($"{options.StationName} Port '{options.Port}' is not a valid integer. Check configuration in ServerStationConfig.json file");
    //    }

    //    foreach (var kvp in _results)
    //    {
    //        string dataType = kvp.Value.ToLower();

    //        if (String.IsNullOrEmpty(kvp.Key) || !char.IsLetter(kvp.Key[0]))
    //        {
    //            throw new InvalidConfigurationException($"The result name {kvp.Key} is invalid. Cannot be empty or whitespace, and must start with a letter. Check configuration in ServerStationConfig.json file");
    //        }

    //        if (!_validDataTypes.Contains(dataType))
    //        {
    //            throw new InvalidConfigurationException($"The data type '{kvp.Value}' for result '{kvp.Key}' is not valid. Valid data types are: {string.Join(", ", _validDataTypes)}. Check configuration in ServerStationConfig.json file");
    //        }

    //    }
    //}

    public async Task StartAsync()
    {
        try
        {
            _listener = new TcpListener(_ipAddress, _port);
            _listener.Start(1);
            Console.WriteLine($"{_name} server started and listening on {_ipAddress.ToString()}:{_port}");

            while (!_token.IsCancellationRequested)
            {
                //Dispose of previous client if it's still connected
                _client?.Close();

                _client = await _listener.AcceptTcpClientAsync(_token);
                Console.WriteLine($"{_name} server accepted a connection.");
                await HandleClientAsync(_client);

            }

        }
        catch (OperationCanceledException)
        {
            Console.WriteLine($"{_name} server: operation canceled.");
        }
        catch (Exception e)
        {

            if (!_cts.IsCancellationRequested)
            {
                Console.WriteLine($"{_name} server encountered an issue starting the server. {e.Message}");
            }
        }
        finally
        {
            _client?.Close();
        }
    }

    public async Task HandleClientAsync(TcpClient client)
    {
        byte[] buffer = new byte[1024];


        try
        {
            using NetworkStream stream = client.GetStream();
            while (!_token.IsCancellationRequested)
            {
                int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length, _token);
                if (bytesRead == 0)
                {
                    Console.WriteLine($"{_name} server: client disconnected.");
                    break;
                }

                string receivedData = System.Text.Encoding.ASCII.GetString(buffer, 0, bytesRead);
                Console.WriteLine($"{_name} server received: {receivedData}");

                byte[] response = System.Text.Encoding.ASCII.GetBytes($"{_name} server data recieved ACK");
                try
                {
                    await stream.WriteAsync(response, 0, response.Length, _token);
                }
                catch (Exception e)
                {

                    Console.WriteLine($"{_name} server encountered an error replying to client. {e.Message}");
                }
            }
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine($"{_name} server: Read operation canceled.");
        }
        catch (Exception e)
        {

            Console.WriteLine($"{_name} server encountered an issue reading from client. {e.Message}");
        }


    }

    public void Dispose()
    {
        _cts.Cancel();
        _client?.Close();
        _listener?.Stop();
        _cts.Dispose();
        Console.WriteLine($"{_name} server has been stopped.");
    }
}
