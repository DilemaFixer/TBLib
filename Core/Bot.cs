using Core.Middleware;
using Model;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;

namespace Core;

public class Bot
{
    private ITelegramBotClient _client;
    private Middlewares _middlewares;
    private IContextBuilder _contextBuilder;
    private IErrorHandler _errorHandler;
    private CancellationTokenSource _cancellationToken;

    public Bot(ITelegramBotClient client, Middlewares middlewares, IContextBuilder contextBuilder, IErrorHandler errorHandler)
    {
        ArgumentNullException.ThrowIfNull(client);
        ArgumentNullException.ThrowIfNull(middlewares);
        ArgumentNullException.ThrowIfNull(contextBuilder);
        ArgumentNullException.ThrowIfNull(errorHandler);
        
        _client = client;
        _middlewares = middlewares;
        _contextBuilder = contextBuilder;
        _errorHandler = errorHandler;
    }

    public void Start(ReceiverOptions? receiverOptions = null)
    {
        _cancellationToken = new CancellationTokenSource();
        _client.StartReceiving(OnUpdateAsync, OnErrorAsync , receiverOptions , _cancellationToken.Token);
    }

    public void Stop() => _cancellationToken.Cancel();
    
    private async Task OnUpdateAsync(ITelegramBotClient bot , Update update , CancellationToken cancellationToken)
    {
        BotContext context = _contextBuilder.Build(bot, update, cancellationToken);
        await _middlewares.InvokeAsync(context);
    }
    
    private async Task OnErrorAsync(ITelegramBotClient bot, Exception exception, CancellationToken cancellationToken) =>
        await _errorHandler.OnErrorAsync(bot, exception, cancellationToken);
}