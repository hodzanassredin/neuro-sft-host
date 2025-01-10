using LlmCommon.Abstractions;

namespace LlmCommon.Implementations
{
    public class BroadcastStorage : IEntityStorage
    {
        private readonly IEntityStorage storage;
        private readonly IEventBus eventBus;

        public BroadcastStorage(IEntityStorage storage, IEventBus eventBus)
        {
            this.storage = storage;
            this.eventBus = eventBus;
        }

        public Task<T?> Load<T>(Ids.Id id) where T : Entity 
        {
            return storage.Load<T>(id);
        }

        public async Task Remove(Entity entity)
        {
            await storage.Remove(entity);
            await eventBus.PublishEventsFrom(entity);
        }

        public async Task Upsert(Entity entity)
        {
            await storage.Upsert(entity);
            await eventBus.PublishEventsFrom(entity);
        }
    }
}
