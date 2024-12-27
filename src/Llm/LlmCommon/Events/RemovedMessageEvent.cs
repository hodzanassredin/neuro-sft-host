using LlmCommon.Abstractions;

namespace LlmCommon.Events
{
    public class RemovedMessageEvent : ChatEvent
    {
        public Ids.Id ChatId { get; set; } = Ids.Empty;

        public RemovedMessageEvent(Ids.Id chatId, Ids.Id messageId)
        {
            ChatId = chatId;
            MessageId = messageId;
        }

        public Ids.Id MessageId { get; set; } = Ids.Empty;

        public override Task<bool> Accept(IChatsEventHandler visitor)
        {
            return visitor.Visit(this);
        }
    }
}
