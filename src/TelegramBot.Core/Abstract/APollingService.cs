using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TelegramBot.Core.Interfaces;

namespace TelegramBot.Core.Abstract;

public abstract class APollingService<TReceiver> : BackgroundService where TReceiver : IReceiver
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<APollingService<TReceiver>> _logger;

    protected APollingService(IServiceProvider serviceProvider, ILogger<APollingService<TReceiver>> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Starting polling service");

        await DoWork(cancellationToken);
    }

    private async Task DoWork(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var receiver = scope.ServiceProvider.GetRequiredService<TReceiver>();

                await receiver.StartReceiveAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError("Polling failed with exception: {Exception}", ex);

                await Task.Delay(TimeSpan.FromSeconds(5), cancellationToken);
            }
        }
    }
}