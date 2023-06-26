using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace TelegramBot.Core.Services;

public class UpdateHandler : IUpdateHandler
{
    private readonly ITelegramBotClient _botClient;
    private readonly ILogger<UpdateHandler> _logger;

    public UpdateHandler(ITelegramBotClient botClient, ILogger<UpdateHandler> logger)
    {
        _botClient = botClient;
        _logger = logger;
    }

    public async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        var handler = update switch
        {
            { Message: { } message } => OnMessageReceived(message, cancellationToken),
            { EditedMessage: { } message } => OnMessageReceived(message, cancellationToken),
            { CallbackQuery: { } callbackQuery } => OnCallBackQueryReceived(callbackQuery, cancellationToken),
            _ => throw new ArgumentOutOfRangeException(nameof(update), update, null)
        };

        await handler;
    }

    private async Task OnMessageReceived(Message message, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Receive message type: {MessageType}", message.Type);

        if (message.Text is not { } messageText) return;
        
        var action = messageText.Split(' ')[0] switch
        {
            "/start" => SendSelectCurrency(_botClient, message, cancellationToken),
            "/help" => SendHelpMessage(_botClient, message, cancellationToken),
            _ => SendHelpMessage(_botClient, message, cancellationToken)
        };

        var sendMessage = await action;
        _logger.LogInformation("The message was sent with id: {SendMessageId}", sendMessage.MessageId);

        static async Task<Message> SendSelectCurrency(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
        {
            var currencyInlineKeyboard = new InlineKeyboardMarkup(new[]
            {
                InlineKeyboardButton.WithCallbackData("USD 🇺🇸", "USD"),
                InlineKeyboardButton.WithCallbackData("EUR 🇪🇺", "EUR"),
                InlineKeyboardButton.WithCallbackData("PLZ 🇵🇱", "PLZ")
            });
            
            return await botClient.SendTextMessageAsync(
                chatId: message.Chat.Id,
                text: "Select currency:",
                replyMarkup: currencyInlineKeyboard,
                cancellationToken: cancellationToken);
        }
    }

    private async Task OnCallBackQueryReceived(CallbackQuery callbackQuery, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Received inline keyboard callback from: {CallbackQueryId}", callbackQuery.Id);

        if (callbackQuery.Data is not {} callbackQueryData) return;

        var currentRate = string.Empty;

        var action = callbackQueryData.Split(' ')[0] switch
        {
            "USD" => SendSelectDate(_botClient, callbackQuery, cancellationToken),
            "EUR" => SendSelectDate(_botClient, callbackQuery, cancellationToken),
            "PLZ" => SendSelectDate(_botClient, callbackQuery, cancellationToken),
            "Today" => SendTypeDate(_botClient, callbackQuery, cancellationToken),
            "SetDate" => SendTypeDate(_botClient, callbackQuery, cancellationToken),
            _ => SendHelpMessage(_botClient, callbackQuery.Message!, cancellationToken)
        };
        
        var sendMessage = await action;
        _logger.LogInformation("The message was sent with id: {SendMessageId}", sendMessage.MessageId);
            
        static async Task<Message> SendSelectDate(ITelegramBotClient botClient, CallbackQuery callbackQuery, CancellationToken cancellationToken)
        {
            var dateInlineKeyboard = new InlineKeyboardMarkup(new[]
            {
                InlineKeyboardButton.WithCallbackData("Today", "Today"),
                InlineKeyboardButton.WithCallbackData("Set date", "SetDate")
            });
        
            return await botClient.EditMessageTextAsync(
                chatId: callbackQuery.Message!.Chat.Id,
                messageId: callbackQuery.Message.MessageId,
                text: "Select date:",
                replyMarkup: dateInlineKeyboard,
                cancellationToken: cancellationToken);
        }
        
        static async Task<Message> SendTypeDate(ITelegramBotClient botClient, CallbackQuery callbackQuery, CancellationToken cancellationToken)
        {
            return await botClient.SendTextMessageAsync(
                chatId: callbackQuery.Message!.Chat.Id,
                text: "Type date [format - dd:mm:yyyy]",
                cancellationToken: cancellationToken);
        }
    }
            
    private static async Task<Message> SendHelpMessage(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
    {
        const string helpMessage = "/start - start to select currency\n" +
                                   "/help - help";
            
        return await botClient.SendTextMessageAsync(
            chatId: message.Chat.Id,
            text: helpMessage,
            cancellationToken: cancellationToken);
    }

    public Task HandlePollingErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}