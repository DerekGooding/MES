using MES.Common;
using MES.Common.Config.Enums;

namespace MES.PLC;

internal class PLCStation(StationOptions stationOptions, ClientSimulationOptions clientOptions, List<string> serialNumberList)
{

    public event Action Completed;

    private PLCClient _client = new PLCClient(stationOptions.IpAddress, int.Parse(stationOptions.Port), stationOptions.StationName);
    private string _name = clientOptions.StationName;
    private List<string> _serialNumbers = serialNumberList;
    private int _snArrayIndex = int.Parse(clientOptions.SerialNumberArrayIndex);
    private string _currentSerialNumber;
    private int _minCycleTime = int.Parse(clientOptions.MinCycleTime);
    private int _maxCycleTime = int.Parse(clientOptions.MaxCycleTimel);
    private Random _random = new Random();
    private Dictionary<string, string> _results = stationOptions.Results;

    public async Task StartAsync()
    {
        if (_snArrayIndex < _serialNumbers.Count)
        {
            string statusResponse = string.Empty;
            _currentSerialNumber = _serialNumbers[_snArrayIndex];
            if (!_client.IsConnected())
            {
                await _client.ConnectAsync();
            }
            await Task.Delay(_random.Next(100, 500)); //Simulate latency in the initial request

            if (_snArrayIndex != 0)
            {
                statusResponse = await ReadWriteAsync(FormatStatusCheckMessage());
            }

            if (EvaluateStatusResponse(statusResponse) || _snArrayIndex == 0)
            {
                await Task.Delay(_random.Next(_minCycleTime, _maxCycleTime)); //Simulate variable cycle time of the physical station
                string updateMessage = FormatUpdateMessage();
                await ReadWriteAsync(updateMessage);
            }
            else
            {
                await Task.Delay(_random.Next(_minCycleTime, _maxCycleTime));
            }

        }

        Completed?.Invoke(); // Notify the coordinator that this station has completed its cycle
    }

    public async Task<string> ReadWriteAsync(string dataToSend) => await _client.SendReceiveAsync(dataToSend);

    private string FormatStatusCheckMessage() => $"{PLCOperationsEnum.GetStatus}|{_name}|{_currentSerialNumber}";

    private bool EvaluateStatusResponse(string response)
    {
        if (string.IsNullOrEmpty(response))
        {
            return false;
        }

        string[] parts = response.Split('|');

        return parts[0].Trim() == nameof(PLCOperationsEnum.Good);

    }

    private string FormatUpdateMessage()
    {
        string resultString = "";

        foreach (var result in _results)
        {
            string dataType = result.Value.ToLower();

            switch (dataType)
            {
                case "int":
                    resultString += $"|{result.Key}|{GenerateIntResult()}";

                    break;
                case "real":
                    resultString += $"|{result.Key}|{GenerateFloatResult()}";

                    break;
                case "bool":
                    resultString += $"|{result.Key}|{GenerateBoolResult()}";

                    break;
                case "string":
                    resultString += $"|{result.Key}|{GenerateStringResult()}";

                    break;

            }
        }

        return resultString.Length > 0
            ? $"{PLCOperationsEnum.UpdateStatus}|{_name}|{_currentSerialNumber}|{GeneratePassFailResult()}:{resultString}"
            : $"{PLCOperationsEnum.UpdateStatus}|{_name}|{_currentSerialNumber}|{GeneratePassFailResult()}";
    }

    private int GenerateIntResult() => _random.Next(100, 150);

    private float GenerateFloatResult() => (float)(_random.Next(5000, 6000) / 100.0);

    private bool GenerateBoolResult() => _random.Next(0, 2) == 1;

    private string GenerateStringResult()
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        return new string(Enumerable.Repeat(chars, 8)
          .Select(s => s[_random.Next(s.Length)]).ToArray());
    }

    private PLCOperationsEnum GeneratePassFailResult() => _random.Next(1, 100) <= 10 ? PLCOperationsEnum.Bad : PLCOperationsEnum.Good;

}
