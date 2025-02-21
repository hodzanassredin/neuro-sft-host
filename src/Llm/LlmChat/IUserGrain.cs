
using Orleans;

namespace LlmChat
{
    public interface IUserGrain : IGrainWithStringKey
    {
        Task<string[]> GetSubscriptions();
        Task AddSubscription(string chatId);
    }
}
