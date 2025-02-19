using LlmCommon.Abstractions;
using LlmCommon.Dtos;

namespace LlmCommon.Events
{
    public class ChangedChatEvent : ChatEvent
    {
        public ChangedChatEvent(Ids.Id chatId, string? name, AiSettingsDto? aiSettings, User user) : base(chatId)
        {
            Name = name;
            AiSettings = aiSettings;
            User = user;
        }

        public string Name { get; set; }
        public AiSettingsDto? AiSettings { get; }
        public User User { get; set; }

        public override Task<bool> Accept(IChatsEventHandler visitor)
        {
            return visitor.Visit(this);
        }
    }
}
