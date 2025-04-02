using System;
using System.Threading.Tasks;
using Core;
using Core.Middleware;
using Core.States;
using Model;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace ExampleBot
{
    class Program
    {
        private const string TOKEN = "YOUR_BOT_TOKEN";

        static async Task Main(string[] args)
        {
            // 1. Инициализация компонентов бота
            var botClient = new TelegramBotClient(TOKEN);
            var middlewares = new Middlewares();
            var stateManager = new InMemoryStateManager();
            var router = new Router(stateManager, "main");
            
            // 2. Создание и настройка обработчиков
            var handlers = new BotHandlers();
            
            // 3. Парсинг состояний, селекторов и действий
            router.ParseStates(handlers);
            router.ParseSelectors(handlers);
            router.ParseActions(handlers);
            
            // 4. Добавление middleware для маршрутизации
            middlewares.Add(new LoggingMiddleware());
            middlewares.Add(new RouteMiddleware(router));
            
            // 5. Создание и запуск бота
            var bot = new Bot(
                botClient,
                middlewares,
                new BaseContextBuilder(),
                new BaseErrorHandler()
            );
            
            Console.WriteLine("Бот запущен! Нажмите Enter для остановки.");
            bot.Start();
            
            Console.ReadLine();
            bot.Stop();
            Console.WriteLine("Бот остановлен.");
        }
    }

    // Класс для вспомогательного middleware для логирования
    public class LoggingMiddleware : IMiddleware
    {
        public async Task InvokeAsync(BotContext context, Func<Task> next)
        {
            Console.WriteLine($"[{DateTime.Now}] Получено обновление: {context.UpdateType}");
            
            if (!string.IsNullOrEmpty(context.Text))
                Console.WriteLine($"Текст: {context.Text}");
                
            await next();
            
            Console.WriteLine($"[{DateTime.Now}] Обработка завершена");
        }
    }

    // Основной класс обработчиков бота
    public class BotHandlers
    {
        #region Состояния
        
        [State("main")]
        public async Task MainState(BotContext context, Actions actions)
        {
            // Логика состояния main выполняется при первом обращении к боту
            // Обработка сообщений производится через actions
            await actions.Handle(context);
        }
        
        [State("profile")]
        public async Task ProfileState(BotContext context, Actions actions)
        {
            // Если текст является командой возврата, переключаемся на основное состояние
            if (context.Text == "/back")
            {
                await context.Bot.SendTextMessageAsync(
                    context.ChatId,
                    "Возвращаемся в главное меню",
                    cancellationToken: context.CancellationToken
                );
                
                await ChangeState(context, "main");
                return;
            }
            
            // Иначе выполняем обработку текущего состояния
            await actions.Handle(context);
        }
        
        [State("settings", "preferences")] // Один обработчик для двух состояний
        public async Task SettingsState(BotContext context, Actions actions)
        {
            if (context.Text == "/back")
            {
                await context.Bot.SendTextMessageAsync(
                    context.ChatId,
                    "Возвращаемся в главное меню",
                    cancellationToken: context.CancellationToken
                );
                
                await ChangeState(context, "main");
                return;
            }
            
            await actions.Handle(context);
        }
        
        #endregion
        
        #region Селекторы
        
        [Selector("CommandSelector")]
        private bool IsCommand(BotContext context)
        {
            return !string.IsNullOrEmpty(context.Text) && context.Text.StartsWith("/");
        }
        
        [Selector("TextSelector")]
        private bool IsText(BotContext context)
        {
            return context.UpdateType == UpdateType.Message && 
                   !string.IsNullOrEmpty(context.Text) && 
                   !context.Text.StartsWith("/");
        }
        
        [Selector("CallbackSelector")]
        private bool IsCallback(BotContext context)
        {
            return context.UpdateType == UpdateType.CallbackQuery;
        }
        
        #endregion
        
        #region Действия для основного состояния
        
        [Action("StartCommand", new[] { "CommandSelector" }, new[] { "main" }, 0, false)]
        private async Task HandleStart(BotContext context)
        {
            if (context.Text == "/start")
            {
                var keyboard = new InlineKeyboardMarkup(new[]
                {
                    new[]
                    {
                        InlineKeyboardButton.WithCallbackData("Профиль", "profile"),
                        InlineKeyboardButton.WithCallbackData("Настройки", "settings")
                    }
                });
                
                await context.Bot.SendTextMessageAsync(
                    context.ChatId,
                    "Добро пожаловать! Выберите раздел:",
                    replyMarkup: keyboard,
                    cancellationToken: context.CancellationToken
                );
            }
        }
        
        [Action("HelpCommand", new[] { "CommandSelector" }, new[] { "main", "profile", "settings" }, 1, false)]
        private async Task HandleHelp(BotContext context)
        {
            if (context.Text == "/help")
            {
                await context.Bot.SendTextMessageAsync(
                    context.ChatId,
                    "Доступные команды:\n" +
                    "/start - Начать работу с ботом\n" +
                    "/profile - Перейти в профиль\n" +
                    "/settings - Открыть настройки\n" +
                    "/help - Показать справку\n" +
                    "/back - Вернуться в главное меню",
                    cancellationToken: context.CancellationToken
                );
            }
        }
        
        [Action("ProfileCommand", new[] { "CommandSelector" }, new[] { "main" }, 2, false)]
        private async Task HandleProfileCommand(BotContext context)
        {
            if (context.Text == "/profile")
            {
                await context.Bot.SendTextMessageAsync(
                    context.ChatId,
                    "Вы перешли в раздел профиля. Используйте /back для возврата.",
                    cancellationToken: context.CancellationToken
                );
                
                await ChangeState(context, "profile");
            }
        }
        
        [Action("SettingsCommand", new[] { "CommandSelector" }, new[] { "main" }, 3, false)]
        private async Task HandleSettingsCommand(BotContext context)
        {
            if (context.Text == "/settings")
            {
                await context.Bot.SendTextMessageAsync(
                    context.ChatId,
                    "Вы перешли в раздел настроек. Используйте /back для возврата.",
                    cancellationToken: context.CancellationToken
                );
                
                await ChangeState(context, "settings");
            }
        }
        
        [Action("MainCallback", new[] { "CallbackSelector" }, new[] { "main" }, 4, false)]
        private async Task HandleMainCallback(BotContext context)
        {
            var callbackQuery = context.Update.CallbackQuery;
            var callbackData = callbackQuery.Data;
            
            switch (callbackData)
            {
                case "profile":
                    await context.Bot.SendTextMessageAsync(
                        context.ChatId,
                        "Вы перешли в раздел профиля через меню. Используйте /back для возврата.",
                        cancellationToken: context.CancellationToken
                    );
                    
                    await ChangeState(context, "profile");
                    break;
                    
                case "settings":
                    await context.Bot.SendTextMessageAsync(
                        context.ChatId,
                        "Вы перешли в раздел настроек через меню. Используйте /back для возврата.",
                        cancellationToken: context.CancellationToken
                    );
                    
                    await ChangeState(context, "settings");
                    break;
            }
            
            // Подтверждаем обработку CallbackQuery
            await context.Bot.AnswerCallbackQueryAsync(
                callbackQuery.Id,
                cancellationToken: context.CancellationToken
            );
        }
        
        [Action("DefaultTextInMain", new[] { "TextSelector" }, new[] { "main" }, 5, false)]
        private async Task HandleDefaultTextInMain(BotContext context)
        {
            await context.Bot.SendTextMessageAsync(
                context.ChatId,
                $"Вы отправили: {context.Text}\nИспользуйте /help для списка команд.",
                cancellationToken: context.CancellationToken
            );
        }
        
        #endregion
        
        #region Действия для состояния профиля
        
        [Action("ProfileTextMessage", new[] { "TextSelector" }, new[] { "profile" }, 0, false)]
        private async Task HandleProfileText(BotContext context)
        {
            await context.Bot.SendTextMessageAsync(
                context.ChatId,
                $"Сообщение в профиле: {context.Text}",
                cancellationToken: context.CancellationToken
            );
        }
        
        #endregion
        
        #region Действия для состояния настроек
        
        [Action("SettingsTextMessage", new[] { "TextSelector" }, new[] { "settings", "preferences" }, 0, true)]
        private async Task HandleSettingsText(BotContext context)
        {
            await context.Bot.SendTextMessageAsync(
                context.ChatId,
                $"Настройка: {context.Text}",
                cancellationToken: context.CancellationToken
            );
        }
        
        // Действие, которое выполнится после предыдущего, т.к. activateWithoutInterruption = true
        [Action("SettingsHelp", new[] { "TextSelector" }, new[] { "settings", "preferences" }, 1, false)]
        private async Task HandleSettingsHelp(BotContext context)
        {
            await context.Bot.SendTextMessageAsync(
                context.ChatId,
                "Подсказка: в настройках вы можете указать свои предпочтения.",
                cancellationToken: context.CancellationToken
            );
        }
        
        #endregion
        
        // Вспомогательный метод для изменения состояния
        private async Task ChangeState(BotContext context, string newState)
        {
            var stateManager = new InMemoryStateManager();
            await stateManager.SetStateAsync(context.ChatId, newState);
        }
    }
}