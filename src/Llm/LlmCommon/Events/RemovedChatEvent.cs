
using LlmCommon.Abstractions;

namespace LlmCommon.Events
{
    public class RemovedChatEvent : Event
    {
        public Ids.Id ChatId { get; set; }

        public RemovedChatEvent(Ids.Id chatId)
        {
            ChatId = chatId;
        }
        public override Task Accept(IEventHandler visitor)
        {
            return visitor.Visit(this);
        }
    }
}
