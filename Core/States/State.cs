using System.Reflection;
using Model;

namespace Core.States;

public class State
{
    private MethodInfo _method;
    private Actions _actions;
    private object _stateInstance;
    public string Name { get; private set; }

    public State(string name, MethodInfo method , object stateInstance)
    {
        ArgumentNullException.ThrowIfNull(method);
        ArgumentNullException.ThrowIfNull(stateInstance);
        if(string.IsNullOrEmpty(name))
            throw new ArgumentException("Name cannot be null or empty");
        
        Name = name;
        _method = method;
        _stateInstance = stateInstance;
        _actions = new Actions();
    }

    public State AddAction(Action action)
    {
        _actions.AddAction(action);
        return this;
    }

    public async Task Handle(BotContext context)
    {
        if (_method != null && _stateInstance != null)
        {
            var parameters = new object[] { context, _actions };
            var result = _method.Invoke(_stateInstance, parameters);

            if (result is Task task)
                await task;
            else if (result is ValueTask valueTask)
                await valueTask;
        }
    }

    public State RemoveAction(Action action)
    {
        _actions.RemoveAction(action);
        return this;
    }

    public Action? FindAction(string name) => 
        _actions.FindAction(a => a.Name == name);
}