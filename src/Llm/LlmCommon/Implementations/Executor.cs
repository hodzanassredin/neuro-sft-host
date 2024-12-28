using LlmCommon.Abstractions;
using LlmCommon.Commands.Chat;
using LlmCommon.Entities;
using LlmCommon.Queries;
using LlmCommon.Views;

namespace LlmCommon.Implementations
{
    public class Executor(IEntityStorage<ChatEntity> chats, IEventBus eventBus) : IExecutor, IViewStorage
    {
        public Ids.Id LastAddedMessageId { get; private set; }
        public async Task Visit(LeaveCommand cmd, IContext ctx)
        {
            var chat = await chats.Load(cmd.ChatId);
            var user = ctx.GetCurrentUser();
            chat.Leave(user);
            await chats.Upsert(chat);
            await eventBus.PublishEventsFrom(chat);
        }

        public async Task Visit(JoinCommand cmd, IContext ctx)
        {
            var chat = await chats.Load(cmd.ChatId);
            var user = ctx.GetCurrentUser();
            chat.Join(user);
            await chats.Upsert(chat);
            await eventBus.PublishEventsFrom(chat);
        }

        public async Task Visit(ChangeMessageCommand cmd, IContext ctx)
        {
            var chat = await chats.Load(cmd.ChatId);
            var user = ctx.GetCurrentUser();
            chat.ChangeMessage(user, cmd.MessageId, cmd.Text, cmd.Append);
            await chats.Upsert(chat);
            await eventBus.PublishEventsFrom(chat);
        }

        public async Task Visit(AddMessageCommand cmd, IContext ctx)
        {
            var chat = await chats.Load(cmd.ChatId);
            var user = ctx.GetCurrentUser();
            var id = chat.AddMessage(cmd.Text, user);
            await chats.Upsert(chat);
            await eventBus.PublishEventsFrom(chat);
            LastAddedMessageId = id;
        }

        public async Task Visit(RemoveMessageCommand cmd, IContext ctx)
        {
            var chat = await chats.Load(cmd.ChatId);
            var user = ctx.GetCurrentUser();
            chat.RemoveMessage(user, cmd.MessageId);
            await chats.Upsert(chat);
            await eventBus.PublishEventsFrom(chat);
        }

        public async Task Visit(RemoveChatCommand cmd, IContext ctx)
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

        public async Task Visit(AddChatCommand cmd, IContext ctx)
        {
            var user = ctx.GetCurrentUser();
            var chat = new ChatEntity(cmd.Name, user);
            await chats.Upsert(chat);
            await eventBus.PublishEventsFrom(chat);
        }

        public async Task<View> Get(AllChatsQuery q)
        {
            var allChats = await chats.GetAll();
            return new AllChatsView() { 
                Chats = allChats.Select(x=>x.Dto).ToList(),
            };
        }

        public async Task Visit(ChangeChatCommand cmd, IContext ctx)
        {
            var chat = await chats.Load(cmd.ChatId);
            var user = ctx.GetCurrentUser();
            if (chat.Dto.Owner.Id == user.Id)
            {
                chat.ChangeChat(user, cmd.Text);
                await chats.Upsert(chat);
                await eventBus.PublishEventsFrom(chat);
            }
        }
    }
}
