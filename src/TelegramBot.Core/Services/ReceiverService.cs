using Microsoft.Extensions.Logging;
using Telegram.Bot;
using TelegramBot.Core.Abstract;

namespace TelegramBot.Core.Services;

public class ReceiverService : AReceiverService<UpdateHandler>
{
    public ReceiverService(ITelegramBotClient botClient, UpdateHandler updateHandler, 
        ILogger<ReceiverService> logger) : base(botClient, updateHandler, logger)
    {
    }
}