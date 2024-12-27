
using LlmCommon.Abstractions;

namespace LlmCommon.Events
{
    public class RemovedChatEvent : ChatEvent
    {
        public Ids.Id ChatId { get; set; }

        public RemovedChatEvent(Ids.Id chatId)
        {
            ChatId = chatId;
        }
        public override Task<bool> Accept(IChatsEventHandler visitor)
        {
            return visitor.Visit(this);
        }
    }
}
