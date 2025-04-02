using Core;
using Core.Middleware;
using Core.States;
using Telegram.Bot;

const string TOKEN = "";

ITelegramBotClient tgClient = new TelegramBotClient(TOKEN);
Middlewares middlewares = new Middlewares();

IStateManager stateManager = new InMemoryStateManager();
Router router = new Router(stateManager, States.BASE_STATE);
router.ParseStates(router); 

middlewares.Add(new RouteMiddleware(router));

IContextBuilder contextBuilder = new BaseContextBuilder();
IErrorHandler errorHandler = new BaseErrorHandler();

Bot bot = new Bot(tgClient, middlewares, contextBuilder, errorHandler);

bot.Start();