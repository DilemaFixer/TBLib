namespace Model;

public class ActionAttribute : Attribute
{
    public string Action { get; set; }
    public string[] Selector  { get; set; }
    public string[] States { get; set; }

    public ActionAttribute(string action, string[] states , params string[] selector)
    {
        if(string.IsNullOrWhiteSpace(action))
            throw new ArgumentException("Action or state is empty");
        
        ArgumentNullException.ThrowIfNull(selector);
        if(selector.Length == 0)
            throw new ArgumentException("Selector is empty");
        if(selector.Any(string.IsNullOrEmpty))
            throw new ArgumentException("Selector is empty");
        
        ArgumentNullException.ThrowIfNull(states);
        if(states.Length == 0)
            throw new ArgumentException("states is empty");
        if(states.Any(string.IsNullOrEmpty))
            throw new ArgumentException("states is empty");
        
        Action = action;
        States = states;
    }
}