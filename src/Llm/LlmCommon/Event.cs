using LlmCommon.Abstractions;

namespace LlmCommon
{
    public abstract class Event
    {
        public abstract Task Accept(IEventHandler visitor);
    }
}
