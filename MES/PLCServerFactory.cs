
using MES.Common;
using MES.Data.Repositories;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace MES;

internal class PLCServerFactory(ILogger<PLCServer> logger, IServiceProvider serviceProvider) : IPLCServerFactory
{
    private List<StationOptions> _stationOptions;
    private string _connectionString;
    private readonly ILogger<PLCServer> _logger = logger;
    private readonly IServiceProvider _serviceProvider = serviceProvider;

    public List<PLCServer> CreateServers()
    {
        List<PLCServer> servers = new List<PLCServer>();
        LoadStationConfig();
        var dbLogger = _serviceProvider.GetRequiredService<ILogger<PartDataRepository>>();
        foreach (StationOptions option in _stationOptions)
        {
            servers.Add(new PLCServer(option, _connectionString, _logger, _serviceProvider));
        }
        return servers;
    }

    private void LoadStationConfig()
    {
        _stationOptions = new List<StationOptions>();

        JsonSerializerOptions jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        try
        {
            string optionsConfigPath = Path.Combine(AppContext.BaseDirectory, "Config", "StationConfig.json");
            _stationOptions = JsonSerializer.Deserialize<List<StationOptions>>(File.ReadAllText(optionsConfigPath), jsonOptions);
            ValidateConfig.ValidateStationConfig(_stationOptions);
            _connectionString = DbConnectionHelper.GetConnectionString();

        }
        catch (Exception e)
        {

            Console.WriteLine($"Error loading configuration: {e.Message}");

        }
    }
}
