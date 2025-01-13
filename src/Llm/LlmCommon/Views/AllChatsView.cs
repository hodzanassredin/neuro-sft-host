using LlmCommon.Abstractions;
using LlmCommon.Dtos;
using LlmCommon.Events;
using System.Text.Json.Serialization;

namespace LlmCommon.Views
{
    public class AllChatsView : View, IChatsEventHandler
    {
        [JsonInclude]
        public List<ChatDtoBase> Chats { get; set; } = [];

        public override bool IsValid()
        {
            return Chats != null;
        }

        public Task<bool> Visit(ChangedMessageEvent ev)
        {
            return Task.FromResult(false);
        }

        public Task<bool> Visit(CreatedChatEvent ev)
        {
            Chats.Add(new ChatDtoBase() { 
                Id = ev.ChatId,
                Name = ev.Name,
                Owner = ev.Owner,
            });
            return Task.FromResult(true);
        }

        public Task<bool> Visit(CreatedMessageEvent ev)
        {
            return Task.FromResult(false);
        }

        public Task<bool> Visit(RemovedChatEvent ev)
        {
            var chat = Chats.Single(x => x.Id == ev.ChatId);
            Chats.Remove(chat);
            return Task.FromResult(true);
        }

        public Task<bool> Visit(RemovedMessageEvent ev)
        {
            return Task.FromResult(false);
        }

        public Task<bool> Visit(UserJoinEvent ev)
        {
            return Task.FromResult(false);
        }

        public Task<bool> Visit(UserLeaveEvent ev)
        {
            return Task.FromResult(false);
        }

        public Task<bool> Visit(ChangedChatEvent ev)
        {
            var chat = Chats.Single(x => x.Id == ev.ChatId);
            chat.Name = ev.Name;
            return Task.FromResult(true);
        }
    }
}
