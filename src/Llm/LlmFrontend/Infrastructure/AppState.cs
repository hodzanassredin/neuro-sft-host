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
        }

        public AppState(ILogger<AppState> logger,  IEventBus bus, IRequestHandler handler)
        {
            this.logger = logger;
            this.bus = bus;
            this.handler = handler;
            bus.Subscribe(this);
        }
        private AllChatsView? chats;
        private List<ChatDto> loadedChats = [];

        public AllChatsView ChatsView => chats;

        

        private readonly ILogger<AppState> logger;
        private readonly IEventBus bus;
        private readonly IRequestHandler handler;

        public async Task LoadAsync() {
            chats = await handler.HandleQuery(AllChatsQuery.Instance) as AllChatsView;
            await bus.Publish(AppStateChangedEvent.Instance);
        }


        public async Task<ChatDto> GetChat(Ids.Id id) {
            var chat = loadedChats.SingleOrDefault(x => x.Id == id);
            if (chat == null) {
                var chatView = await handler.HandleQuery(new ChatQuery(id)) as ChatView;
                chat = chatView.Chat;
                loadedChats.Add(chat);
            }

            return chat;
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
