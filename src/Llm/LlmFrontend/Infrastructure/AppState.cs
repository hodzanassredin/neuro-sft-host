using LlmCommon;
using LlmCommon.Abstractions;
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

        public AppState(IRequestHandler handler, IEventBus bus)
        {
            this.handler = handler;
            this.bus = bus;
            bus.Subscribe(this);
        }
        private AllChatsView? chats;

        public AllChatsView ChatsView => chats;

        private readonly IRequestHandler handler;
        private readonly IEventBus bus;

        public async Task LoadAsync() {
            chats = await this.handler.HandleQuery(AllChatsQuery.Instance);
        }

        public void Dispose()
        {
            bus?.UnSubscribe(this);
        }

        public async Task Handle(Event ev)
        {
            if (ev is ChatEvent cev)
            {
                var accepted = await cev.Accept(chats);
                if (accepted)
                {
                    await bus.Publish(AppStateChangedEvent.Instance);
                }
            }
            
        }
    }
}
