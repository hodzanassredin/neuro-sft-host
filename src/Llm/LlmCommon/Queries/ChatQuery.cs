using LlmCommon.Abstractions;
using LlmCommon.Views;
using System.Diagnostics;

namespace LlmCommon.Queries
{
    public class ChatQuery : TypedQuery<ChatView>
    {
        public ChatQuery(Ids.Id chatId)
        {
            Debug.Assert(chatId != null && chatId != Ids.Empty);
            ChatId = chatId;
        }

        public Ids.Id ChatId { get; }

        public override async Task<View> Accept(ViewStorage visitor)
        {
            return await visitor.Get(this);
        }
    }
}
