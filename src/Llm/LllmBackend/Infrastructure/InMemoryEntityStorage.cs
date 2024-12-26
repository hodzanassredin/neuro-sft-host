using LlmCommon;
using LlmCommon.Abstractions;

namespace LlmBackend.Infrastructure
{
    public class InMemoryEntityStorage<T> : IEntityStorage<T> where T : Entity
    {
        private readonly IList<T> _items = new List<T>();

        public Task<IEnumerable<T>> GetAll()
        {
            return Task.FromResult(_items.AsEnumerable());
        }

        public Task<T> Load(Ids.Id id)
        {
            return Task.FromResult(_items.Single(x=>x.Id == id));
        }

        public Task Remove(Ids.Id id)
        {
            var item = _items.Single(x => x.Id == id);
            _items.Remove(item);
            return Task.CompletedTask;
        }

        public async Task Upsert(T entity)
        {
            if (entity.Id != Ids.Empty)
            {
                await Remove(entity.Id);
                
            }
            _items.Add(entity);
        }
    }
}
