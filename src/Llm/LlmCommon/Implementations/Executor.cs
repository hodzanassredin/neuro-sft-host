using LlmCommon.Abstractions;
using LlmCommon.Commands.Chat;
using LlmCommon.Entities;
using LlmCommon.Tasks;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics;

namespace LlmCommon.Implementations
{
    public class Executor(IEntityStorage storage, IUnitOfWork unitOfWork, ITaskQueue taskQueue) : IExecutor
    {
        public async Task Visit(LeaveCommand cmd, IContext ctx)
        {
            var chat = await storage.Load<ChatEntity>(cmd.ChatId);
            Debug.Assert(chat != null);
            var user = ctx.GetCurrentUser();
            chat.Leave(user);
            await storage.Upsert(chat);
            await unitOfWork.StoreAsync();
        }

        public async Task Visit(JoinCommand cmd, IContext ctx)
        {
            var chat = await storage.Load<ChatEntity>(cmd.ChatId);
            Debug.Assert(chat != null);
            var user = ctx.GetCurrentUser();
            chat.Join(user);
            await storage.Upsert(chat);
            await unitOfWork.StoreAsync();
        }

        public async Task Visit(ChangeMessageCommand cmd, IContext ctx)
        {
            var chat = await storage.Load<ChatEntity>(cmd.ChatId);
            Debug.Assert(chat != null);
            var user = ctx.GetCurrentUser();
            chat.ChangeMessage(user, cmd.MessageId, cmd.Text, cmd.Append);
            await storage.Upsert(chat);
            await unitOfWork.StoreAsync();
        }

        public async Task Visit(AddMessageCommand cmd, IContext ctx)
        {
            var chat = await storage.Load<ChatEntity>(cmd.ChatId);
            Debug.Assert(chat != null);
            var user = ctx.GetCurrentUser();
            cmd.AddedMessageId = chat.AddMessage(cmd.Text, user);
            await storage.Upsert(chat);
            await unitOfWork.StoreAsync();
            if (user.Id != AiManager.aiUser.Id) {
                await taskQueue.QueueBackgroundWorkItemAsync(new GenerateResponseTask(cmd.ChatId));
            }
        }

        public async Task Visit(RemoveMessageCommand cmd, IContext ctx)
        {
            var chat = await storage.Load<ChatEntity>(cmd.ChatId);
            var user = ctx.GetCurrentUser();
            chat.RemoveMessage(user, cmd.MessageId);
            await storage.Upsert(chat);
            await unitOfWork.StoreAsync();
        }

        public async Task Visit(RemoveChatCommand cmd, IContext ctx)
        {
            var chat = await storage.Load<ChatEntity>(cmd.ChatId);
            var user = ctx.GetCurrentUser();
            chat.Remove(user);
            if (chat.IsRemoved)
            {
                await storage.Remove(chat);
            }
            await unitOfWork.StoreAsync();
        }

        public async Task Visit(AddChatCommand cmd, IContext ctx)
        {
            var user = ctx.GetCurrentUser();
            var chat = new ChatEntity(cmd.Name, user);
            await storage.Upsert(chat);
            await unitOfWork.StoreAsync();
        }

        public async Task Visit(ChangeChatCommand cmd, IContext ctx)
        {
            var chat = await storage.Load<ChatEntity>(cmd.ChatId);
            var user = ctx.GetCurrentUser();
            if (chat.Dto.Owner.Id == user.Id)
            {
                chat.ChangeChat(user, cmd.Text, cmd.AiSettings);
                await storage.Upsert(chat);
            }
            await unitOfWork.StoreAsync();
        }

        public async Task Visit(RegenerateMessageCommand cmd, IContext ctx)
        {
            var chat = await storage.Load<ChatEntity>(cmd.ChatId);
            Debug.Assert(chat != null);
            var user = ctx.GetCurrentUser();
            chat.ChangeMessage(user, cmd.MessageId, "", false);
            await storage.Upsert(chat);
            await unitOfWork.StoreAsync();
            await taskQueue.QueueBackgroundWorkItemAsync(new GenerateResponseTask(cmd.ChatId, cmd.MessageId));
        }


    }
}
