using LlmCommon.Events;

namespace LlmCommon.Abstractions
{
    public interface IEventHandler
    {
        Task Visit(ChangedMessageEvent changedMessageEvent);
        Task Visit(CreatedChatEvent createdChatEvent);
        Task Visit(CreatedMessageEvent createdMessageEvent);
        Task Visit(RemovedChatEvent removedChatEvent);
        Task Visit(RemovedMessageEvent removedMessageEvent);
        Task Visit(UserJoinEvent userJoinEvent);
        Task Visit(UserLeaveEvent userLeaveEvent);
    }
}
