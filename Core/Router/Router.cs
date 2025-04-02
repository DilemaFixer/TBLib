using Core.States;
using Model;

namespace Core;

public class Router
{
    private States.States _states;
    private List<Selector> _selectors;

    public Router(IStateManager stateManager, string baseState)
    {
        ArgumentNullException.ThrowIfNull(stateManager);
        if(string.IsNullOrEmpty(baseState))
            throw new ArgumentNullException(nameof(baseState));
        
        _states = new States.States(stateManager, baseState);
        _selectors = new();
    }

    public async Task Handle(BotContext context) => 
        await _states.Handle(context);

    public void ParseStates(object[] objs) 
    {
        ArgumentNullException.ThrowIfNull(objs);
        if(objs.Length == 0)
            throw new ArgumentNullException(nameof(objs));
        
        if(objs.Any(o => o == null))
            throw new ArgumentNullException(nameof(objs));
        
        foreach (object o in objs) 
            ParseStates(o);
    }
    
    public void ParseStates<T>(T obj) where T : class
    {
        ArgumentNullException.ThrowIfNull(obj);
        _states.ParseStates(obj);
    }

    public void ParseSelectors(object[] objs)
    {
        ArgumentNullException.ThrowIfNull(objs);
        if(objs.Length == 0)
            throw new ArgumentNullException(nameof(objs));
        
        if(objs.Any(o => o == null))
            throw new ArgumentNullException(nameof(objs));

        foreach (object o in objs) 
            ParseSelectors(o);
    }

    public void ParseSelectors<T>(T obj) where T : class
    {
        ArgumentNullException.ThrowIfNull(obj);
        var selectors = Parser.ParseSelectorsFromType(obj);
        
        if(selectors is null || selectors.Count == 0)
            throw new ArgumentException("Selectors are empty.");
        
        foreach (Selector selector in selectors)
        {
            if (_selectors.Any(s => s.SelectorName == selector.SelectorName)) 
                throw new ArgumentException($"Selector {selector.SelectorName} already exists.");
        }
        
        _selectors.AddRange(selectors);
    }

    public void ParseActions(object[] objs)
    {
        ArgumentNullException.ThrowIfNull(objs);
        if(objs.Length == 0)
            throw new ArgumentNullException(nameof(objs));
        
        if(objs.Any(o => o == null))
            throw new ArgumentNullException(nameof(objs));

        foreach (object o in objs)  
            ParseActions(o);
    }

    public void ParseActions<T>(T obj) where T : class
    {
        ArgumentNullException.ThrowIfNull(obj);
        Parser.ParseActionFromType(obj , _selectors , _states);
    }
}