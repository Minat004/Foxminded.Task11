using Microsoft.Extensions.Logging;
using TelegramBot.Core.Abstract;

namespace TelegramBot.Core.Services;

public class PollingService : APollingService<ReceiverService>
{
    public PollingService(IServiceProvider serviceProvider, 
        ILogger<PollingService> logger) : base(serviceProvider, logger)
    {
    }
}