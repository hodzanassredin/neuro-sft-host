using LlmBackend.Auth;
using LlmCommon;
using LlmCommon.Abstractions;
using LlmCommon.Queries;
using LlmCommon.Views;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
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
            Debug.Assert(view != null && !String.IsNullOrEmpty(key) && view.IsValid());
            var ent = await this.context.Views
                    .SingleOrDefaultAsync(x => x.Id == key);
            if (ent == null)
            {
                var v = new DbView
                {
                    Id = key,
                    View = JsonSerializer.SerializeToDocument(view, view.GetType())
                };
                context.Views.Add(v);
            }
            else {
                ent.View = JsonSerializer.SerializeToDocument(view, view.GetType());
                context.Views.Update(ent);
            }
        }
        private async Task<TV?> Get<TV>(string key) where TV : View
        {
            var ent = await this.context.Views
                    .Where(x=>x.Id == key)
                    .Select(x => x.View)
                    .AsNoTracking()
                    .SingleOrDefaultAsync();
            if (ent == null) {
                return null;
            }
            var res = ent.Deserialize<TV>();
            Debug.Assert(res != null);
            return res;
        }
        
        public override async Task<AllChatsView> Get(AllChatsQuery q)
        {
            var res = await Get<AllChatsView>(Keys.ALL_CHATS_KEY) ?? new AllChatsView();
            return res;
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

        public override Task Remove(View view)
        {
            switch (view)
            {
                case ChatView cv:
                    DbView toDelete = new DbView() { Id = Keys.ChatKey(cv.Chat.Id) };
                    context.Views.Entry(toDelete).State = EntityState.Deleted;
                    break;
                default:
                    throw new NotImplementedException();
            };
            return Task.CompletedTask;
            
        }
    }
}
