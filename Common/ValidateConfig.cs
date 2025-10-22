using MES.Common.Exceptions;
using System.Net;

namespace MES.Common;

public static class ValidateConfig
{
    private static readonly string[] _validDataTypes = ["string", "integer", "real", "bool"];
    public static void ValidateStationConfig(List<StationOptions> options)
    {


        foreach (var option in options)
        {
            if (String.IsNullOrEmpty(option.StationName) || !char.IsLetter(option.StationName[0]))
            {
                throw new InvalidConfigurationException("Station name cannot be empty or whitespace. Check configuration in ServerStationConfig.json file");
            }

            if (!IPAddress.TryParse(option.IpAddress, out _))
            {
                throw new InvalidConfigurationException($"{option.StationName} IP address '{option.IpAddress}' is not valid. Check configuration in ServerStationConfig.json file");
            }

            if (!int.TryParse(option.Port, out _))
            {
                throw new InvalidConfigurationException($"{option.StationName} Port '{option.Port}' is not a valid integer. Check configuration in ServerStationConfig.json file");
            }

            if (option.Results != null)
            {
                foreach (var kvp in option.Results)
                {
                    string dataType = kvp.Value.ToLower();

                    if (String.IsNullOrEmpty(kvp.Key) || !char.IsLetter(kvp.Key[0]))
                    {
                        throw new InvalidConfigurationException($"The result name {kvp.Key} is invalid. Cannot be empty or whitespace, and must start with a letter. Check configuration in ServerStationConfig.json file");
                    }

                    if (!_validDataTypes.Contains(dataType))
                    {
                        throw new InvalidConfigurationException($"The data type '{kvp.Value}' for result '{kvp.Key}' is not valid. Valid data types are: {string.Join(", ", _validDataTypes)}. Check configuration in ServerStationConfig.json file");
                    }

                }
            }

        }

        var duplicateNames = options.GroupBy(o => o.StationName)
                                    .Where(g => g.Count() > 1)
                                    .Select(g => g.Key)
                                    .ToList();
        if (duplicateNames.Any())
        {
            throw new InvalidConfigurationException($"Duplicate station names found: {string.Join(", ", duplicateNames)}. Station names must be unique. Check configuration in ServerStationConfig.json file");
        }

        var duplicateIP_PortCombo = options.GroupBy(o => (o.Port, o.IpAddress))
                                    .Where(g => g.Count() > 1)
                                    .Select(g => g.Key)
                                    .ToList();
        if (duplicateIP_PortCombo.Any())
        {
            throw new InvalidConfigurationException($"Duplicate IP address and Port combinations found: {string.Join(", ", duplicateIP_PortCombo.Select(c => $"{c.IpAddress}:{c.Port}"))}. Each station must have a unique IP address and Port combination. Check configuration in ServerStationConfig.json file");
        }

    }

    public static void ValidateClientSimulationConfig(List<ClientSimulationOptions> clientOptions, List<StationOptions> stationOptions)
    {

        foreach (var option in clientOptions)
        {
            if (String.IsNullOrEmpty(option.StationName) || !char.IsLetter(option.StationName[0]))
            {
                throw new InvalidConfigurationException("Station name cannot be empty or whitespace. Check configuration in ClientSimulationConfig.json file");
            }
            if (!int.TryParse(option.MinCycleTime, out int minCycleTime) || minCycleTime < 0)
            {
                throw new InvalidConfigurationException($"{option.StationName} MinCycleTime '{option.MinCycleTime}' is not a valid non-negative integer. Check configuration in ClientSimulationConfig.json file");
            }
            if (!int.TryParse(option.MaxCycleTimel, out int maxCycleTime) || maxCycleTime < 0)
            {
                throw new InvalidConfigurationException($"{option.StationName} MaxCycleTime '{option.MaxCycleTimel}' is not a valid non-negative integer. Check configuration in ClientSimulationConfig.json file");
            }
            if (minCycleTime > maxCycleTime)
            {
                throw new InvalidConfigurationException($"{option.StationName} MinCycleTime '{option.MinCycleTime}' cannot be greater than MaxCycleTime '{option.MaxCycleTimel}'. Check configuration in ClientSimulationConfig.json file");
            }
        }

        var duplicateNames = clientOptions.GroupBy(o => o.StationName)
                                    .Where(g => g.Count() > 1)
                                    .Select(g => g.Key)
                                    .ToList();
        if (duplicateNames.Any())
        {
            throw new InvalidConfigurationException($"Duplicate station names found: {string.Join(", ", duplicateNames)}. Station names must be unique. Check configuration in ClientSimulationConfig.json file");
        }

        foreach (var option in clientOptions)
        {
            if (!stationOptions.Any(s => s.StationName == option.StationName))
            {
                throw new InvalidConfigurationException($"No matching station found for client simulation station name '{option.StationName}'. Check configuration in ClientSimulationConfig.json file");
            }
        }

        if (clientOptions.Count > stationOptions.Count)
        {
            throw new InvalidConfigurationException($"More client simulation stations ({clientOptions.Count}) than server stations ({stationOptions.Count}). Each client simulation station must correspond to a server station. Check configuration in ClientSimulationConfig.json file");
        }

        if (clientOptions.Count < stationOptions.Count)
        {
            throw new InvalidConfigurationException($"Fewer client simulation stations ({clientOptions.Count}) than server stations ({stationOptions.Count}). Each server station must have a corresponding client simulation station. Check configuration in ClientSimulationConfig.json file");
        }

        if (clientOptions[0].SerialNumberArrayIndex != "0")
        {
            throw new InvalidConfigurationException($"The first client simulation station must have SerialNumberArrayIndex of '0'. Check configuration in ClientSimulationConfig.json file");
        }


        var indices = clientOptions.ConvertAll(o => int.Parse(o.SerialNumberArrayIndex));

        for (int i = 0; i < indices.Count; i++)
        {
            if (indices[i] != i)
            {
                throw new InvalidConfigurationException(
                    $"SerialNumberArrayIndex values must be consecutive and in order starting from 0. " +
                    $"Found {indices[i]} at position {i}. Check configuration in ClientSimulationConfig.json file");
            }
        }
    }
}
