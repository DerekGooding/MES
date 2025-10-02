using Common;
using System.Text.Json;

namespace MES;

internal class Program
{
    static async Task Main(string[] args)
    {
        List<StationOptions> options;

        JsonSerializerOptions jsonOptions = new JsonSerializerOptions {
            PropertyNameCaseInsensitive = true
        };

        try
        {
            options = JsonSerializer.Deserialize<List<StationOptions>>(File.ReadAllText("StationConfig.json"), jsonOptions);
            ValidateStationConfig.ValidateConfiguration(options);
        }
        catch (Exception e)
        {

            Console.WriteLine($"Error loading configuration: {e.Message}");
            return;
        }



        List<PLCServer> servers = new List<PLCServer>();
        List<Task> serverTasks = new List<Task>();

        foreach (var option in options)
        {
            PLCServer server = new PLCServer(option);
            servers.Add(server);
            serverTasks.Add(server.StartAsync());
        }


        //PLCServer stationTen = new PLCServer("127.0.0.1", 5000, "Station 10");
        //PLCServer stationTwenty = new PLCServer("127.0.0.1", 5001, "Station 20");
        //PLCServer stationThirty = new PLCServer("127.0.0.1", 5002, "Station 30");

        //Task stationTenTask = stationTen.StartAsync();
        //Task stationTwentyTask = stationTwenty.StartAsync();
        //Task stationThirtyTask = stationThirty.StartAsync();

        Console.WriteLine("Press any key to exit...");
        Console.ReadKey();

        foreach (var server in servers)
        {
            server.Dispose();
        }

        //stationTen.Dispose();
        //stationTwenty.Dispose();
        //stationThirty.Dispose();

        //await Task.WhenAll(stationTenTask, stationTwentyTask, stationThirtyTask);
        await Task.WhenAll(serverTasks);
    }
}
