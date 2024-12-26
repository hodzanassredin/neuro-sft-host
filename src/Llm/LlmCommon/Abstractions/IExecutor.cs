using LlmCommon.Commands.Chat;
namespace LlmCommon.Abstractions
{
    public interface IExecutor
    {
        Task Visit(LeaveCommand leaveCommand);
        Task Visit(JoinCommand joinCommand);
        Task Visit(ChangeMessageCommand upsertMessageCommand);
        Task Visit(AddMessageCommand addMessageCommand);
        Task Visit(RemoveMessageCommand removeMessageCommand);
        Task Visit(RemoveChatCommand removeChatCommand);
        Task Visit(AddChatCommand addChatCommand);
    }
}
