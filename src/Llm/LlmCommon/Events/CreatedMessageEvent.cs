
using LlmCommon.Abstractions;
using LlmCommon.Dtos;

namespace LlmCommon.Events
{
    public class CreatedMessageEvent : ChatEvent
    {
        public CreatedMessageEvent(User writer, Ids.Id chatId, Ids.Id messageId, string text) : base(chatId)
        {
            Writer = writer;
            MessageId = messageId;
            Text = text;
        }
        public User Writer { get; set; } = User.Empty;
        public Ids.Id MessageId { get; set; } = Ids.Empty;
        public string Text { get; set; } = String.Empty;

        public override Task<bool> Accept(IChatsEventHandler visitor)
        {
            return visitor.Visit(this);
        }
    }
}
