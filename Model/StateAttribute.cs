namespace Model;

public class StateAttribute : Attribute
{
    public string[] States { get; set; }

    public StateAttribute(string[] states)
    {
        ArgumentNullException.ThrowIfNull(states);
        
        if (states.Length == 0)
            throw new ArgumentException("States must contain at least one state");
        
        if(states.Any(string.IsNullOrEmpty))
            throw new ArgumentException("States must contain at least one state");
        
        States = states;
    }
}