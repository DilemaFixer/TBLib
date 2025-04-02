using System.Reflection;
using Core.States;
using Model;

namespace Core;

public static class Parser
{
    public static void ParseActionFromType<T>(T obj, List<Selector> selectors , States.States states)
        where T : class
    {
        var type = typeof(T);
        var methods = type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        
        foreach (var method in methods)
        {
            var attribute = method.GetCustomAttribute<ActionAttribute>();
            if (attribute == null)
                continue;

            var args = method.GetParameters();
            
            if (args.Length != 1)
                throw new Exception($"Method {method.Name} must have exactly one parameter type {typeof(BotContext)}");
            
            if(args[0].ParameterType != typeof(BotContext))
                throw new Exception($"Method {method.Name} must have exactly one parameter type {typeof(BotContext)}");
            
            var action = new Action(method, obj, attribute.Action);

            foreach (var selectorName in attribute.Selector)
            {
                var selector = selectors.FirstOrDefault(s => s.GetType().Name == selectorName);

                if (selector == null)
                    throw new InvalidOperationException(
                        $"Selector '{selectorName}' not found in the provided selectors list");

                action.AddSelector(selector);
            }

            foreach (string state in attribute.States)
            {
                if(!states.Contains(state))
                    throw new Exception($"State '{state}' not found in the provided states list");
                
                states[state].AddAction(action);
            }
        }
    }

    public static List<Selector> ParseSelectorsFromType<T>(T obj) where T : class
    {
        var type = typeof(T);
        var methods = type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        var selectors = new List<Selector>();

        foreach (var method in methods)
        {
            var attribute = method.GetCustomAttribute<SelectorAttribute>();
            if (attribute == null)
                continue;
            
            var args = method.GetParameters();
            
            if(args.Length == 0)
                throw new InvalidOperationException($"Selectors func must have at least one parameter . {method.Name}");
            
            if(args[0].ParameterType != typeof(BotContext))
                throw new InvalidOperationException($"Selectors func must have a parameter of type {typeof(BotContext).Name} in func {method.Name}");
            
            var selector = new Selector(method, obj , attribute.Selector);
            
            if(selectors.Any(s => s.SelectorName == selector.SelectorName))
                throw new Exception($"Selector {selector.SelectorName} is already registered");
            
            selectors.Add(selector);
        }

        return selectors;
    }

    public static void ParserStateFromType<T>(T obj, Dictionary<string, State> states) where T : class
    {
        var type = typeof(T);
        var methods = type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

        foreach (var method in methods)
        {
            var attribute = method.GetCustomAttribute<StateAttribute>();
            if (attribute == null)
                continue;

            var args = method.GetParameters();

            if (args.Length != 2)
                throw new InvalidOperationException($"Method '{method.Name}' has an invalid number of arguments");

            if (args[0].ParameterType != typeof(BotContext) && args[1].ParameterType != typeof(Actions))
                throw new InvalidOperationException(
                    $"Method '{method.Name}' has an invalid type of arguments . Must bee BotContext , Actions");

            foreach (var stateName in attribute.States)
            {
                var state = new State(stateName, method, obj);
                
                if(states.ContainsKey(stateName))
                    throw new Exception($"State {stateName} is already registered");
                
                states[stateName] = state;
            }
        }
    }
}