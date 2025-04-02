using Model;

namespace Core.Middleware;

public class RouteMiddleware : IMiddleware
{
    private Router _router;

    public RouteMiddleware(Router router) => 
        _router = router;

    public async Task InvokeAsync(BotContext context, Func<Task> next)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(next);
        
        await _router.Handle(context);
        
        await next();
    }
}