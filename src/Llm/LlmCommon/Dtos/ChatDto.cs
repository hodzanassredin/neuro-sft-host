namespace LlmCommon.Dtos
{
    public class ChatDtoBase {
        public string Name { get; set; } = String.Empty;
        public Ids.Id Id { get; set; } = Ids.Empty;
        public User Owner { get; set; } = User.Empty;
    }
    public class ChatDto : ChatDtoBase
    {
        private ChatDto(List<User> subscribers, List<MessageDto> messages)
        {
            Subscribers = subscribers ?? [];
            Messages = messages ?? [];
        }
        public ChatDto()
        {
            
        }
        public List<User> Subscribers { get; private set; } = [];
        public List<MessageDto> Messages { get; private set; } = [];
    }
}
