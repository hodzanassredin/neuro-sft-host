using LlmCommon;
using LlmCommon.Abstractions;
using System.Threading.Channels;

namespace LlmBackend.Infrastructure
{
    public sealed class DefaultBackgroundTaskQueue : ITaskQueue
    {
        private readonly Channel<LongRunningTask> _queue;

        public DefaultBackgroundTaskQueue(int capacity)
        {
            BoundedChannelOptions options = new(capacity)
            {
                FullMode = BoundedChannelFullMode.Wait
            };
            _queue = Channel.CreateBounded<LongRunningTask>(options);
        }

        public async ValueTask QueueBackgroundWorkItemAsync(
            LongRunningTask workItem)
        {
            ArgumentNullException.ThrowIfNull(workItem);

            await _queue.Writer.WriteAsync(workItem);
        }

        public async ValueTask<LongRunningTask?> DequeueAsync(
            CancellationToken cancellationToken)
        {
            LongRunningTask? workItem;
            _queue.Reader.TryRead(out workItem);
            return workItem;
        }
    }
}
