

using System.Net;

namespace Common;

public static class ValidateStationConfig
{
    private static readonly string[] _validDataTypes = { "string", "int", "integer", "float", "real", "bool", "boolean" };
    public static void ValidateConfiguration(List<StationOptions> options)
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
}
