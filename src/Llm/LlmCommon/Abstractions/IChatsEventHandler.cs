using LlmCommon.Events;

namespace LlmCommon.Abstractions
{
    public interface IChatsEventHandler
    {
        Task<bool> Visit(ChangedMessageEvent changedMessageEvent);
        Task<bool> Visit(CreatedChatEvent createdChatEvent);
        Task<bool> Visit(CreatedMessageEvent createdMessageEvent);
        Task<bool> Visit(RemovedChatEvent removedChatEvent);
        Task<bool> Visit(RemovedMessageEvent removedMessageEvent);
        Task<bool> Visit(UserJoinEvent userJoinEvent);
        Task<bool> Visit(UserLeaveEvent userLeaveEvent);
    }
}
