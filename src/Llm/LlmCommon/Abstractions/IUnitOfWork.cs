namespace LlmCommon.Abstractions
{
    public interface IUnitOfWork
    {
        Task<int> StoreAsync(CancellationToken cancellationToken = default);
    }
}
