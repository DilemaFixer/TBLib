# TBLib 🤖

![License](https://img.shields.io/badge/license-MIT-blue.svg)
![Language](https://img.shields.io/badge/language-C%23-brightgreen.svg)
![Version](https://img.shields.io/badge/version-1.0.0-orange.svg)
![.NET](https://img.shields.io/badge/.NET-8.0-purple.svg)

A flexible and extensible C# library for building Telegram bots using a state-based architecture with middleware support.

## 📥 Installation

### Clone the repository

```bash
# Create a directory for the project
mkdir TBLib
cd TBLib

# Clone the repository
git clone https://github.com/DilemaFixer/TBLib.git .
```

## 🚀 Getting Started

TBLib is designed to make Telegram bot development easier by providing a clean, state-based architecture. Here's how to get started:

```csharp
// Initialize the bot client
var botClient = new TelegramBotClient("YOUR_BOT_TOKEN");

// Create state manager
var stateManager = new InMemoryStateManager();

// Create router
var router = new Router(stateManager, "main");

// Register your controllers
router.ParseStates(new YourStateController());
router.ParseSelectors(new YourSelectorController());
router.ParseActions(new YourActionController());

// Set up middleware pipeline
var middlewares = new Middlewares()
    .Add(new RouteMiddleware(router));

// Set up error handler
var errorHandler = new BaseErrorHandler();

// Set up context builder
var contextBuilder = new BaseContextBuilder();

// Create the bot
var bot = new Bot(botClient, middlewares, contextBuilder, errorHandler);

// Start the bot
bot.Start();
```

## 📚 API Documentation

### Core Concepts

TBLib is built around several core concepts:

- **States**: Represent different workflow states of your bot
- **Actions**: Operations that can be performed on specific states
- **Selectors**: Conditions to decide which actions to perform
- **Middleware**: Pipeline components for request processing

### State Management

The state system helps you organize your bot's workflow.

```csharp
public interface IStateManager
{
    Task<string> GetStateAsync(long chatId);  // Get current state for a chat
    Task SetStateAsync(long chatId, string state);  // Set state for a chat
    Task ClearStateAsync(long chatId);  // Clear state for a chat
}
```

**InMemoryStateManager**: Default implementation that stores states in memory.

```csharp
// Example: Creating a state manager
var stateManager = new InMemoryStateManager();
```

### Router

The router dispatches updates to the appropriate state and action handlers.

```csharp
// Creating a router with a base state
var router = new Router(stateManager, "main");

// Parsing states, selectors, and actions from controller classes
router.ParseStates(new MyStateController());
router.ParseSelectors(new MySelectorController());
router.ParseActions(new MyActionController());

// Redirecting to a specific state and action
await router.RedirectTo("stateName", "actionName", context);
```

### States

States represent different modes or workflows of your bot.

#### State Definition

```csharp
// Defining a state in a controller class
public class MyStateController
{
    [State("main", "menu")]  // Multiple state names can be specified
    public async Task MainState(BotContext context, Actions actions)
    {
        // State handler logic
        await context.Bot.SendTextMessageAsync(
            context.ChatId,
            "Welcome to the main menu!",
            cancellationToken: context.CancellationToken);
    }
}
```

When using multiple state names in the attribute, the state handler will be registered for all specified states.

### Actions

Actions are operations that can be performed in specific states.

#### Action Definition

```csharp
public class MyActionController
{
    [Action(
        action: "showHelp",
        selector: new[] { "CommandSelector" },  // Multiple selectors can be specified
        states: new[] { "main", "settings" },   // Multiple states can be specified
        order: 1,
        activateWithoutInterruption: false)]
    public async Task ShowHelp(BotContext context)
    {
        await context.Bot.SendTextMessageAsync(
            context.ChatId,
            "Help information...",
            cancellationToken: context.CancellationToken);
    }
}
```

- The `selector` parameter can contain multiple selector names, meaning the action will be triggered if ANY of the selectors return true.
- The `states` parameter can contain multiple state names, meaning the action will be registered with all the specified states.

### Selectors

Selectors determine whether an action should be executed based on the context.

#### Selector Definition

```csharp
public class MySelectorController
{
    [Selector("CommandSelector")]
    public bool IsCommand(BotContext context)
    {
        return context.Text?.StartsWith("/") ?? false;
    }

    [Selector("TextContainsSelector")]
    public bool ContainsText(BotContext context)
    {
        return !string.IsNullOrEmpty(context.Text);
    }
}
```

### Middleware

Middleware components process requests in a pipeline.

```csharp
// Creating a middleware pipeline
var middlewares = new Middlewares()
    .Add(new LoggingMiddleware())
    .Add(new RouteMiddleware(router))
    .Add(new ErrorHandlingMiddleware());
```

#### Custom Middleware

```csharp
public class LoggingMiddleware : IMiddleware
{
    public async Task InvokeAsync(BotContext context, Func<Task> next)
    {
        Console.WriteLine($"Received update from {context.ChatId}");
        await next();
        Console.WriteLine($"Processed update from {context.ChatId}");
    }
}
```

### Bot Context

The `BotContext` provides access to all relevant information about the current update.

```csharp
public class BotContext
{
    public Update Update;                     // The Telegram update
    public ITelegramBotClient Bot;            // The bot client
    public CancellationToken CancellationToken; // Cancellation token
    public UpdateType UpdateType => Update.Type; // The type of update
    public string Text;                       // Extracted text from the update
    public long ChatId { get; set; }          // The chat ID
}
```

### Complete Example

Here's a complete example showing how to create a simple bot:

```csharp
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
```

## ⚙️ Dependencies

- [Telegram.Bot](https://github.com/TelegramBots/Telegram.Bot) - The official Telegram Bot API client for .NET
