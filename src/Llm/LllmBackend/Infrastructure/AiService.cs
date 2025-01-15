
using LlmCommon;
using LlmCommon.Abstractions;
using System.Diagnostics;

namespace LlmBackend.Infrastructure
{
    public class AiService : BackgroundService
    {
        private readonly ITaskQueue taskQueue;
        private readonly IServiceProvider sp;
        private readonly ILogger<AiService> logger;
        private readonly List<LongRunningTask> startedTasks = [];
        public AiService(ITaskQueue taskQueue, IServiceProvider sp, ILogger<AiService> logger)
        {
            this.taskQueue = taskQueue;
            this.sp = sp;
            this.logger = logger;
        }
        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            logger.LogInformation("Running.");

            return ProcessTaskQueueAsync(stoppingToken);
        }

        private async Task CancelAll() {
            foreach (var task in startedTasks) { 
                await task.Cancel();
            }
        }

        const int MAX_PARALLEL_TASKS = 100;

        private void CleanaupTasks() {
            for (int i = startedTasks.Count; i > 0; i--) {
                var workItem = startedTasks[i-1];
                if (workItem.State != LongRunningTaskState.Started || workItem.State != LongRunningTaskState.Waiting) {
                    startedTasks.Remove(workItem);
                    if (workItem.State == LongRunningTaskState.Error)
                    {
                        logger.LogError(workItem.Error(), "Exception occured during task execution");
                    }
                    else if (workItem.State == LongRunningTaskState.Finished)
                    {
                        logger.LogDebug("Task finished");
                    }

                }
            }
        }
        private async void RunTask(LongRunningTask workItem, CancellationToken stoppingToken)
        {
            
            logger.LogInformation($"starting {workItem.GetType().Name}.");
            _ = workItem.Start(sp);
            Debug.Assert(workItem.State != LongRunningTaskState.Created);
            if (workItem.State == LongRunningTaskState.Started || workItem.State == LongRunningTaskState.Waiting)
            {
                startedTasks.Add(workItem);
            }
            else if (workItem.State == LongRunningTaskState.Error)
            {
                logger.LogError(workItem.Error(), "Exception occured during task start");
            }
            else if (workItem.State == LongRunningTaskState.Finished)
            {
                logger.LogDebug("Task finished");
            }
        }

        private async Task WaitTaskSlot(CancellationToken stoppingToken)
        {
            CleanaupTasks();
            while (!stoppingToken.IsCancellationRequested && startedTasks.Count > MAX_PARALLEL_TASKS)
            {
                await Task.Delay(100, stoppingToken);
                CleanaupTasks();
            }
        }

        private async Task ProcessTaskQueueAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await WaitTaskSlot(stoppingToken);
                    var workItem = await taskQueue.DequeueAsync(stoppingToken);
                    if (workItem != null)
                    {
                        RunTask(workItem, stoppingToken);
                    }
                    else {
                        await Task.Delay(100, stoppingToken);
                    }
                }
                catch (OperationCanceledException)
                {
                    // Prevent throwing if stoppingToken was signaled
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error occurred executing task work item.");
                }
            }
            await CancelAll();
        }

        public override async Task StopAsync(CancellationToken stoppingToken)
        {
            logger.LogInformation(
                $"{nameof(AiService)} is stopping.");

            await base.StopAsync(stoppingToken);
        }
    }
}
