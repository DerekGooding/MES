using Common;
using Common.Config.Enums;

namespace PLC
{
    internal class PLCStation
    {

        public event Action Completed;

        private PLCClient _client;
        private string _name;
        private List<string> _serialNumbers;
        private int _snArrayIndex = 0;
        private string _currentSerialNumber;
        private int _minCycleTime;
        private int _maxCycleTime;
        private Random _random = new Random();
        private Dictionary<string, string> _results;



        public PLCStation(StationOptions stationOptions, ClientSimulationOptions clientOptions, List<string> serialNumberList)
        {
            _client = new PLCClient(stationOptions.IpAddress, int.Parse(stationOptions.Port), stationOptions.StationName);
            _name = clientOptions.StationName;
            _serialNumbers = serialNumberList;
            _snArrayIndex = int.Parse(clientOptions.SerialNumberArrayIndex);
            _minCycleTime = int.Parse(clientOptions.MinCycleTime);
            _maxCycleTime = int.Parse(clientOptions.MaxCycleTimel);
            _results = stationOptions.Results;
        }

        public async Task StartAsync()
        {
            if (_snArrayIndex < _serialNumbers.Count)
            {
                _currentSerialNumber = _serialNumbers[_snArrayIndex];
                if (!_client.IsConnected())
                {
                    await _client.ConnectAsync();
                }
                await Task.Delay(_random.Next(100, 500)); //Simulate latency in the initial request
                string statusResponse = await ReadWriteAsync(FormatStatusCheckMessage());
                if (EvaluateStatusResponse(statusResponse))
                {
                    await Task.Delay(_random.Next(_minCycleTime, _maxCycleTime)); //Simulate variable cycle time of the physical station
                    string updateMessage = FormatUpdateMessage();
                    await ReadWriteAsync(updateMessage);
                }

            }

            Completed?.Invoke();
        }

        public async Task<string> ReadWriteAsync(string dataToSend)
        {

            return await _client.SendReceiveAsync(dataToSend);

        }

        private string FormatStatusCheckMessage()
        {
            return $"{PLCOperationsEnum.GetStatus} | {_name} | {_currentSerialNumber}";
        }

        private bool EvaluateStatusResponse(string response)
        {
            if (string.IsNullOrEmpty(response))
            {
                return false;
            }

            string[] parts = response.Split('|');

            return parts[0].Trim() == PLCOperationsEnum.StatusGood.ToString();

        }

        private string FormatUpdateMessage()
        {
            foreach (var result in _results)
            {

                switch (result.Value)
                {
                    case "int":

                        break;
                    case "real":

                        break;
                    case "bool":

                        break;
                    case "string":

                        break;

                }
            }

            if (_random.Next(1, 100) <= 10)
            {
                return $"{PLCOperationsEnum.UpdateStatus} | {_name} | {_currentSerialNumber} | {PLCOperationsEnum.StatusBad}";
            }

            return $"{PLCOperationsEnum.UpdateStatus} | {_name} | {_currentSerialNumber} | {PLCOperationsEnum.StatusGood}";
        }

        private int GenerateIntResult()
        {
            return _random.Next(100, 150);
        }

        private float GenerateFloatResult()
        {
            return (float)(_random.Next(5000, 6000) / 100.0);
        }

        private bool GenerateBoolResult()
        {
            return _random.Next(0, 2) == 1;
        }

        private string GenerateStringResult()
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, 8)
              .Select(s => s[_random.Next(s.Length)]).ToArray());
        }

    }
}
