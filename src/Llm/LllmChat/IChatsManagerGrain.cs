using Orleans;

namespace LlmChat
{
    public interface IChatsManagerGrain : IGrainWithStringKey
    {
        Task<List<string>> GetChats();
        Task CreateChat(string chatId, string userName);
    }
}
