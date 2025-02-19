using LlmCommon.Abstractions;
using System.Collections.Concurrent;
using System.Diagnostics;

namespace LlmCommon.Implementations
{
    public class SimpleEventBus : IEventBus
    {
        ConcurrentBag<IEventHandler> handlers = [];
        public Task Publish(Event @event)
        {
            Debug.Assert(@event != null && @event.IsValid());
            var tasks = handlers.Select(h => h.Handle(@event)).ToList();
            return Task.WhenAll(tasks);
        }

        public void Subscribe(IEventHandler handler)
        {
            if (handlers.Contains(handler)) { throw new Exception("Cant subscribe two times"); }
            handlers.Add(handler);
        }

        public void UnSubscribe(IEventHandler handler)
        {
            var hs = handlers.ToList();
            hs.Remove(handler);
            handlers = new ConcurrentBag<IEventHandler>(handlers.Where(x => !ReferenceEquals(x, handler)));
        }
    }
}
