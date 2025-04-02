using System.Reflection;
using Model;

namespace Core;

public class Action
{
    private MethodInfo _method;
    private List<Selector> _selectors;
    private object _obj;
    public string Name { get; private set; }
    public uint Order { get; private set; }
    public bool ActivateWithoutInterruption { get; private set; }

    public Action(MethodInfo method, object obj, string? name , uint order ,
        bool activateWithoutInterruption)
    {
        ArgumentNullException.ThrowIfNull(method);
        ArgumentNullException.ThrowIfNull(obj);
        if(string.IsNullOrEmpty(name))
            throw new ArgumentException("Name cannot be null or empty");
        
        _method = method;
        _obj = obj;
        Name = name;
        Order = order;
        ActivateWithoutInterruption = activateWithoutInterruption;
        _selectors = new List<Selector>();
    }

    public bool IsTarget(BotContext context)
    {
        if (_selectors.Count == 0)
            throw new InvalidOperationException("No selectors have been registered");

        return _selectors.Any(s => s.IsTarget(context));
    }

    public async Task ExecuteAsync(BotContext context)
    {
        var parameters = new object[] { context };

        var result = _method.Invoke(_obj, parameters);

        if (result is Task task)
            await task;
        else if (result is ValueTask valueTask)
            await valueTask;
    }
    
    public void AddSelector(Selector selector) => _selectors.Add(selector);
    public void RemoveSelector(Selector selector) => _selectors.Remove(selector);
}