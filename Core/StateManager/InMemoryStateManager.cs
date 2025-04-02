namespace Core;

public class InMemoryStateManager : IStateManager
{
    public Dictionary<long , string> _dictionary = new();
    
    public Task<string> GetStateAsync(long chatId)
    {
        return Task.FromResult(_dictionary[chatId]);
    }

    public Task SetStateAsync(long chatId, string state)
    {
       _dictionary[chatId] = state;
       return Task.CompletedTask;
    }

    public Task ClearStateAsync(long chatId)
    {
       _dictionary.Remove(chatId);
       return Task.CompletedTask;
    }
}