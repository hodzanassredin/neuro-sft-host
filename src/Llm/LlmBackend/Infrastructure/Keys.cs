using LlmCommon;

namespace LlmBackend.Infrastructure
{
    static class Keys
    {
        public const string ALL_CHATS_KEY = "all_chats";
        public static string ChatKey(Ids.Id id) => $"chat_{id}";
    }
}
