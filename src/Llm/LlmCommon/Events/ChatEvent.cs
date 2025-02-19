using LlmCommon.Abstractions;

namespace LlmCommon.Events
{
    public abstract class ChatEvent : Event
    {
        protected ChatEvent(Ids.Id chatId)
        {
            ChatId = chatId;
        }
        public override bool IsValid()
        {
            return ChatId != Ids.Empty && ChatId != null;
        }
        public Ids.Id ChatId { get; private set; } = Ids.Empty;
        public abstract Task<bool> Accept(IChatsEventHandler visitor);
    }
}
