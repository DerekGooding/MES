using System.Net;
using System.Net.Sockets;
using MES.Common;
using MES.Common.Config.Enums;
using MES.Data;
using System.Reflection;

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
                string parsedResponse = await ParseRequest(receivedData);

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

    private async Task<string> ParseRequest(string request)
    {
        
        string[] parts = request.Split('|');

        if (parts.Length < 3)
        {
            return $"{_name} received invalid request format: {request}";
        }
        string operation = parts[0].Trim();
        string stationName = parts[1].Trim();
        string serialNumber = parts[2].Trim();

        if (operation == PLCOperationsEnum.GetStatus.ToString())
        {
            bool status = await GetStatus(serialNumber);
            if (status)
            {
                return $"{PLCOperationsEnum.Good}|{_name}|{serialNumber}";
            }
            return $"{PLCOperationsEnum.Bad}|{_name}|{serialNumber}";
        }

        if (operation == PLCOperationsEnum.UpdateStatus.ToString())
        {
            
            string status = parts[3].Trim();
            await UpdateStatus(request);
            return $"{PLCOperationsEnum.UpdateStatus}|{_name}|{status}";
        }

        return $"{_name} received unknown operation: {operation}";
    }

    private async Task<bool>  GetStatus(string serialNumber)
    {
        using var repository = new PartDataRepository();
        PartData? part = await repository.GetPartDataBySerialNumberAsync(serialNumber);
        Console.WriteLine($"{_name} server checked status for Serial Number: {serialNumber}, Status: {part?.Status}");
        return part != null && part.Status == PLCOperationsEnum.Good.ToString();
    }

    private async Task UpdateStatus(string request)
    {
        string[] parts = request.Split('|', ':');
        string operation = parts[0].Trim();
        string stationName = parts[1].Trim();
        string serialNumber = parts[2].Trim();
        string status = parts[3].Trim();
        using var repository = new PartDataRepository();

        PartData part = new PartData
        {
            SerialNumber = serialNumber,
            Status = status,
            LastStationComplete = stationName,
            Timestamp = DateTime.Now
        };

        if (parts.Length == 4)
        {
            await repository.UpdatePartDataAsync(part);
            return;
        }

        var properties = typeof(PartData).GetProperties(BindingFlags.Public | BindingFlags.Instance);

        foreach (var result in _results)
        {
            string dataType = result.Value.ToLower();
            string key = result.Key;
            int index = Array.IndexOf(parts, key);

            if (index != -1 && index + 1 < parts.Length)
            {
                foreach (var property in properties)
                {
                    string propertyName = property.Name;
                    string value = parts[index + 1].Trim();
                    if (propertyName == key)
                    {
                        switch (dataType)
                        {
                            case "int":
                                int intValue;
                                if (int.TryParse(value, out intValue))
                                {
                                    property.SetValue(part, intValue);
                                }
                                break;
                            case "real":
                                float floatValue;
                                if (float.TryParse(value, out floatValue))
                                {
                                    property.SetValue(part, floatValue);
                                }
                                break;
                            case "bool":
                                bool boolValue;
                                if (bool.TryParse(value, out boolValue))
                                {
                                    property.SetValue(part, boolValue);
                                }
                                break;
                            case "string":
                                property.SetValue(part, value);
                                break;
                        }
                    }
                }
            }
        }
        
        await repository.UpdatePartDataAsync(part);
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
