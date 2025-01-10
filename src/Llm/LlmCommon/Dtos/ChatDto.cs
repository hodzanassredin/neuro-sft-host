namespace LlmCommon.Dtos
{
    public class ChatDtoBase {
        public string Name { get; set; } = String.Empty;
        public Ids.Id Id { get; set; } = Ids.Empty;
        public User Owner { get; set; } = User.Empty;
    }
    public class ChatDto : ChatDtoBase
    {
        public List<User> Subscribers { get; set; } = [];
        public List<MessageDto> Messages { get; set; } = [];
    }
}
