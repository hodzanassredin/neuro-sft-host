using LlmCommon.Abstractions;
using LlmCommon.Dtos;

namespace LlmCommon.Events
{
    public class ChangedChatEvent : ChatEvent
    {
        public ChangedChatEvent(Ids.Id chatId, string name, User user)
        {
            ChatId = chatId;
            Name = name;
            User = user;
        }
        public Ids.Id ChatId { get; set; }
        public string Name { get; set; }
        public User User { get; set; }

        public override Task<bool> Accept(IChatsEventHandler visitor)
        {
            return visitor.Visit(this);
        }
    }
}
