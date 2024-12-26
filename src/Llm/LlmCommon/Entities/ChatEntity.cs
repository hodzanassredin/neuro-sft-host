
using LlmCommon.Dtos;
using LlmCommon.Events;
using System.Diagnostics;
using System.Text.Json.Serialization;

namespace LlmCommon.Entities
{
    public class ChatEntity : Entity
    {
        private ChatEntity() { }
        public ChatEntity(string name, User user)
        {
            Apply(new Events.CreatedChatEvent(Ids.dir.GenerateId(), name, user));
        }
        [JsonInclude]
        private ChatDto dto;
        [JsonIgnore]
        public ChatDto Dto => dto;
        public override void Apply(Event @event)
        {
            switch (@event)
            {
                case CreatedChatEvent createdChatEvent:
                    Id = createdChatEvent.ChatId;
                    dto = new ChatDto {
                        Id = createdChatEvent.ChatId,
                        Name = createdChatEvent.Name,
                        Owner = createdChatEvent.Owner,
                    };
                    break;
                case CreatedMessageEvent createdMessageEvent:
                    Debug.Assert(createdMessageEvent.ChatId == this.Id);
                    dto.Messages.Add(new MessageDto { 
                        Id = createdMessageEvent.MessageId,
                        Text = createdMessageEvent.Text,
                        User = createdMessageEvent.Writer
                    });
                    break;
                case ChangedMessageEvent changedMessageEvent:
                    Debug.Assert(changedMessageEvent.ChatId == this.Id);
                    Debug.Assert(changedMessageEvent.MessageId != Ids.Empty);
                    var msg = dto.Messages.Single(x => x.Id == changedMessageEvent.MessageId);
                    msg.Text = changedMessageEvent.Text;
                    break;
                case RemovedChatEvent removedChatEvent:
                    Debug.Assert(removedChatEvent.ChatId == this.Id);
                    this.IsRemoved = true;
                    break;
                case RemovedMessageEvent removedMessageEvent:
                    Debug.Assert(removedMessageEvent.ChatId == this.Id);
                    var msgToRemove = dto.Messages.Single(x => x.Id == removedMessageEvent.MessageId);
                    dto.Messages.Remove(msgToRemove);
                    break;
                case UserJoinEvent userJoinEvent:
                    Debug.Assert(userJoinEvent.ChatId == this.Id);
                    dto.Subscribers.Add(userJoinEvent.User);
                    break;
                case UserLeaveEvent userLeaveEvent:
                    Debug.Assert(userLeaveEvent.ChatId == this.Id);
                    var s = dto.Subscribers.Single(x => x.Id == userLeaveEvent.User.Id);
                    dto.Subscribers.Remove(s);
                    break;
            }
        }
        public void AddMessage(string text, User user) {
            var isSubscriber = this.dto.Subscribers.Any(x => x.Id == user.Id);
            CheckAuth(isSubscriber || user.Id == this.dto.Owner.Id);
            Apply(new CreatedMessageEvent(user, this.Id, Ids.dir.GenerateId(), text));
        }
        public void ChangeMessage(User user, Ids.Id messageId, string text) {
            var msg = this.dto.Messages.Single(x => x.Id == messageId);
            CheckAuth(user.Id == this.dto.Owner.Id || msg.User.Id == user.Id);
            Apply(new ChangedMessageEvent(this.Id, messageId, text, user));
        }
        public void Remove(User user)
        {
            CheckAuth(user.Id == this.dto.Owner.Id);
            Apply(new RemovedChatEvent(this.Id));
        }

        public void RemoveMessage(User user, Ids.Id messageId)
        {
            var msg = this.dto.Messages.Single(x=>x.Id == messageId);
            CheckAuth(user.Id == this.dto.Owner.Id || msg.User.Id == user.Id);
            Apply(new RemovedMessageEvent(this.Id, messageId));
        }

        private void CheckAuth(bool pred) { 
            if (!pred) { throw new Exception("User cant delete other user's data"); }
        }

        public void Join(User user)
        {
            var isSubscribed = this.dto.Subscribers.Any(x => x.Id == user.Id);
            if (!isSubscribed)
            {
                Apply(new UserJoinEvent(this.Id, user));
            }
        }
        public void Leave(User user)
        {
            var isSubscribed = this.dto.Subscribers.Any(x => x.Id == user.Id);
            if (isSubscribed)
            {
                Apply(new UserLeaveEvent(this.Id, user));
            }
        }

    }
}
