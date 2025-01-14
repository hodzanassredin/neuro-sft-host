using LlmCommon.Abstractions;
using LlmCommon.Dtos;
using LlmCommon.Events;
using System.Diagnostics;
using System.Text.Json.Serialization;

namespace LlmCommon.Views
{
    public class ChatView : View, IChatsEventHandler
    {
        [JsonInclude]
        public ChatDto Chat { get; set; }

        public override bool IsValid()
        {
            return Chat != null && Chat.Id != Ids.Empty && Chat.Owner.Id != Ids.Empty;
        }

        public Task<bool> Visit(ChangedMessageEvent ev)
        {
            var msg = Chat.Messages.Single(x => x.Id == ev.MessageId);
            if (ev.Append)
            {
                msg.Text += ev.Text;
            }
            else {
                msg.Text = ev.Text;
            }
            return Task.FromResult(true);
        }

        public Task<bool> Visit(CreatedMessageEvent ev)
        {
            if (Chat?.Id != ev.ChatId) {
                return Task.FromResult(false); 
            }
            var alreadyContains = Chat.Messages.Any(x => x.Id == ev.MessageId);
            Debug.Assert(!alreadyContains);
            Chat.Messages.Add(new MessageDto
            {
                Id = ev.MessageId,
                Text = ev.Text,
                User = ev.Writer
            });
            return Task.FromResult(true);
        }

        public Task<bool> Visit(RemovedMessageEvent ev)
        {
            Debug.Assert(Chat?.Id == ev.ChatId);
            var msg = Chat.Messages.Single(x => x.Id == ev.MessageId);

            Chat.Messages.Remove(msg);
            return Task.FromResult(true);
        }

        public Task<bool> Visit(UserJoinEvent ev)
        {
            Debug.Assert(Chat?.Id == ev.ChatId);
            Chat.Subscribers.Add(ev.User);
            return Task.FromResult(true);
        }

        public Task<bool> Visit(UserLeaveEvent ev)
        {
            Debug.Assert(Chat?.Id == ev.ChatId);
            var exUser = Chat.Subscribers.Single(x=>x.Id == ev.User.Id);
            Chat.Subscribers.Remove(exUser);
            return Task.FromResult(true);
        }

        public Task<bool> Visit(ChangedChatEvent ev)
        {
            Debug.Assert(Chat?.Id == ev.ChatId);
            if (ev.Name != null)
            {
                Chat.Name = ev.Name;
            }
            if (ev.AiSettings != null) { 
                Chat.AiSettings = ev.AiSettings;
            }
            return Task.FromResult(true);
        }

        public Task<bool> Visit(CreatedChatEvent createdChatEvent)
        {
            Debug.Assert(this.Chat == null);
            this.Chat = new ChatDto { 
                Id = createdChatEvent.ChatId,
                Name = createdChatEvent.Name,
                Owner = createdChatEvent.Owner
            };
            return Task.FromResult(true);
        }

        public Task<bool> Visit(RemovedChatEvent removedChatEvent)
        {
            return Task.FromResult(false);
        }
    }
}
