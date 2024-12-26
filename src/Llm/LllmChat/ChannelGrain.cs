using Orleans.Runtime;
using Orleans.Streams;
using Orleans;

namespace LlmChat
{
    public class ChannelGrain : Grain, IChannelGrain
    {
        private readonly List<ChatMsg> _messages = new(100);
        private readonly List<string> _onlineMembers = new(10);

        private IAsyncStream<RoomChatMsg> _stream = null!;

        public override Task OnActivateAsync(CancellationToken cancellationToken)
        {
            var streamProvider = this.GetStreamProvider(Constants.ChatsStreamStorage);

            var streamId = StreamId.Create(
                "ChatRoom", String.Empty);

            _stream = streamProvider.GetStream<RoomChatMsg>(
                streamId);

            return base.OnActivateAsync(cancellationToken);
        }

        public async Task<StreamId> Join(string nickname)
        {
            var chatId = this.GetPrimaryKeyString();
            _onlineMembers.Add(nickname);
            var userGrain = this.GrainFactory.GetGrain<IUserGrain>(nickname);
            await userGrain.AddSubscription(chatId);

            await _stream.OnNextAsync(new RoomChatMsg(this.GetPrimaryKeyString(),
                new ChatMsg(
                    "System",
                    $"{nickname} joins the chat '{this.GetPrimaryKeyString()}' ...")));

            return _stream.StreamId;
        }

        public async Task<StreamId> Leave(string nickname)
        {
            _onlineMembers.Remove(nickname);

            await _stream.OnNextAsync(new RoomChatMsg(this.GetPrimaryKeyString(),
                new ChatMsg(
                    "System",
                    $"{nickname} leaves the chat...")));

            return _stream.StreamId;
        }

        public async Task<bool> Message(ChatMsg msg)
        {
            _messages.Add(msg);

            await _stream.OnNextAsync(new RoomChatMsg(this.GetPrimaryKeyString(), msg));

            return true;
        }

        public Task<string[]> GetMembers() => Task.FromResult(_onlineMembers.ToArray());

        public Task<ChatMsg[]> ReadHistory(int numberOfMessages)
        {
            var response = _messages
                .OrderByDescending(x => x.Created)
                .Take(numberOfMessages)
                .OrderBy(x => x.Created)
                .ToArray();

            return Task.FromResult(response);
        }
    }
}
