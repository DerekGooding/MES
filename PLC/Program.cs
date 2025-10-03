using Common;
using System.Text.Json;

namespace PLC
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            List<ClientSimulationOptions> clientSimulationOptions;
            List<StationOptions> stationOptions;

            JsonSerializerOptions jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            try
            {
                clientSimulationOptions = JsonSerializer.Deserialize<List<ClientSimulationOptions>>(File.ReadAllText("Config/ClientSimulationConfig.json"), jsonOptions);
                stationOptions = JsonSerializer.Deserialize<List<StationOptions>>(File.ReadAllText("Config/StationConfig.json"), jsonOptions);
                ValidateConfig.ValidateStationConfig(stationOptions);
                ValidateConfig.ValidateClientSimulationConfig(clientSimulationOptions, stationOptions);
            }
            catch (Exception e)
            {

                Console.WriteLine($"Error loading configuration: {e.Message}");
                Console.ReadKey();
                return;
            }

            SerialNumberGenerator serialGen = new SerialNumberGenerator("AA", 0, 6, 5);

            List<PLCStation> stations = new List<PLCStation>();

            foreach (var stationOption in stationOptions)
            {
                var clientSimulationOption = clientSimulationOptions
                    .Find(c => c.StationName == stationOption.StationName);

                stations.Add(new PLCStation(stationOption, clientSimulationOption, serialGen.serialNumbers));
            }



            //PLCStation stationTen = new PLCStation("127.0.0.1", 5000, "Station 10", serialGen.serialNumbers, 0, 6000, 9000);
            //PLCStation stationTwenty = new PLCStation("127.0.0.1", 5001, "Station 20", serialGen.serialNumbers, 1, 6000, 9000);
            //PLCStation stationThirty = new PLCStation("127.0.0.1", 5002, "Station 30", serialGen.serialNumbers, 2, 6000, 9000);

            //List<PLCStation> stations = new List<PLCStation>
            //{
            //    stationTen,
            //    stationTwenty,
            //    stationThirty
            //};

            Coordinator coordinator = new Coordinator(stations);
            

            while (true)
            {
                serialGen.GenerateSerialNumbers();
                await coordinator.Coordinate();
                Console.WriteLine("Coordinate Complete");
                //await Task.Delay(3000); 
                coordinator.Reset(); 
            }

        }
    }
}
