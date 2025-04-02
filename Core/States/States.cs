using Model;

namespace Core.States;

public class States
{
    public const string BASE_STATE = "main";
    private Dictionary<string, State> _states = new();
    private IStateManager _stateManager;
    private string _baseState;

    public States(IStateManager stateManager , string baseState)
    {
        ArgumentNullException.ThrowIfNull(baseState);
        if(string.IsNullOrEmpty(baseState))
            throw new ArgumentNullException(nameof(baseState));
        
        _stateManager = stateManager;
        _baseState = baseState;
    }

    public State this[string key] => _states[key];

    public States Add(State state)
    {
        _states.Add(state.Name, state);
        return this;
    }

    public States Remove(State state)
    {
        _states.Remove(state.Name);
        return this;
    }
    
    public bool Contains(string key) => _states.ContainsKey(key);
    
    public async Task Handle(BotContext context)
    {
        string state = await _stateManager.GetStateAsync(context.ChatId);

        if (string.IsNullOrEmpty(state))
        {
            if(!_states.ContainsKey(_baseState))
                throw new ArgumentException($"Base state not found: {_baseState}");
            
            await _stateManager.SetStateAsync(context.ChatId, _baseState);
            state = _baseState;
        }
        
        if(!_states.ContainsKey(state))
            throw new ArgumentException($"State not found: {state}");
        
        await _states[state].Handle(context);
    }
    
    public void ParseStates<T>(T obj) where T : class => 
        Parser.ParserStateFromType(obj , _states);
}