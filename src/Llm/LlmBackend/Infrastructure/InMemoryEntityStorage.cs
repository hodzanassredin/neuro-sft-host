using LlmCommon;
using LlmCommon.Abstractions;

namespace LlmBackend.Infrastructure
{
    public class InMemoryEntityStorage : IEntityStorage
    {
        public InMemoryEntityStorage()
        {
        }
        private static readonly IList<Entity> _items = new List<Entity>();


        public Task<T?> Load<T>(Ids.Id id) where T : Entity
        {
            T? res = null;
            var e = _items.SingleOrDefault(x => x.Id == id);
            if (e != null)
            {
                res = (T)e;
            }
            return Task.FromResult(res);
        }

        public Task Remove(Entity entity)
        {
            var item = _items.Single(x => x.Id == entity.Id);
            _items.Remove(item);
            return Task.CompletedTask;
        }

        public async Task Upsert(Entity entity)
        {
            var ex = _items.SingleOrDefault(x => x.Id == entity.Id);
            if (ex!= null)
            {
                _items.Remove(ex);

            }
            _items.Add(entity);

        }
    }
}
