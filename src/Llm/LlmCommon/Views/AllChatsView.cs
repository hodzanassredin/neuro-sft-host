using LlmCommon.Abstractions;
using LlmCommon.Dtos;
using LlmCommon.Events;

namespace LlmCommon.Views
{
    public class AllChatsView : View, IChatsEventHandler
    {
        public List<ChatDto> Chats { get; set; } = [];

        public Task<bool> Visit(ChangedMessageEvent ev)
        {
            var chat = Chats.Single(x => x.Id == ev.ChatId);
            var msg = chat.Messages.Single(x => x.Id == ev.MessageId);
            if (ev.Append)
            {
                msg.Text += ev.Text;
            }
            else {
                msg.Text = ev.Text;
            }
            return Task.FromResult(true);
        }

        public Task<bool> Visit(CreatedChatEvent ev)
        {
            Chats.Add(new ChatDto() { 
                Id = ev.ChatId,
                Messages = [],
                Name = ev.Name,
                Owner = ev.Owner,
                Subscribers = []
            });
            return Task.FromResult(true);
        }

        public Task<bool> Visit(CreatedMessageEvent ev)
        {
            var chat = Chats.Single(x => x.Id == ev.ChatId);
            chat.Messages.Add(new MessageDto
            {
                Id = ev.MessageId,
                Text = ev.Text,
                User = ev.Writer
            });
            return Task.FromResult(true);
        }

        public Task<bool> Visit(RemovedChatEvent ev)
        {
            var chat = Chats.Single(x => x.Id == ev.ChatId);
            Chats.Remove(chat);
            return Task.FromResult(true);
        }

        public Task<bool> Visit(RemovedMessageEvent ev)
        {
            var chat = Chats.Single(x => x.Id == ev.ChatId);
            var msg = chat.Messages.Single(x => x.Id == ev.MessageId);

            chat.Messages.Remove(msg);
            return Task.FromResult(true);
        }

        public Task<bool> Visit(UserJoinEvent ev)
        {
            var chat = Chats.Single(x => x.Id == ev.ChatId);
            chat.Subscribers.Add(ev.User);
            return Task.FromResult(true);
        }

        public Task<bool> Visit(UserLeaveEvent ev)
        {
            var chat = Chats.Single(x => x.Id == ev.ChatId);
            var exUser = chat.Subscribers.Single(x=>x.Id == ev.User.Id);
            chat.Subscribers.Remove(exUser);
            return Task.FromResult(true);
        }

        public Task<bool> Visit(ChangedChatEvent ev)
        {
            var chat = Chats.Single(x => x.Id == ev.ChatId);
            chat.Name = ev.Name;
            return Task.FromResult(true);
        }
    }
}
