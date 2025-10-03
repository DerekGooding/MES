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
            options = JsonSerializer.Deserialize<List<StationOptions>>(File.ReadAllText("Config/StationConfig.json"), jsonOptions);
            ValidateConfig.ValidateStationConfig(options);
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

        Console.WriteLine("Press any key to exit...");
        Console.ReadKey();

        foreach (var server in servers)
        {
            server.Dispose();
        }

        await Task.WhenAll(serverTasks);
    }
}
