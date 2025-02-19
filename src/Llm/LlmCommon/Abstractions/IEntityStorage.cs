

namespace LlmCommon.Abstractions
{
    public interface IEntityStorage
    {
        Task<T?> Load<T>(Ids.Id id) where T : Entity;
        Task Upsert(Entity entity);
        Task Remove(Entity entity);
    }
}
