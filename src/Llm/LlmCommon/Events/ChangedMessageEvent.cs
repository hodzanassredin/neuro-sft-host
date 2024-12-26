
using LlmCommon.Abstractions;
using LlmCommon.Dtos;

namespace LlmCommon.Events
{
    public class ChangedMessageEvent : Event
    {
        public ChangedMessageEvent(Ids.Id chatId, Ids.Id messageId, string message, User user)
        {
            ChatId = chatId;
            MessageId = messageId;
            Text = message;
            User = user;
        }
        public Ids.Id ChatId { get; set; } = Ids.Empty;
        public string Text { get; set; } = String.Empty;
        public Ids.Id MessageId { get; set; } = Ids.Empty;
        public User User { get; set; }

        public override Task Accept(IEventHandler visitor)
        {
            return visitor.Visit(this);
        }
    }
}
