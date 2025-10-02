

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

        public PLCStation(string ipAddress, int port, string name, List<string> serialNumberList, int serialNumberArrayIndex, int minCycleTime, int maxCycleTime)
        {
            _client = new PLCClient(ipAddress, port, name);
            _name = name;
            _serialNumbers = serialNumberList;
            _snArrayIndex = serialNumberArrayIndex;
            _minCycleTime = minCycleTime;
            _maxCycleTime = maxCycleTime;
        }

        public async Task StartAsync()
        {
            if(_snArrayIndex < _serialNumbers.Count)
            {
                _currentSerialNumber = _serialNumbers[_snArrayIndex];
                if (!_client.IsConnected())
                {
                    await _client.ConnectAsync();
                }
                await Task.Delay(_random.Next(100, 500)); //Simulate latency in the initial request
                await ReadWriteAsync($"{_name} sent {_currentSerialNumber} to server");
                await Task.Delay(_random.Next(_minCycleTime, _maxCycleTime)); //Simulate variable cycle time of the physical station
                await ReadWriteAsync($"{_name} process complete.");
            }

            Completed?.Invoke();
        }

        public async Task ReadWriteAsync(string dataToSend)
        {
            
            await _client.SendReceiveAsync(dataToSend);
            //Completed?.Invoke();
        }
    }
}
