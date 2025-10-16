using MES.Common;
using MES.Data;
using System.Text.Json;

namespace MES;

internal class Program
{
    static async Task Main(string[] args)
    {

        string connectionString;

        try {
            connectionString = DbConnectionHelper.GetConnectionString();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            return;
        }

        DataContext context = new DataContext(connectionString);
        context.Database.EnsureDeleted(); //Delete and recreate database on each run for testing purposes
        context.Database.EnsureCreated();
        context.Dispose();

        List<StationOptions> options;

        JsonSerializerOptions jsonOptions = new JsonSerializerOptions {
            PropertyNameCaseInsensitive = true
        };

        //Read station configuration from the configuration file and verify the values are valid
        try
        {
            string optionsConfigPath = Path.Combine(AppContext.BaseDirectory, "Config", "StationConfig.json");
            options = JsonSerializer.Deserialize<List<StationOptions>>(File.ReadAllText(optionsConfigPath), jsonOptions);
            ValidateConfig.ValidateStationConfig(options);
        }
        catch (Exception e)
        {

            Console.WriteLine($"Error loading configuration: {e.Message}");
            return;
        }

        List<PLCServer> servers = new List<PLCServer>();
        List<Task> serverTasks = new List<Task>();

        /*Create a new server for each station defined in the configuration file and add it to a list. 
         Also, start each server and add it to a list of Tasks*/
        
        foreach (var option in options)
        {
            PLCServer server = new PLCServer(option, connectionString);
            servers.Add(server);
            serverTasks.Add(server.StartAsync());
        }

        Console.WriteLine("Press any key to exit...");
        Console.ReadKey();

        await Task.WhenAll(serverTasks);
        foreach (var server in servers)
        {
            server.Dispose();
        }
    }
}
