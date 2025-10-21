
using Microsoft.Extensions.Hosting;

namespace MES;

internal class PLCServerService : BackgroundService
{
    private List<PLCServer> _servers;
    private List<Task> _serverTasks;

    public PLCServerService(List<PLCServer> servers)
    {
        _servers = servers;
        _serverTasks = new List<Task>();
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _servers.ForEach(server =>
        {
            _serverTasks.Add(server.StartAsync(stoppingToken));
        });

        await Task.WhenAll(_serverTasks);
    }
}
