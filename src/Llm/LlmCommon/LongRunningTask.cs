
using Microsoft.Extensions.DependencyInjection;

namespace LlmCommon
{
    public enum LongRunningTaskState { 
        Created,
        Waiting,
        Started,
        Finished,
        Cancelled,
        Error
    }

    public abstract class LongRunningTask : IDisposable
    {
        public abstract Task Start(IServiceProvider sp);
        public abstract Task Cancel();
        public abstract void Dispose();
        public abstract LongRunningTaskState State { get; }
        public abstract Exception? Error();
    }

    public abstract class BaseLongRunningTask : LongRunningTask
    {
        private LongRunningTaskState state = LongRunningTaskState.Created;
        private Exception? ex;
        public override LongRunningTaskState State => state;

        private CancellationTokenSource _cts;
        public override Task Cancel()
        {
            if (state == LongRunningTaskState.Started)
            {
                _cts.Cancel();
                state = LongRunningTaskState.Cancelled;
            }
            return Task.CompletedTask;
        }

        public override void Dispose()
        {
            _cts.Dispose();
        }
        protected abstract Task Run(IServiceProvider sp, CancellationToken ct);
        public override async Task Start(IServiceProvider sp)
        {
            _cts = new CancellationTokenSource();
            state = LongRunningTaskState.Started;
            try
            {
                using (var scope = sp.CreateScope())
                {
                    await Run(scope.ServiceProvider, _cts.Token).ConfigureAwait(false);
                }
                state = LongRunningTaskState.Finished;
            }
            catch (Exception ex)
            {
                this.ex = ex;
                state = LongRunningTaskState.Error;
            }
        }

        public override Exception? Error()
        {
            return ex;
        }
    }
}
