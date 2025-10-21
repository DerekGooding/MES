using MES.Common;
using MES.Data;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;

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

        Log.Logger = new LoggerConfiguration()
            .WriteTo.Console()
            .WriteTo.File("Logs/server_log.txt", rollingInterval: RollingInterval.Day)
            .CreateLogger();


        // Create the host builder for the worker service
        var builder = Host.CreateApplicationBuilder(args);
        // Add Serilog to the DI container
        builder.Logging.AddSerilog(); 
        // Tell the builder that anytime an IPLCServerFactory is requested, return a PLCServerFactory
        builder.Services.AddSingleton<IPLCServerFactory, PLCServerFactory>();
        // Add PLCServerService as a background (hosted) service to the DI container
        builder.Services.AddHostedService(provider =>
        {
            var factory = provider.GetRequiredService<IPLCServerFactory>(); // Returns the PLCServerFactory
            var servers = factory.CreateServers(); // Uses the factory to create the list of PLCServers
            return new PLCServerService(servers); // Passes the list of PLCServers to the PLCServerService
        });

        // Build and run the host
        var host = builder.Build();
        // Wait for the host to shutdown
        await host.RunAsync();

    }
}
