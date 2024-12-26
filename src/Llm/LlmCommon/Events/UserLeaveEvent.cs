using LlmCommon.Abstractions;
using LlmCommon.Dtos;

namespace LlmCommon.Events
{
    public class UserLeaveEvent : Event
    {
        public Ids.Id ChatId { get; set; }

        public UserLeaveEvent(Ids.Id chatId, User user)
        {
            ChatId = chatId;
            User = user;
        }

        public User User { get; set; }

        public override Task Accept(IEventHandler visitor)
        {
            return visitor.Visit(this);
        }
    }
}
