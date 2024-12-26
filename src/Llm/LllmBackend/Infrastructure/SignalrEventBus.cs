using LlmCommon;
using LlmCommon.Abstractions;

namespace LlmBackend.Infrastructure
{
    public class SignalrEventBus : IEventBus
    {
        public Task Publish(Event @event)
        {
            throw new NotImplementedException();
        }

        public void Subscribe(IEventHandler handler)
        {
            throw new NotImplementedException();
        }
    }
}
