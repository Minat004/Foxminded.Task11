namespace TelegramBot.Core.Interfaces;

public interface IReceiver
{
    Task StartReceiveAsync(CancellationToken cancellationToken);
}