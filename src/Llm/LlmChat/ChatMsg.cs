using Orleans;

namespace LlmChat
{
    [GenerateSerializer]
    public record class ChatMsg(
        string? Author,
        string Text)
    {
        [Id(0)]
        public string Author { get; init; } = Author ?? "None";

        [Id(1)]
        public DateTimeOffset Created { get; init; } = DateTimeOffset.Now;
    }
    [GenerateSerializer]
    public record class RoomChatMsg(
        string room,
        ChatMsg msg)
    {
        [Id(0)]
        public string Room { get; init; } = room;

        [Id(1)]
        public ChatMsg Msg { get; init; } = msg;
    }
}
