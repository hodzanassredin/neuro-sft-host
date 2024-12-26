
namespace LlmCommon.Abstractions
{
    public interface IEventBus
    {
        async Task PublishEventsFrom(Entity ent) {
            foreach (var ev in ent.DequeueUncommittedEvents())
            {
                await Publish(ev);
            }
        }
        Task Publish(Event @event);
        void Subscribe(IEventHandler handler);
    }
}
