using LlmCommon.Abstractions;

namespace LlmCommon.Events
{
    public class RemovedMessageEvent : Event
    {
        public Ids.Id ChatId { get; set; } = Ids.Empty;

        public RemovedMessageEvent(Ids.Id chatId, Ids.Id messageId)
        {
            ChatId = chatId;
            MessageId = messageId;
        }

        public Ids.Id MessageId { get; set; } = Ids.Empty;

        public override Task Accept(IEventHandler visitor)
        {
            return visitor.Visit(this);
        }
    }
}
