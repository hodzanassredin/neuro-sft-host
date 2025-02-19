
using LlmCommon.Abstractions;

namespace LlmCommon.Events
{
    public class RemovedChatEvent : ChatEvent
    {

        public RemovedChatEvent(Ids.Id chatId) : base(chatId)
        {
        }
        public override Task<bool> Accept(IChatsEventHandler visitor)
        {
            return visitor.Visit(this);
        }
    }
}
