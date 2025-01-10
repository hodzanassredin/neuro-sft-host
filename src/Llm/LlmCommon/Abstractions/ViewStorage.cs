

using LlmCommon.Queries;
using LlmCommon.Views;

namespace LlmCommon.Abstractions
{
    public abstract class ViewStorage
    {
        public abstract Task<AllChatsView> Get(AllChatsQuery q);
        public abstract Task<ChatView?> Get(ChatQuery q);
        public abstract Task Save(View view);
    }
}
