using Model;
using Telegram.Bot.Types;

namespace Core;

public class Actions
{
    private List<Action> _actions = new();

    public void AddAction(Action action)
    {
        ArgumentNullException.ThrowIfNull(action);
        _actions.Add(action);
    }

    public void RemoveAction(Action action)
    {
        ArgumentNullException.ThrowIfNull(action);
        _actions.Remove(action);
    }

    public async Task Handle(BotContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        var orderedActions = _actions.OrderBy(action => action.Order).ToList();

        foreach (var action in orderedActions)
        {
            if (action.IsTarget(context))
            {
                await action.ExecuteAsync(context);

                if (!action.ActivateWithoutInterruption)
                    break;
            }
        }
    }

    public Action? FindAction(Predicate<Action> predicate) =>
        _actions.FirstOrDefault(action => predicate(action));
}