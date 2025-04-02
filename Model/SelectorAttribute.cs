namespace Model;

public class SelectorAttribute : Attribute
{
    public string Selector { get; }

    public SelectorAttribute(string selector)
    {
       if(string.IsNullOrEmpty(selector))
           throw new ArgumentNullException(nameof(selector));
        
       Selector = selector;
    }
}