using Model;

namespace Core.Middleware;

public interface IMiddleware
{
    Task InvokeAsync(BotContext context, Func<Task> next);
}