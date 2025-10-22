using MES.Common.Exceptions;
using System.Text.Json;

namespace MES.Common;

public static class DbConnectionHelper
{
    public static string GetConnectionString()
    {
        var baseDir = AppContext.BaseDirectory;
        var solutionDir = Directory.GetParent(baseDir).Parent.Parent.Parent.Parent.FullName;

        try
        {
            if (!Directory.Exists(Path.Combine(solutionDir, "Data")))
            {
                Directory.CreateDirectory(Path.Combine(solutionDir, "Data"));
            }

            var dataDir = Path.Combine(solutionDir, "Data");
            var dbConfigPath = Path.Combine(AppContext.BaseDirectory, "Config", "DatabaseConfig.json");
            var dbConfigJson = File.ReadAllText(dbConfigPath);
            using var jsonDoc = JsonDocument.Parse(dbConfigJson);
            var configConnString = jsonDoc.RootElement.GetProperty("ConnectionString").GetString();
            var dbName = configConnString.Replace("Data Source=", "").Trim();
            var dbFullPath = Path.Combine(dataDir, dbName);
            return $"Data Source={dbFullPath}";
        }
        catch (Exception ex)
        {

            throw new InvalidConfigurationException($"There was a problem getting the path to the database: {ex}");
        }
    }
}
