using LlmCommon.Abstractions;
using LlmCommon.Events;
using LlmCommon.Queries;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics;

namespace LlmCommon.Views
{
    internal class ViewsEventHandler : IEventHandler
    {
        private readonly IServiceProvider sp;

        public ViewsEventHandler(IServiceProvider sp)
        {
            this.sp = sp;
        }
        public async Task<bool> Handle(Event ev)
        {
            var changed = false;
            if (ev is ChatEvent cev)
            {
                
                using var scope = sp.CreateScope();
                var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                var viewStorage = scope.ServiceProvider.GetRequiredService<ViewStorage>();

                var chatsView = await viewStorage.Get(AllChatsQuery.Instance);
                if (await cev.Accept(chatsView))
                {
                    await viewStorage.Save(chatsView);
                    changed = true;
                }
                var chatView = await viewStorage.Get(new ChatQuery(cev.ChatId));
                if (chatView == null && ev is CreatedChatEvent cce) {
                    
                    chatView = new ChatView ();
                }
                Debug.Assert(chatView != null);
                if (ev is RemovedChatEvent rce) {
                    await viewStorage.Remove(chatView);
                    changed = true;
                }
                else if (await cev.Accept(chatView))
                {
                    await viewStorage.Save(chatView);
                    changed = true;
                }
                if (changed)
                {
                    await unitOfWork.StoreAsync();
                }
            }
            return changed;
        }

        
    }
}
