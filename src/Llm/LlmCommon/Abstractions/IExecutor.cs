using LlmCommon.Commands.Chat;
namespace LlmCommon.Abstractions
{
    public interface IExecutor
    {
        Ids.Id LastAddedMessageId { get;  }

        Task Visit(LeaveCommand leaveCommand, IContext ctx);
        Task Visit(JoinCommand joinCommand, IContext ctx);
        Task Visit(ChangeMessageCommand upsertMessageCommand, IContext ctx);
        Task Visit(AddMessageCommand addMessageCommand, IContext ctx);
        Task Visit(RemoveMessageCommand removeMessageCommand, IContext ctx);
        Task Visit(RemoveChatCommand removeChatCommand, IContext ctx);
        Task Visit(AddChatCommand addChatCommand, IContext ctx);
        Task Visit(ChangeChatCommand changeChatCommand, IContext ctx);
        
    }
}
