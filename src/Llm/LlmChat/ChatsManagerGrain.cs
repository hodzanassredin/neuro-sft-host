using Orleans;
using Orleans.Runtime;
using Orleans.Streams;

namespace LlmChat
{
    public class ChatsManagerGrain : Grain, IChatsManagerGrain
    {
        private string _owner = String.Empty;
        private readonly HashSet<string> _chats = [];
        private IAsyncStream<string> _stream = null!;

        public override Task OnActivateAsync(CancellationToken cancellationToken)
        {
            var streamProvider = this.GetStreamProvider(Constants.ChatsStreamStorage);

            var streamId = StreamId.Create("ChatRooms", String.Empty);

            _stream = streamProvider.GetStream<string>(
                streamId);

            return base.OnActivateAsync(cancellationToken);
        }
        public Task<List<string>> GetChats()
        {
            return Task.FromResult(new List<string>(_chats));
        }

        public Task CreateChat(string chatId, string userName)
        {
            _owner = userName;
            _chats.Add(chatId);
            return _stream.OnNextAsync(chatId);
        }
    }
}
