using LlmCommon.Abstractions;
using LlmCommon.Commands.Chat;
using LlmCommon.Entities;
using LlmCommon.Queries;
using LlmCommon.Views;

namespace LlmCommon.Implementations
{
    public class Exceutor(IEntityStorage<ChatEntity> chats, IContext ctx, IEventBus eventBus) : IExecutor, IViewStorage
    {
        public async Task Visit(LeaveCommand cmd)
        {
            var chat = await chats.Load(cmd.ChatId);
            var user = ctx.GetCurrentUser();
            chat.Leave(user);
            await chats.Upsert(chat);
            await eventBus.PublishEventsFrom(chat);
        }

        public async Task Visit(JoinCommand cmd)
        {
            var chat = await chats.Load(cmd.ChatId);
            var user = ctx.GetCurrentUser();
            chat.Join(user);
            await chats.Upsert(chat);
            await eventBus.PublishEventsFrom(chat);
        }

        public async Task Visit(ChangeMessageCommand cmd)
        {
            var chat = await chats.Load(cmd.ChatId);
            var user = ctx.GetCurrentUser();
            chat.ChangeMessage(user, cmd.MessageId, cmd.Text);
            await chats.Upsert(chat);
            await eventBus.PublishEventsFrom(chat);
        }

        public async Task Visit(AddMessageCommand cmd)
        {
            var chat = await chats.Load(cmd.ChatId);
            var user = ctx.GetCurrentUser();
            chat.AddMessage(cmd.Text, user);
            await chats.Upsert(chat);
            await eventBus.PublishEventsFrom(chat);
        }

        public async Task Visit(RemoveMessageCommand cmd)
        {
            var chat = await chats.Load(cmd.ChatId);
            var user = ctx.GetCurrentUser();
            chat.RemoveMessage(user, cmd.MessageId);
            await chats.Upsert(chat);
            await eventBus.PublishEventsFrom(chat);
        }

        public async Task Visit(RemoveChatCommand cmd)
        {
            var chat = await chats.Load(cmd.ChatId);
            var user = ctx.GetCurrentUser();
            chat.Remove(user);
            if (chat.IsRemoved)
            {
                await chats.Remove(chat.Id);
            }
            await eventBus.PublishEventsFrom(chat);
        }

        public async Task Visit(AddChatCommand cmd)
        {
            var user = ctx.GetCurrentUser();
            var chat = new ChatEntity(cmd.Name, user);
            await chats.Upsert(chat);
            await eventBus.PublishEventsFrom(chat);
        }

        public async Task<AllChatsView> Visit(AllChatsQuery q)
        {
            var allChats = await chats.GetAll();
            return new AllChatsView() { 
                Chats = allChats.Select(x=>x.Dto).ToList(),
            };
        }
    }
}
