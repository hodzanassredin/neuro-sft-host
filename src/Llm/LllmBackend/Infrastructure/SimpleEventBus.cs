using LlmCommon;
using LlmCommon.Abstractions;

namespace LlmBackend.Infrastructure
{
    public class SimpleEventBus : IEventBus
    {
        IEventHandler handler;
        public Task Publish(Event @event)
        {
            return @event.Accept(handler);
        }

        public void Subscribe(IEventHandler handler)
        {
            this.handler = handler;
        }
    }
}
