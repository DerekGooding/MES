

namespace PLC;

internal class Coordinator
{
    private int _remainingStations;
    private IEnumerable<PLCStation> _stations;

    public Coordinator(IEnumerable<PLCStation> stations)
    {
        _stations = stations;
    }

    public async Task Coordinate()
    {
        _remainingStations = _stations.Count();
        var tasks = new List<Task>();
        foreach (var station in _stations)
        {
            station.Completed += OnStationCompleted;
            tasks.Add(station.StartAsync());

        }

        await Task.WhenAll(tasks);
    }

    private void OnStationCompleted()
    {
        if (Interlocked.Decrement(ref _remainingStations) == 0)
        {
            Console.WriteLine("All PLC stations have completed their operations.");
        }
    }

    public void Reset()
    {
        _remainingStations = _stations.Count();
    }
}
