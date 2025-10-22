
using Microsoft.Extensions.Hosting;

namespace MES;

internal class PLCServerService(List<PLCServer> servers) : BackgroundService
{
    private List<PLCServer> _servers = servers;
    private List<Task> _serverTasks = [];

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _servers.ForEach(server => _serverTasks.Add(server.StartAsync(stoppingToken)));

        await Task.WhenAll(_serverTasks);
    }
}
