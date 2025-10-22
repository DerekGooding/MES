﻿using MES.Common;
using System.Text.Json;

namespace MES.PLC;

internal static class Program
{
    private static readonly JsonSerializerOptions _jsonOptions = new(){ PropertyNameCaseInsensitive = true };

    static async Task Main()
    {
        List<ClientSimulationOptions> clientSimulationOptions;
        List<StationOptions> stationOptions;

        //Read station configuration and client configuration from the configuration files and verify the values are valid
        try
        {
            var clientConfigPath = Path.Combine(AppContext.BaseDirectory, "Config", "ClientSimulationConfig.json");
            var optionsConfigPath = Path.Combine(AppContext.BaseDirectory, "Config", "StationConfig.json");
            clientSimulationOptions = JsonSerializer.Deserialize<List<ClientSimulationOptions>>(File.ReadAllText(clientConfigPath), _jsonOptions) ?? [];
            stationOptions = JsonSerializer.Deserialize<List<StationOptions>>(File.ReadAllText(optionsConfigPath), _jsonOptions) ?? [];
            ValidateConfig.ValidateStationConfig(stationOptions);
            ValidateConfig.ValidateClientSimulationConfig(clientSimulationOptions, stationOptions);
        }
        catch (Exception e)
        {

            Console.WriteLine($"Error loading configuration: {e.Message}");
            Console.ReadKey();
            return;
        }

        // Create a serial number generator for the simulation
        var serialGen = new SerialNumberGenerator("AA", 0, 6, stationOptions.Count);

        /* Create a PLC station for each station defined in the configuration file and add it to a list. The station is
         * what simulates the physical station on the manufacturing line.
         */
        List<PLCStation> stations = [];

        foreach (var stationOption in stationOptions)
        {
            var clientSimulationOption = clientSimulationOptions.Find(c => c.StationName == stationOption.StationName);
            if(clientSimulationOption == null)
            {
                Console.WriteLine($"No client simulation configuration found for station: {stationOption.StationName}");
                continue;
            }
            stations.Add(new PLCStation(stationOption, clientSimulationOption, serialGen.serialNumbers));
        }

        // Create a coordinator to manage the PLC stations
        var coordinator = new Coordinator(stations);


        while (true)
        {
            serialGen.GenerateSerialNumbers(); // Generate a new set of serial numbers for each run
            await coordinator.Coordinate(); // Start the coordination of PLC stations
            coordinator.Reset(); // Reset the coordinator for the next run
        }

    }
}
