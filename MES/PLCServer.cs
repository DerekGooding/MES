using System.Net;
using System.Net.Sockets;
using Common;
using Common.Config.Enums;

namespace MES;

internal class PLCServer : IDisposable
{
    private IPAddress _ipAddress;
    private int _port;
    private TcpListener _listener;
    private TcpClient _client;
    private String _name;
    private CancellationToken _token;
    private CancellationTokenSource _cts;
    private Dictionary<string, string> _results = new Dictionary<string, string>();


    public PLCServer(StationOptions options)
    {
        _port = int.Parse(options.Port);
        _ipAddress = IPAddress.Parse(options.IpAddress);
        _name = options.StationName;
        _cts = new CancellationTokenSource();
        _token = _cts.Token;
        _results = options.Results;
    }


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
                string parsedResponse = ParseRequest(receivedData);

                byte[] response = System.Text.Encoding.ASCII.GetBytes(parsedResponse);
                try
                {
                    Console.WriteLine($"{_name} server sent: {parsedResponse}");
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

    private string ParseRequest(string request)
    {
        
        string[] parts = request.Split('|');
        if (parts.Length < 3)
        {
            return $"{_name} received invalid request format: {request}";
        }
        string operation = parts[0].Trim();

        if(operation == PLCOperationsEnum.GetStatus.ToString())
        {
            return $"{PLCOperationsEnum.StatusGood} | {_name} | {parts[2]}";
        }

        if (operation == PLCOperationsEnum.UpdateStatus.ToString())
        {
            string serialNumber = parts[2].Trim();
            string status = parts[3].Trim();
            return $"{PLCOperationsEnum.UpdateStatus} | {_name} | {status}";
        }

        return $"{_name} received unknown operation: {operation}";
    }

    private bool GetStatus(string serialNumber)
    {
        return true;
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
