
namespace LlmCommon.Abstractions
{
    public interface IEntityStorage<T> where T : Entity
    {
        Task<IEnumerable<T>> GetAll();//todo paging
        Task<T> Load(Ids.Id id);
        Task Upsert(T entity);
        Task Remove(Ids.Id id);
    }
}
