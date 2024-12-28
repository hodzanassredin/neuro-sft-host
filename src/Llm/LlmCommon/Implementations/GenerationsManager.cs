using LlmCommon.Abstractions;
using LlmCommon.Commands.Chat;
using LlmCommon.Dtos;
using LlmCommon.Entities;
using Microsoft.Extensions.AI;

namespace LlmCommon.Implementations
{
    public class GenerationsManager : IContext
    {
        private readonly IEntityStorage<ChatEntity> chats;
        private readonly IChatClient client;
        private readonly IEventBus eventBus;
        private readonly IExecutor handler;
        private readonly Executor executor;
        private User aiUser = new User(Ids.Parse("AI"), "AI");

        private const string model = "/models/toxic_sft_cotype/merged";

        //private ConcurrentDictionary<Ids.Id, CancellationTokenSource> inFly = new();
        public GenerationsManager(IEntityStorage<ChatEntity> chats, IChatClient client, IEventBus eventBus, IExecutor handler)
        {
            this.chats = chats;
            this.client = client;
            this.eventBus = eventBus;
            this.handler = handler;
            this.executor = new Executor(chats, eventBus);
        }

        public async Task StartGeneration(Ids.Id chatId, string? system = null)
        {
            var msgs = await MapMsgs(chatId, system);
            var opts = GetOps();
            await new AddMessageCommand(chatId, "Thinking...").Accept(executor, this);
            var msgId = executor.LastAddedMessageId;//todo remove that crap
            //var resp = client.CompleteAsync(msgs, opts, cts.Token);
            var stream = client.CompleteStreamingAsync(msgs, opts);
            await foreach (var item in stream)
            {
                await new ChangeMessageCommand(chatId, msgId, item.Text??"").Accept(executor,this);
            }
        }

        private async Task<List<ChatMessage>> MapMsgs(Ids.Id chatId, string? system)
        {
            var chat = (await chats.Load(chatId)).Dto;
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
