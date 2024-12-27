using LlmCommon.Abstractions;

namespace LlmCommon.Events
{
    public abstract class ChatEvent : Event
    {
        public abstract Task<bool> Accept(IChatsEventHandler visitor);

    }
}
