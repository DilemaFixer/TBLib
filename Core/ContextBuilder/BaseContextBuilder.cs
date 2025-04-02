using Model;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace Core;

public class BaseContextBuilder : IContextBuilder
{
    public BotContext Build(ITelegramBotClient bot, Update update, CancellationToken cancellationToken)
    {
        BotContext botContext = new BotContext();
        botContext.Update = update;
        botContext.Bot = bot;
        botContext.CancellationToken = cancellationToken;
       
        if(update.Type == UpdateType.Message)
            botContext.Text = update.Message.Text;
        else if(update.Type == UpdateType.CallbackQuery)
            botContext.Text = update.CallbackQuery.Message.Text;
        
        return botContext;
    }
}