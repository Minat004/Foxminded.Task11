using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace TelegramBot;

public class App
{
    private readonly IConfiguration _configuration;

    public App(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task RunAsync()
    {
        var botClient = new TelegramBotClient(_configuration.GetValue<string>("ApiKeyBot"));

        var cts = new CancellationTokenSource();

        var options = new ReceiverOptions()
        {   
            AllowedUpdates = Array.Empty<UpdateType>()
        };
        
        await botClient.ReceiveAsync(UpdateAsync, ErrorAsync, options, cts.Token);
        
        cts.Cancel();
    }

    private async Task UpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        var message = update.Message;
        
        if (message?.Text is null)
            return;
        
        var chatId = message.Chat.Id;
        
        var inlineKeyboard = new InlineKeyboardMarkup(new[]
        {
            InlineKeyboardButton.WithCallbackData("USD", "USD"),
            InlineKeyboardButton.WithCallbackData("EUR", "USD"),
        });
        
        var replyKeyboardMarkup = new ReplyKeyboardMarkup(new[]
        {
            new KeyboardButton[] { "USD 🇺🇸", "EUR 🇪🇺", "PLZ 🇵🇱" }
        })
        {
            ResizeKeyboard = true
        };

        if (message.Text == "/start")
        {
            await botClient.SendTextMessageAsync(
                chatId: chatId,
                text: "Select your currency",
                replyMarkup: replyKeyboardMarkup,
                cancellationToken: cancellationToken);
        }
        
        if (message.Text == "/Help")
        {
            await botClient.SendTextMessageAsync(
                chatId: chatId,
                text: "Start",
                cancellationToken: cancellationToken);
        }
        
        await botClient.SendTextMessageAsync(
            chatId: chatId,
            text: "You said:\n" + message.Text,
            cancellationToken: cancellationToken);
    }

    private Task ErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}