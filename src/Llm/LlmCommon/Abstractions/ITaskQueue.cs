
namespace LlmCommon.Abstractions
{
    public interface ITaskQueue
    {
        ValueTask QueueBackgroundWorkItemAsync(
            LongRunningTask workItem);

        ValueTask<LongRunningTask> DequeueAsync(
            CancellationToken cancellationToken);
    }
}
