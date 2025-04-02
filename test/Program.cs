using System;
using System.Threading;
using System.Threading.Tasks;
using Core;
using Core.Middleware;
using Model;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace SimpleTelegramBot
{
    class Program
    {
        static async Task Main(string[] args)
        {
            // Set your bot token here
            var botToken = "YOUR_BOT_TOKEN";
            var botClient = new TelegramBotClient(botToken);

            // Create the state manager
            var stateManager = new InMemoryStateManager();

            // Create the router with "main" as the base state
            var router = new Router(stateManager, "main");

            // Register controllers
            var botController = new BotController();
            router.ParseStates(botController);
            router.ParseSelectors(botController);
            router.ParseActions(botController);

            // Create middleware pipeline
            var middlewares = new Middlewares()
                .Add(new LoggingMiddleware())
                .Add(new RouteMiddleware(router));

            // Create error handler
            var errorHandler = new CustomErrorHandler();
            
            // Create context builder
            var contextBuilder = new CustomContextBuilder();

            // Create and start the bot
            var bot = new Bot(botClient, middlewares, contextBuilder, errorHandler);
            
            Console.WriteLine("Starting bot...");
            bot.Start();
            Console.WriteLine("Bot is running. Press Enter to exit.");
            Console.ReadLine();
            
            // Stop the bot
            bot.Stop();
            Console.WriteLine("Bot stopped.");
        }
    }

    // Custom error handler
    public class CustomErrorHandler : IErrorHandler
    {
        public Task OnErrorAsync(ITelegramBotClient bot, Exception exception, CancellationToken cancellationToken)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"Error occurred: {exception.Message}");
            Console.WriteLine(exception.StackTrace);
            Console.ResetColor();
            return Task.CompletedTask;
        }
    }

    // Custom context builder that extracts chat ID
    public class CustomContextBuilder : BaseContextBuilder
    {
        public  BotContext Build(ITelegramBotClient bot, Update update, CancellationToken cancellationToken)
        {
            var context = base.Build(bot, update, cancellationToken);
            
            // Extract chat ID based on update type
            if (update.Type == UpdateType.Message && update.Message != null)
            {
                context.ChatId = update.Message.Chat.Id;
            }
            else if (update.Type == UpdateType.CallbackQuery && update.CallbackQuery?.Message != null)
            {
                context.ChatId = update.CallbackQuery.Message.Chat.Id;
            }
            
            return context;
        }
    }

    // Custom logging middleware
    public class LoggingMiddleware : IMiddleware
    {
        public async Task InvokeAsync(BotContext context, Func<Task> next)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine($"[{DateTime.Now}] Received update from Chat ID: {context.ChatId}, Text: {context.Text ?? "N/A"}");
            Console.ResetColor();
            
            // Call the next middleware in the pipeline
            await next();
            
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"[{DateTime.Now}] Finished processing update");
            Console.ResetColor();
        }
    }

    // Controller containing all state, selector and action handlers
    public class BotController
    {
        // --------------- STATES ---------------
        
        [State("main")]
        public async Task MainState(BotContext context, Actions actions)
        {
            Console.WriteLine("Handling 'main' state");
            // Main state's default behavior can be empty - actions will handle specific commands
        }
        
        [State("menu")]
        public async Task MenuState(BotContext context, Actions actions)
        {
            Console.WriteLine("Handling 'menu' state");
            // Menu state's default behavior can be empty - actions will handle specific commands
        }
        
        [State("order")]
        public async Task OrderState(BotContext context, Actions actions)
        {
            Console.WriteLine("Handling 'order' state");
            // Order state's default behavior can be empty - actions will handle specific commands
        }

        // --------------- SELECTORS ---------------
        
        [Selector("CommandSelector")]
        public bool IsCommand(BotContext context)
        {
            // Check if message starts with '/'
            return context.Text?.StartsWith("/") == true;
        }
        
        [Selector("CallbackSelector")]
        public bool IsCallback(BotContext context)
        {
            // Check if update is a callback query
            return context.UpdateType == UpdateType.CallbackQuery;
        }
        
        [Selector("TextSelector")]
        public bool HasText(BotContext context)
        {
            // Check if message has any text
            return !string.IsNullOrEmpty(context.Text);
        }

        // --------------- ACTIONS ---------------
        
        // /start command - works in any state
        [Action(
            action: "start",
            selector: new[] { "CommandSelector" },
            states: new[] { "main", "menu", "order" },
            order: 0)]
        public async Task Start(BotContext context)
        {
            if (context.Text == "/start")
            {
                var keyboard = new ReplyKeyboardMarkup(new[]
                {
                    new[] { new KeyboardButton("📋 Menu") ,  new KeyboardButton("🛒 Order") },
                    new[] {  new KeyboardButton("ℹ️ About"),  new KeyboardButton("🆘 Help")}
                })
                {
                    ResizeKeyboard = true
                };

                await context.Bot.SendMessage(
                    chatId: context.ChatId,
                    text: "👋 Welcome to the Sample Bot!\nUse the keyboard below to navigate.",
                    replyMarkup: keyboard,
                    cancellationToken: context.CancellationToken);
                
                // Set the state to main
                await SetState(context, "main");
            }
        }
        
        // /menu command or "📋 Menu" button - works in any state
        [Action(
            action: "menu",
            selector: new[] { "CommandSelector", "TextSelector" },
            states: new[] { "main", "menu", "order" },
            order: 1)]
        public async Task ShowMenu(BotContext context)
        {
            if (context.Text == "/menu" || context.Text == "📋 Menu")
            {
                var inlineKeyboard = new InlineKeyboardMarkup(new[]
                {
                    new[] 
                    { 
                        InlineKeyboardButton.WithCallbackData("🍕 Pizza", "menu_pizza"),
                        InlineKeyboardButton.WithCallbackData("🍔 Burger", "menu_burger")
                    },
                    new[] 
                    { 
                        InlineKeyboardButton.WithCallbackData("🍣 Sushi", "menu_sushi"),
                        InlineKeyboardButton.WithCallbackData("🍝 Pasta", "menu_pasta")
                    },
                    new[]
                    {
                        InlineKeyboardButton.WithCallbackData("🔙 Back to Main", "back_main")
                    }
                });

                await context.Bot.SendMessage(
                    chatId: context.ChatId,
                    text: "📋 *Menu*\nSelect a category to view items:",
                    parseMode: ParseMode.Markdown,
                    replyMarkup: inlineKeyboard,
                    cancellationToken: context.CancellationToken);
                
                // Set the state to menu
                await SetState(context, "menu");
            }
        }
        
        // Handles menu item selection
        [Action(
            action: "menuItem",
            selector: new[] { "CallbackSelector" },
            states: new[] { "menu" },
            order: 0)]
        public async Task HandleMenuItem(BotContext context)
        {
            if (context.Update.CallbackQuery == null) return;
            
            var callbackData = context.Update.CallbackQuery.Data;
            
            // First, answer the callback query to stop the loading indicator
            await context.Bot.AnswerCallbackQuery(
                callbackQueryId: context.Update.CallbackQuery.Id,
                cancellationToken: context.CancellationToken);
            
            string itemText = "Unknown menu item";
            string itemDescription = "No description available";
            string itemPrice = "0.00";
            
            // Handle different menu selections
            switch (callbackData)
            {
                case "menu_pizza":
                    itemText = "🍕 Pizza";
                    itemDescription = "Authentic Italian pizza with various toppings";
                    itemPrice = "12.99";
                    break;
                case "menu_burger":
                    itemText = "🍔 Burger";
                    itemDescription = "Juicy beef burger with cheese and fresh vegetables";
                    itemPrice = "9.99";
                    break;
                case "menu_sushi":
                    itemText = "🍣 Sushi";
                    itemDescription = "Fresh sushi rolls with salmon, tuna, and avocado";
                    itemPrice = "15.99";
                    break;
                case "menu_pasta":
                    itemText = "🍝 Pasta";
                    itemDescription = "Homemade pasta with rich sauce and parmesan";
                    itemPrice = "11.99";
                    break;
                case "back_main":
                    // Return to main menu
                    await context.Bot.SendMessage(
                        chatId: context.ChatId,
                        text: "Returning to main menu...",
                        cancellationToken: context.CancellationToken);
                    
                    await SetState(context, "main");
                    return;
            }
            
            // Show item details with order button
            var orderKeyboard = new InlineKeyboardMarkup(new[]
            {
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("🛒 Add to Order", $"order_{callbackData.Substring(5)}")
                },
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("🔙 Back to Menu", "back_menu")
                }
            });
            
            await context.Bot.SendMessage(
                chatId: context.ChatId,
                text: $"*{itemText}*\n\n{itemDescription}\n\nPrice: ${itemPrice}",
                parseMode: ParseMode.Markdown,
                replyMarkup: orderKeyboard,
                cancellationToken: context.CancellationToken);
        }
        
        // Handle "🛒 Order" button or /order command
        [Action(
            action: "order",
            selector: new[] { "CommandSelector", "TextSelector" },
            states: new[] { "main", "menu", "order" },
            order: 1)]
        public async Task ShowOrder(BotContext context)
        {
            if (context.Text == "/order" || context.Text == "🛒 Order")
            {
                var keyboard = new InlineKeyboardMarkup(new[]
                {
                    new[] 
                    { 
                        InlineKeyboardButton.WithCallbackData("🧹 Clear Order", "clear_order") 
                    },
                    new[] 
                    { 
                        InlineKeyboardButton.WithCallbackData("💰 Checkout", "checkout")
                    },
                    new[]
                    {
                        InlineKeyboardButton.WithCallbackData("🔙 Back to Main", "back_main")
                    }
                });

                await context.Bot.SendMessage(
                    chatId: context.ChatId,
                    text: "🛒 *Your Order*\n\nYour order is currently empty. Browse the menu to add items.",
                    parseMode: ParseMode.Markdown,
                    replyMarkup: keyboard,
                    cancellationToken: context.CancellationToken);
                
                // Set the state to order
                await SetState(context, "order");
            }
        }
        
        // Handle "ℹ️ About" button or /about command
        [Action(
            action: "about",
            selector: new[] { "CommandSelector", "TextSelector" },
            states: new[] { "main", "menu", "order" },
            order: 2)]
        public async Task ShowAbout(BotContext context)
        {
            if (context.Text == "/about" || context.Text == "ℹ️ About")
            {
                await context.Bot.SendMessage(
                    chatId: context.ChatId,
                    text: "ℹ️ *About This Bot*\n\nThis is a sample bot built using TBLib - a C# library for Telegram bots. It demonstrates states, actions, selectors, and middleware functionality.",
                    parseMode: ParseMode.Markdown,
                    cancellationToken: context.CancellationToken);
            }
        }
        
        // Handle "🆘 Help" button or /help command
        [Action(
            action: "help",
            selector: new[] { "CommandSelector", "TextSelector" },
            states: new[] { "main", "menu", "order" },
            order: 2)]
        public async Task ShowHelp(BotContext context)
        {
            if (context.Text == "/help" || context.Text == "🆘 Help")
            {
                await context.Bot.SendMessage(
                    chatId: context.ChatId,
                    text: "🆘 *Available Commands*\n\n" +
                          "/start - Start or restart the bot\n" +
                          "/menu - Browse our menu\n" +
                          "/order - View your current order\n" +
                          "/about - Learn about this bot\n" +
                          "/help - Show this help message",
                    parseMode: ParseMode.Markdown,
                    cancellationToken: context.CancellationToken);
            }
        }
        
        // Handle "back_menu" callback - return to menu
        [Action(
            action: "backToMenu",
            selector: new[] { "CallbackSelector" },
            states: new[] { "menu" },
            order: 1)]
        public async Task BackToMenu(BotContext context)
        {
            if (context.Update.CallbackQuery?.Data == "back_menu")
            {
                // Answer the callback query
                await context.Bot.AnswerCallbackQuery(
                    callbackQueryId: context.Update.CallbackQuery.Id,
                    cancellationToken: context.CancellationToken);
                
                // Redirect to the menu action
                context.Text = "/menu";
                await ShowMenu(context);
            }
        }
        
        // Handle unknown commands or text - works in any state as a fallback
        [Action(
            action: "unknown",
            selector: new[] { "TextSelector" },
            states: new[] { "main", "menu", "order" },
            order: 999)] // High order number to run last
        public async Task HandleUnknown(BotContext context)
        {
            // Skip if it was a command or handled by another action
            if (context.Text?.StartsWith("/") == true ||
                context.Text == "📋 Menu" ||
                context.Text == "🛒 Order" ||
                context.Text == "ℹ️ About" ||
                context.Text == "🆘 Help")
                return;

            await context.Bot.SendMessage(
                chatId: context.ChatId,
                text: "I don't understand that command. Use /help to see available commands.",
                cancellationToken: context.CancellationToken);
        }
        
        // Helper method to set the user's state
        private async Task SetState(BotContext context, string state)
        {
            var stateManager = TelegramBotClientExtensions.Container.GetService(typeof(IStateManager)) as IStateManager;
            if (stateManager != null)
            {
                await stateManager.SetStateAsync(context.ChatId, state);
            }
        }
    }
    
    // Extension to simulate dependency injection container
    // Note: This is for demonstration purposes only
    public static class TelegramBotClientExtensions
    {
        public static DependencyContainer Container { get; set; } = new DependencyContainer();
        
        public class DependencyContainer
        {
            private Dictionary<Type, object> _services = new Dictionary<Type, object>();
            
            public void Register<T>(T service)
            {
                _services[typeof(T)] = service;
            }
            
            public object GetService(Type type)
            {
                if (_services.TryGetValue(type, out var service))
                {
                    return service;
                }
                return null;
            }
        }
    }
}