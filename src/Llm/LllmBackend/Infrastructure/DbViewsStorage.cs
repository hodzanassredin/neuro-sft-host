using LlmBackend.Auth;
using LlmCommon;
using LlmCommon.Abstractions;
using LlmCommon.Queries;
using LlmCommon.Views;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace LlmBackend.Infrastructure
{
    class DbViewsStorage : ViewStorage
    {
        private readonly AppDbContext context;

        public DbViewsStorage(AppDbContext context)
        {
            this.context = context;
        }

        private async Task Save(View view, string key) {
            var ent = await this.context.Views
                    .SingleOrDefaultAsync(x => x.Id == key);
            if (ent == null)
            {
                context.Views.Add(new DbView
                {
                    Id = key,
                    View = JsonSerializer.SerializeToDocument(view)
                });
            }
            else {
                ent.View = JsonSerializer.SerializeToDocument(view);
                context.Views.Update(ent);
            }
        }
        private async Task<TV?> Get<TV>(string key) where TV : View
        {
            var ent = await this.context.Views
                    .Where(x=>x.Id == key)
                    .Select(x => x.View)
                    .AsNoTracking()
                    .FirstOrDefaultAsync();
            if (ent == null) {
                return null;
            }
            return ent.Deserialize<TV>();
        }
        
        public override Task<AllChatsView> Get(AllChatsQuery q)
        {
            return Get<AllChatsView>(Keys.ALL_CHATS_KEY)!;
        }

        public override Task<ChatView?> Get(ChatQuery q)
        {
            return Get<ChatView>(Keys.ChatKey(q.ChatId));
        }

        public override Task Save(View view)
        {
            switch (view)
            {
                case AllChatsView _:
                    return Save(view, Keys.ALL_CHATS_KEY);
                case ChatView cv:
                    return Save(view, Keys.ChatKey(cv.Chat.Id));
                default:
                    throw new NotImplementedException();
            };
        }
    }
}
