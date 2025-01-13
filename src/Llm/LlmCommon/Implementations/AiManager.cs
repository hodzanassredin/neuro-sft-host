using LlmCommon.Abstractions;
using LlmCommon.Commands.Chat;
using LlmCommon.Dtos;
using LlmCommon.Entities;
using Microsoft.Extensions.AI;
using System.Diagnostics;

namespace LlmCommon.Implementations
{
    public class AiManager : IContext
    {
        private readonly IEntityStorage entStorage;
        private readonly IChatClient client;
        private readonly IExecutor executor;
        private readonly IMetrics metrics;
        private readonly IUnitOfWork unitOfWork;
        public static readonly User aiUser = new User(Ids.Parse("AI"), "AI", true);

        private const string model = "/models/toxic_sft_cotype/merged";

        //private ConcurrentDictionary<Ids.Id, CancellationTokenSource> inFly = new();
        public AiManager(IEntityStorage entStorage, IChatClient client, IExecutor executor, IMetrics metrics, IUnitOfWork unitOfWork)
        {
            this.entStorage = entStorage;
            this.client = client;
            this.executor = executor;
            this.metrics = metrics;
            this.unitOfWork = unitOfWork;
        }

        public async Task StartGeneration(Ids.Id chatId, string? system = null)
        {
            metrics.IncLlmRequestCount();
            var sw = Stopwatch.StartNew();

            var msgs = await MapMsgs(chatId, system);
            var opts = GetOps();
            
            //var resp = client.CompleteAsync(msgs, opts, cts.Token);
            var stream = client.CompleteStreamingAsync(msgs, opts);

            TimeSpan prevTokenTime = TimeSpan.MaxValue;
            Ids.Id? msgId = null; 
            await foreach (var item in stream)
            {
                if (msgId == null)//first token received
                {
                    prevTokenTime = sw.Elapsed;
                    metrics.SetTimeToFirstToken(prevTokenTime);
                    var cmd = new AddMessageCommand(chatId, "");
                    await cmd.Accept(executor, this);
                    await unitOfWork.StoreAsync();
                    msgId = cmd.AddedMessageId;
                }
                else {
                    metrics.SetTimeToInterTokenDelay(sw.Elapsed - prevTokenTime);
                    prevTokenTime = sw.Elapsed;
                    await new ChangeMessageCommand(chatId, msgId, item.Text ?? "", true).Accept(executor, this);
                    await unitOfWork.StoreAsync();
                }
                
            }

            metrics.SetTimeToWholeRequest(sw.Elapsed);
        }

        private async Task<List<ChatMessage>> MapMsgs(Ids.Id chatId, string? system)
        {
            var chat = (await entStorage.Load<ChatEntity>(chatId)).Dto;
            var msgs = chat.Messages.Select(x => new ChatMessage()
            {
                AuthorName = x.User.Name,
                Text = x.Text,
                Role = x.User.Name == aiUser.Name ? ChatRole.Assistant : ChatRole.User
            }).ToList();
            if (!String.IsNullOrWhiteSpace(system))
            {
                msgs.Add(new ChatMessage
                {
                    Role = ChatRole.System,
                    Text = system,
                });
            }
            return msgs;
        }

        private static ChatOptions GetOps()
        {
            return new ChatOptions
            {
                Temperature = 0.4f,
                MaxOutputTokens = 150,
                TopP = 0.8f,
                FrequencyPenalty = 0,
                PresencePenalty = 0,
                ResponseFormat = ChatResponseFormatText.Text,
                ModelId = model,
            };
        }

        public User GetCurrentUser()
        {
            return aiUser;
        }
    }
}
