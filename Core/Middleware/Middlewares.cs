using Model;

namespace Core.Middleware;

public class Middlewares
{
    private List<IMiddleware> _middlewares = new List<IMiddleware>();

    public Middlewares Add(IMiddleware middleware)
    {
        _middlewares.Add(middleware);
        return this;
    }

    public Middlewares Remove(IMiddleware middleware)
    {
        _middlewares.Remove(middleware);
        return this;
    }

    public async Task InvokeAsync(BotContext context)
    {
        await ExecuteMiddlewareAsync(context, 0);
    }

    private async Task ExecuteMiddlewareAsync(BotContext context, int index)
    {
        if (index >= _middlewares.Count)
        {
            return;
        }

        var middleware = _middlewares[index];

        Func<Task> next = async () =>
        {
            await ExecuteMiddlewareAsync(context, index + 1);
        };

        await middleware.InvokeAsync(context, next);
    }
}
