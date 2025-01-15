using LlmCommon;
using LlmCommon.Abstractions;
using LlmCommon.Dtos;
using LlmCommon.Events;
using LlmCommon.Queries;
using LlmCommon.Views;

namespace LlmFrontend.Infrastructure
{
    public class AppState : IDisposable, IEventHandler
    {
        public class AppStateChangedEvent : Event
        {
            public static AppStateChangedEvent Instance = new AppStateChangedEvent();

            public override bool IsValid()
            {
                return true;
            }
        }

        public AppState(ILogger<AppState> logger,  IEventBus bus, IRequestHandler handler)
        {
            this.logger = logger;
            this.bus = bus;
            this.handler = handler;
            bus.Subscribe(this);
        }
        private AllChatsView? chats;
        private List<ChatView> loadedChats = [];

        public AllChatsView ChatsView => chats;

        

        private readonly ILogger<AppState> logger;
        private readonly IEventBus bus;
        private readonly IRequestHandler handler;

        public async Task LoadAsync() {
            chats = await handler.HandleQuery(AllChatsQuery.Instance) as AllChatsView;
            await bus.Publish(AppStateChangedEvent.Instance);
        }


        public async Task<ChatDto?> GetChat(Ids.Id id) {
            var chat = loadedChats.SingleOrDefault(x => x.Chat.Id == id);
            if (chat == null) {
                chat = await handler.HandleQuery(new ChatQuery(id)) as ChatView;
                loadedChats.Add(chat);
            }

            return chat?.Chat;
        }

        public void Dispose()
        {
            bus?.UnSubscribe(this);
        }

        public async Task<bool> Handle(Event ev)
        {
            if (ev is ChatEvent cev)
            {
                

                var accepted = await cev.Accept(chats);
                if (ev is RemovedChatEvent rce)
                {
                    var toRemove  = loadedChats.Single(x=>x.Chat.Id == rce.ChatId);
                    loadedChats.Remove(toRemove);
                }
                else
                {
                    foreach (var chat in loadedChats)
                    {
                        accepted = accepted || await cev.Accept(chat);
                    }
                }
                
                
                if (accepted)
                {
                    logger.LogInformation($"Recieved event {cev.ToString()}");
                    await bus.Publish(AppStateChangedEvent.Instance);
                }
                return accepted;
            }
            return false;
        }
    }
}
