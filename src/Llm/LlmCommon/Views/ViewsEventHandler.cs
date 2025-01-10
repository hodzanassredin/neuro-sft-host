using LlmCommon.Abstractions;
using LlmCommon.Events;
using LlmCommon.Queries;

namespace LlmCommon.Views
{
    internal class ViewsEventHandler : IEventHandler
    {
        private readonly ViewStorage viewStorage;

        public ViewsEventHandler(ViewStorage viewStorage)
        {
            this.viewStorage = viewStorage;
        }
        public async Task<bool> Handle(Event ev)
        {
            var changed = false;
            if (ev is ChatEvent cev)
            {
                var chatsView = await viewStorage.Get(AllChatsQuery.Instance);
                if (await cev.Accept(chatsView))
                {
                    await viewStorage.Save(chatsView);
                    changed = true;
                }
                var chatView = await viewStorage.Get(new ChatQuery(cev.ChatId));
                if (chatView!= null && await cev.Accept(chatView))
                {
                    await viewStorage.Save(chatView);
                    changed = true;
                }
            }
            return changed;
        }

        
    }
}
