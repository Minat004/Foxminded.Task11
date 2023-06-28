using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types.Enums;
using TelegramBot.Core.Interfaces;

namespace TelegramBot.Core.Abstract;

public abstract class AReceiverService<TUpdate> : IReceiver where TUpdate : IUpdateHandler
{
    private readonly ITelegramBotClient _botClient;
    private readonly TUpdate _updateHandler;
    private readonly ILogger<AReceiverService<TUpdate>> _logger;

    protected AReceiverService(ITelegramBotClient botClient, TUpdate updateHandler, ILogger<AReceiverService<TUpdate>> logger)
    {
        _botClient = botClient;
        _updateHandler = updateHandler;
        _logger = logger;
    }

    public async Task StartReceiveAsync(CancellationToken cancellationToken)
    {
        var receiverOptions = new ReceiverOptions()
        {   
            AllowedUpdates = new[] { UpdateType.Message, UpdateType.CallbackQuery }
        };

        var me = await _botClient.GetMeAsync(cancellationToken);
        
        _logger.LogInformation("Start receiving updates for {Name}", me.Username ?? "Currency Exchange");

        await _botClient.ReceiveAsync(
            updateHandler: _updateHandler,
            receiverOptions: receiverOptions,
            cancellationToken: cancellationToken);
    }
}