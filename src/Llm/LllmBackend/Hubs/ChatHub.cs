using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;

namespace LllmBackend.Hubs
{
    public class ChatHub : Hub
    {
        private static ConcurrentDictionary<string, ConcurrentDictionary<string, object?>> MyUsers = new();
        [Authorize]
        public async Task SendMessage(string user, string message)
        {
            await Clients.All.SendAsync("ReceiveMessage", user, message);
        }

        public override Task OnConnectedAsync()
        {
            var userName = Context.User?.Identity?.Name;
            if (userName != null)
            {
                var newbag = new ConcurrentDictionary<string, object?>
                {
                    [Context.ConnectionId] = null
                };
                MyUsers.AddOrUpdate(userName, newbag, (key, bag) =>
                {
                    bag.TryAdd(Context.ConnectionId, null);
                    return bag;
                });

            }

            return base.OnConnectedAsync();
        }

        public override Task OnDisconnectedAsync(Exception? exception)
        {
            var userName = Context.User?.Identity?.Name;
            if (userName != null)
            {
                MyUsers.AddOrUpdate(userName, new ConcurrentDictionary<string, object?>(), (key, bag) =>
                {
                    bag.TryRemove(Context.ConnectionId, out var garbage);
                    return bag;
                });


            }

            return base.OnDisconnectedAsync(exception);
        }
    }
}
