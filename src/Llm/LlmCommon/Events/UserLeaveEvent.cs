using LlmCommon.Abstractions;
using LlmCommon.Dtos;

namespace LlmCommon.Events
{
    public class UserLeaveEvent : ChatEvent
    {

        public UserLeaveEvent(Ids.Id chatId, User user) : base(chatId)
        {
            User = user;
        }

        public User User { get; set; }

        public override Task<bool> Accept(IChatsEventHandler visitor)
        {
            return visitor.Visit(this);
        }
    }
}
