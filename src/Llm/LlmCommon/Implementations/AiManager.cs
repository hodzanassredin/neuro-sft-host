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

        

        //private ConcurrentDictionary<Ids.Id, CancellationTokenSource> inFly = new();
        public AiManager(IEntityStorage entStorage, IChatClient client, IExecutor executor, IMetrics metrics, IUnitOfWork unitOfWork)
        {
            this.entStorage = entStorage;
            this.client = client;
            this.executor = executor;
            this.metrics = metrics;
            this.unitOfWork = unitOfWork;
        }

        public async Task StartGeneration(Ids.Id chatId, Ids.Id? regenerateMsgId = null, string? system = null)
        {
            metrics.IncLlmRequestCount();
            var sw = Stopwatch.StartNew();

            var chat = (await entStorage.Load<ChatEntity>(chatId)).Dto;

            var msgs = await MapMsgs(chat, regenerateMsgId, system ?? chat.AiSettings.System);
            var opts = GetOps(chat.AiSettings);
            //var resp = client.CompleteAsync(msgs, opts, cts.Token);
            var stream = client.CompleteStreamingAsync(msgs, opts);

            TimeSpan prevTokenTime = TimeSpan.MaxValue;

            await foreach (var item in stream)
            {
                if (prevTokenTime == TimeSpan.MaxValue)//first token received
                {
                    prevTokenTime = sw.Elapsed;
                    metrics.SetTimeToFirstToken(prevTokenTime);
                    if (regenerateMsgId == null)
                    {
                        var cmd = new AddMessageCommand(chatId, item.Text?? "");
                        await cmd.Accept(executor, this);
                        regenerateMsgId = cmd.AddedMessageId;
                    }
                    else {
                        await new ChangeMessageCommand(chatId, regenerateMsgId!, item.Text ?? "", true).Accept(executor, this);
                    }
                }
                else {
                    metrics.SetTimeToInterTokenDelay(sw.Elapsed - prevTokenTime);
                    prevTokenTime = sw.Elapsed;
                    await new ChangeMessageCommand(chatId, regenerateMsgId!, item.Text ?? "", true).Accept(executor, this);
                }
                await unitOfWork.StoreAsync();
            }

            metrics.SetTimeToWholeRequest(sw.Elapsed);
        }

        private async Task<List<ChatMessage>> MapMsgs(ChatDto chat, Ids.Id? regenerateMsgId, string? system)
        {
            IEnumerable<MessageDto> msgsRaw = chat.Messages.OrderBy(x=>x.CreatedAt);
            if (regenerateMsgId != null) {
                msgsRaw = msgsRaw.TakeWhile(x => x.Id != regenerateMsgId);
            }
            var msgs = msgsRaw.Select(x => new ChatMessage()
            {
                AuthorName = x.User.Name,
                Text = x.Text,
                Role = x.User.Name == aiUser.Name ? ChatRole.Assistant : ChatRole.User
            }).ToList();
            if (!String.IsNullOrWhiteSpace(system))
            {
                msgs.Insert(0, new ChatMessage
                {
                    Role = ChatRole.System,
                    Text = system,
                });
            }
            return msgs;
        }

        private static ChatOptions GetOps(AiSettingsDto settings)
        {
            return new ChatOptions
            {
                Temperature = settings.Temperature,
                MaxOutputTokens = settings.MaxOutputTokens,
                TopP = settings.TopP,
                FrequencyPenalty = settings.FrequencyPenalty,
                PresencePenalty = settings.PresencePenalty,
                ResponseFormat = ChatResponseFormatText.Text,
                ModelId = settings.Model,
                Seed = settings.Seed,
            };
        }

        public User GetCurrentUser()
        {
            return aiUser;
        }
    }
}
