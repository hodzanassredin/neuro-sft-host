
using LlmCommon.Abstractions;
using LlmCommon.Dtos;

namespace LlmCommon.Events
{
    public class ChangedMessageEvent : ChatEvent
    {
        public override string ToString()
        {
            return $"{nameof(ChangedMessageEvent)} ({User.Name}: ({ChatId}:{MessageId}) {Text}";
        }
        public ChangedMessageEvent(Ids.Id chatId, Ids.Id messageId, string text, User user, bool append)
        {
            ChatId = chatId;
            MessageId = messageId;
            Text = text;
            User = user;
            Append = append;
        }
        public Ids.Id ChatId { get; set; } = Ids.Empty;
        public string Text { get; set; } = String.Empty;
        public Ids.Id MessageId { get; set; } = Ids.Empty;
        public User User { get; set; }
        public bool Append { get; }

        public override Task<bool> Accept(IChatsEventHandler visitor)
        {
            return visitor.Visit(this);
        }
    }
}
