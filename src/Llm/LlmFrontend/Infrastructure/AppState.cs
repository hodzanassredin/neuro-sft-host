using LlmCommon;
using LlmCommon.Abstractions;
using LlmCommon.Events;
using LlmCommon.Queries;
using LlmCommon.Views;
using Microsoft.Extensions.Logging;

namespace LlmFrontend.Infrastructure
{
    public class AppState : IDisposable, IEventHandler
    {
        public class AppStateChangedEvent : Event
        {
            public static AppStateChangedEvent Instance = new AppStateChangedEvent();
        }

        public AppState(ILogger<AppState> logger,  IServiceProvider sp, IEventBus bus, IRequestHandler handler)
        {
            this.logger = logger;
            this.sp = sp;
            this.bus = bus;
            this.handler = handler;
            bus.Subscribe(this);
        }
        private AllChatsView? chats;

        public AllChatsView ChatsView => chats;

        private readonly ILogger<AppState> logger;
        private readonly IServiceProvider sp;
        private readonly IEventBus bus;
        private readonly IRequestHandler handler;

        public async Task LoadAsync() {
            chats = await handler.HandleQuery(AllChatsQuery.Instance) as AllChatsView;
            await bus.Publish(AppStateChangedEvent.Instance);
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
                    logger.LogInformation($"Recieved event {cev.ToString()}");
                    await bus.Publish(AppStateChangedEvent.Instance);
                }
            }
            
        }
    }
}
