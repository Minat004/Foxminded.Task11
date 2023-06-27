using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace TelegramBot.Core.Services;

public class UpdateHandler : IUpdateHandler
{
    private readonly ITelegramBotClient _botClient;
    private readonly ILogger<UpdateHandler> _logger;
    private readonly ExchangeArchiveService _exchangeArchiveService;

    public UpdateHandler(ITelegramBotClient botClient, ILogger<UpdateHandler> logger, ExchangeArchiveService exchangeArchiveService)
    {
        _botClient = botClient;
        _logger = logger;
        _exchangeArchiveService = exchangeArchiveService;
    }

    private static string SelectedCurrency { get; set; } = string.Empty;

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
            _ => IsMatchDate(_botClient, message, cancellationToken, _exchangeArchiveService, _logger)
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

        static async Task<Message> IsMatchDate(ITelegramBotClient botClient, Message message, 
            CancellationToken cancellationToken, ExchangeArchiveService exchangeArchiveService, ILogger logger)
        {
            if (!string.IsNullOrEmpty(SelectedCurrency) && Regex.IsMatch(message.Text!, @"^\d{2}.\d{2}.\d{4}$"))
            {
                var dates = message.Text!.Split(".");

                try
                {
                    var date = new DateTime(int.Parse(dates[2]), int.Parse(dates[1]), int.Parse(dates[0]));

                    return await SendDateMessage(botClient, message, exchangeArchiveService, date, cancellationToken);
                }
                catch (ArgumentOutOfRangeException ex)
                {
                    logger.LogWarning("Exception message: {Exception}", ex.Message);

                    return await botClient.SendTextMessageAsync(
                        chatId: message.Chat.Id,
                        text: "Bad date time format try again...\n" +
                              "[format - dd.MM.yyyy {01.12.2014}]",
                        cancellationToken: cancellationToken);
                }
            }

            return await SendHelpMessage(botClient, message, cancellationToken);
        }
    }

    private async Task OnCallBackQueryReceived(CallbackQuery callbackQuery, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Received inline keyboard callback from: {CallbackQueryId}", callbackQuery.Id);

        if (callbackQuery.Data is not {} callbackQueryData) return;

        var action = callbackQueryData.Split(' ')[0] switch
        {
            "USD" => SendSelectDate(_botClient, callbackQuery, cancellationToken),
            "EUR" => SendSelectDate(_botClient, callbackQuery, cancellationToken),
            "PLZ" => SendSelectDate(_botClient, callbackQuery, cancellationToken),
            "Today" => SendDateMessage(_botClient, callbackQuery.Message!, _exchangeArchiveService, DateTime.Today, cancellationToken),
            "SetDate" => SendTypeDate(_botClient, callbackQuery, cancellationToken),
            _ => SendHelpMessage(_botClient, callbackQuery.Message!, cancellationToken)
        };
        
        var sendMessage = await action;
        _logger.LogInformation("The message was sent with id: {SendMessageId}", sendMessage.MessageId);
            
        static async Task<Message> SendSelectDate(ITelegramBotClient botClient, CallbackQuery callbackQuery, CancellationToken cancellationToken)
        {
            SelectedCurrency = callbackQuery.Data!;
            
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
                text: "Type date:\n" +
                      "[format - dd.MM.yyyy {01.12.2014}]",
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
    
    private static async Task<Message> SendDateMessage(ITelegramBotClient botClient, Message message, 
        ExchangeArchiveService exchangeArchiveService, DateTime dateTime, CancellationToken cancellationToken)
    {
        var chatId = message.Chat.Id;
        
        await botClient.SendChatActionAsync(
            chatId: chatId,
            chatAction: ChatAction.Typing,
            cancellationToken: cancellationToken);
        
        await Task.Delay(500, cancellationToken);

        var exchange = await exchangeArchiveService.GetExchangeRate(dateTime);

        var exchangeRate = exchange!.ExchangeRates!.FirstOrDefault(x => x.Currency == SelectedCurrency);

        if (exchangeRate is null && SelectedCurrency == "PLZ")
        {
            exchangeRate = exchange.ExchangeRates!.FirstOrDefault(x => x.Currency == "PLN");
        }

        if (exchangeRate is null)
        {
            return await botClient.SendTextMessageAsync(
                chatId: chatId,
                text: "Something went wrong. Try type another date:",
                cancellationToken: cancellationToken);
        }

        return await botClient.SendTextMessageAsync(
            chatId: chatId,
            text: $"Exchange currency: {SelectedCurrency}\n" +
                  $"Date: {dateTime:dd.MM.yyyy}\n" +
                  $"Sale rate: {exchangeRate!.SaleRate}\n" +
                  $"Purchase rate: {exchangeRate.PurchaseRate}",
            cancellationToken: cancellationToken);
    }

    public async Task HandlePollingErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
    {
        var ErrorMessage = exception switch
        {
            ApiRequestException apiRequestException => $"Telegram API Error:\n" +
                                                       $"[{apiRequestException.ErrorCode}]\n" +
                                                       $"{apiRequestException.Message}",
            _ => exception.ToString()
        };

        _logger.LogInformation("HandleError: {ErrorMessage}", ErrorMessage);

        if (exception is RequestException) await Task.Delay(TimeSpan.FromSeconds(2), cancellationToken);
    }
}