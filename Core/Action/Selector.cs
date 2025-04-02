using System.Reflection;
using Model;

namespace Core;

public class Selector
{
    private MethodInfo _selector;
    private object _target;
    public string SelectorName { get; private set; }

    public Selector(MethodInfo selector, object target, string selectorName)
    {
        ArgumentNullException.ThrowIfNull(selector);
        ArgumentNullException.ThrowIfNull(target);
        if (string.IsNullOrEmpty(selectorName))
            throw new ArgumentException($"'{nameof(selectorName)}' cannot be null or empty.", nameof(selectorName));
        SelectorName = selectorName;
        _selector = selector;
        _target = target;
    }

    public bool IsTarget(BotContext context)
    {
        var parameters = new object[] { context };
        var result = _selector.Invoke(_target, parameters);

        if (result is bool boolResult)
            return boolResult;
        else if (result is Task<bool> taskBoolResult)
            return taskBoolResult.GetAwaiter().GetResult();

        return false;
    }
}