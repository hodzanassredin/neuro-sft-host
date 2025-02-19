using System.Text.Json.Serialization;

namespace LlmCommon.Dtos
{

    public class ChatDtoBase {
        public string Name { get; set; } = String.Empty;
        public Ids.Id Id { get; set; } = Ids.Empty;
        public User Owner { get; set; } = User.Empty;
        public AiSettingsDto AiSettings { get; set; } = AiSettingsDto.Default;
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
        [JsonInclude]
        public List<User> Subscribers { get; private set; } = [];
        [JsonInclude]
        public List<MessageDto> Messages { get; private set; } = [];


    }
}
