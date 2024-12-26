using LlmCommon.Queries;
using LlmCommon.Views;

namespace LlmCommon.Abstractions
{
    public interface IChatService
    {
        void Subscribe(Func<Event> @event);
        Task<AllChatsView> Handle(AllChatsQuery q);
        Task Handle(Commands.Chat.AddChatCommand cmd);
        Task Handle(Commands.Chat.RemoveChatCommand cmd);
        Task Handle(Commands.Chat.AddMessageCommand cmd);
        Task Handle(Commands.Chat.RemoveMessageCommand cmd);
        Task Handle(Commands.Chat.ChangeMessageCommand cmd);
        Task Handle(Commands.Chat.JoinCommand cmd);
        Task Handle(Commands.Chat.LeaveCommand cmd);
    }
}
