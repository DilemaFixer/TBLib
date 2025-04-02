// Program.cs
using Core;
using Core.Middleware;
using Model;
using Telegram.Bot;

// Create bot client
var botClient = new TelegramBotClient("YOUR_BOT_TOKEN");

// Create state manager
var stateManager = new InMemoryStateManager();

// Create router
var router = new Router(stateManager, "main");

// Register controllers
router.ParseStates(new MyStateController());
router.ParseSelectors(new MySelectorController());
router.ParseActions(new MyActionController());

// Create middleware pipeline
var middlewares = new Middlewares()
    .Add(new RouteMiddleware(router));

// Create error handler and context builder
var errorHandler = new BaseErrorHandler();
var contextBuilder = new BaseContextBuilder();

// Create and start the bot
var bot = new Bot(botClient, middlewares, contextBuilder, errorHandler);
bot.Start();

Console.WriteLine("Bot started. Press any key to exit.");
Console.ReadKey();

// Stop the bot
bot.Stop();

// State Controller
public class MyStateController
{
    [State("main")]
    public Task MainState(BotContext context, Actions actions)
    {
        // Main state logic
        return Task.CompletedTask;
    }
}

// Selector Controller
public class MySelectorController
{
    [Selector("CommandSelector")]
    public bool IsCommand(BotContext context)
    {
        return context.Text?.StartsWith("/") ?? false;
    }
}

// Action Controller
public class MyActionController
{
    [Action(
        action: "start",
        selector: new[] { "CommandSelector" },
        states: new[] { "main" })]
    public async Task Start(BotContext context)
    {
        if (context.Text == "/start")
        {
            await context.Bot.SendTextMessageAsync(
                context.ChatId,
                "Welcome to the bot!",
                cancellationToken: context.CancellationToken);
        }
    }
}
