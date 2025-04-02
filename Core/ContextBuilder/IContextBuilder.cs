using Model;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace Core;

public interface IContextBuilder
{
    public BotContext Build(ITelegramBotClient bot, Update update, CancellationToken cancellationToken);
}