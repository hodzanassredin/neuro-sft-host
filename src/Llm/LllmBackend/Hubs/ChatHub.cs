using LlmChat;
using LlmCommon.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Orleans.Streams;

namespace LlmBackend.Hubs
{
    public class ChatHub : Hub, IEventHandler
    {
        private readonly IClusterClient _clusterClient;

        public ChatHub(IClusterClient clusterClient)
        {
            _clusterClient = clusterClient;
            var streamProvider = _clusterClient.GetStreamProvider(Constants.ChatsStreamStorage);
            var chatRoomsStream = streamProvider.GetStream<string>(
                StreamId.Create("ChatRooms", string.Empty));
            chatRoomsStream.SubscribeAsync((data, token) => this.Clients.All.SendAsync("NewGroup", data));

            var newMessagesStream = streamProvider.GetStream<RoomChatMsg>(
                StreamId.Create("ChatRoom", string.Empty));
            newMessagesStream.SubscribeAsync((data, token) => this.Clients.Group(data.room).SendAsync("NewMessage", data.room, data.msg));
        }
        [Authorize]
        public async Task JoinChat(string chatId, string user)
        {
            var userName = Context.User?.Identity?.Name;
            if (userName == null) { throw new Exception("User should be authenticated"); }

            var chatGrain = _clusterClient.GetGrain<IChannelGrain>(chatId);
            await chatGrain.Join(user);
            
            await Groups.AddToGroupAsync(Context.ConnectionId, chatId);
        }
        [Authorize]
        public async Task LeaveChat(string chatId, string user)
        {
            var chatGrain = _clusterClient.GetGrain<IChannelGrain>(chatId);
            await chatGrain.Leave(user);
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, chatId);
        }
        [Authorize]
        public async Task CreateChat(string chatId)
        {
            var userName = Context.User?.Identity?.Name;
            if (userName == null) { throw new Exception("User should be authenticated"); }
            var chatManagerGrain = _clusterClient.GetGrain<IChatsManagerGrain>("ChatManager");
            await chatManagerGrain.CreateChat(chatId, userName);
        }


        [Authorize]
        public async Task SendMessage(string room, string message)
        {
            var userName = Context.User?.Identity?.Name;
            var chatGrain = _clusterClient.GetGrain<IChannelGrain>(room);
            await chatGrain.Message(new ChatMsg(userName, message));
        }

        public override async Task OnConnectedAsync()
        {
            var userName = Context.User?.Identity?.Name;
            if (userName != null)
            {
                var userGrain = _clusterClient.GetGrain<IUserGrain>(userName);
                var subscriptions = await userGrain.GetSubscriptions();
                
                var managerGrain = _clusterClient.GetGrain<IChatsManagerGrain>(string.Empty);
                var allChats = await managerGrain.GetChats();

                var chatsData = new Dictionary<string, ChatMsg[]>();
                foreach (var chat in allChats) {

                    if (subscriptions.Contains(chat))
                    {
                        var chatGrain = _clusterClient.GetGrain<IChannelGrain>(chat);
                        var history = await chatGrain.ReadHistory(10);
                        chatsData.Add(chat, history);
                    }
                    else {
                        chatsData.Add(chat, []);
                    }
                    

                }
                await Clients.Caller.SendAsync("LoadChats", chatsData);


                foreach (var item in subscriptions)
                {
                    await Groups.AddToGroupAsync(Context.ConnectionId, item);
                }
            }

            await base.OnConnectedAsync();
        }

        public override Task OnDisconnectedAsync(Exception? exception)
        {
            return base.OnDisconnectedAsync(exception);
        }
    }
}
