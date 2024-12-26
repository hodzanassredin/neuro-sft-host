
using LlmCommon.Abstractions;
using LlmCommon.Dtos;

namespace LlmCommon.Events
{
    public class CreatedMessageEvent : Event
    {
        public CreatedMessageEvent(User writer, Ids.Id chatId, Ids.Id messageId, string text)
        {
            Writer = writer;
            ChatId = chatId;
            MessageId = messageId;
            Text = text;
        }
        public User Writer { get; set; } = User.Empty;
        public Ids.Id ChatId { get; set; } = Ids.Empty;
        public Ids.Id MessageId { get; set; } = Ids.Empty;
        public string Text { get; set; } = String.Empty;

        public override Task Accept(IEventHandler visitor)
        {
            return visitor.Visit(this);
        }
    }
}
