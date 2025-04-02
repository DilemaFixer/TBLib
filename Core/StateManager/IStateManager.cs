namespace Core;

public interface IStateManager
{
    Task<string> GetStateAsync(long chatId);
    Task SetStateAsync(long chatId, string state);
    Task ClearStateAsync(long chatId);
}