using LlmCommon.Implementations;
using Microsoft.Extensions.DependencyInjection;

namespace LlmCommon.Tasks
{
    public class GenerateResponseTask : BaseLongRunningTask
    {
        private readonly Ids.Id chatId;
        private readonly Ids.Id? messageId;

        public GenerateResponseTask(Ids.Id chatId, Ids.Id? messageId = null)
        {
            this.chatId = chatId;
            this.messageId = messageId;
        }
        protected override async Task Run(IServiceProvider sp, CancellationToken ct)
        {
            var aiManager = sp.GetRequiredService<AiManager>();
            await aiManager.StartGeneration(chatId, messageId);
        }
    }
}
