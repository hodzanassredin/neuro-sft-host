using LlmCommon.Abstractions;

namespace LlmCommon.Events
{
    public abstract class ChatEvent : Event
    {
        public Ids.Id ChatId { get; set; } = Ids.Empty;
        public abstract Task<bool> Accept(IChatsEventHandler visitor);
    }
}
