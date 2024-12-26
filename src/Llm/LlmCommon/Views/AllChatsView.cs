using LlmCommon.Dtos;

namespace LlmCommon.Views
{
    public class AllChatsView : View
    {
        public List<ChatDto> Chats { get; set; } = [];
    }
}
