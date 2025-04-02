using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace Model;

public class BotContext
{
    public Update Update;
    public ITelegramBotClient Bot;
    public CancellationToken CancellationToken;
    public UpdateType UpdateType => Update.Type;
    public string Text;
    public long ChatId { get; set; }
}