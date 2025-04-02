using Telegram.Bot;

namespace Core;

public interface IErrorHandler
{
    public Task OnErrorAsync(ITelegramBotClient bot, Exception exception, CancellationToken cancellationToken);
}