using Telegram.Bot;

namespace Core;

public class BaseErrorHandler : IErrorHandler
{
    public Task OnErrorAsync(ITelegramBotClient bot, Exception exception, CancellationToken cancellationToken)
    {
        Console.WriteLine(exception.Message + "\n" + exception.StackTrace);
        return Task.CompletedTask;
    }
}