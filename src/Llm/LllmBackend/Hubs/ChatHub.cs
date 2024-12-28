using LlmCommon;
using LlmCommon.Abstractions;
using LlmCommon.Commands.Chat;
using LlmCommon.Dtos;
using LlmCommon.Implementations;
using LlmCommon.Transport;
using LlmCommon.Views;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace LlmBackend.Hubs
{
    public interface IFrontendChatClient
    {
        Task HandleEvent(Envelope e);
    }
    public class ChatHub : Hub<IFrontendChatClient>, IContext
    {
        public readonly static ConnectionMapping<Ids.Id> _connections =
            new ConnectionMapping<Ids.Id>();
        private readonly IExecutor executor;
        private readonly IViewStorage viewStorage;
        private readonly AiManager aiManager;

        public ChatHub(IExecutor executor, IViewStorage viewStorage, AiManager aiManager)
        {
            this.executor = executor;
            this.viewStorage = viewStorage;
            this.aiManager = aiManager;
        }

        [Authorize]
        public async Task ExecCommand(Envelope e)
        {
            var cmd = e.Get<Command>();
            await cmd.Accept(executor, this);
            if (cmd is AddMessageCommand amc) {
                await aiManager.StartGeneration(amc.ChatId);
            }
        }
        [Authorize]
        public async Task<Envelope> ExecQuery(Envelope e)
        {
            var q = e.Get<Query>();
            var res = await q.Accept(viewStorage);
            var user = ((IContext)this).GetCurrentUser();
            if (res is AllChatsView acq) {
                foreach (var chat in acq.Chats)
                {
                    if (chat.Subscribers.Any(x => x.Id == user.Id)) {
                        foreach (var connId in _connections.GetConnections(user.Id)) {
                            await Groups.AddToGroupAsync(connId, chat.Id.ToString());
                        }
                    }
                }
            }
            return new Envelope(e.CorellationId, res);
        }
        public override async Task OnConnectedAsync()
        {
            var userName = Context.User?.Identity?.Name;
            
            if (userName != null)
            {
                _connections.Add(((IContext)this).GetCurrentUser().Id, Context.ConnectionId);
            }

            await base.OnConnectedAsync();
        }

        public override Task OnDisconnectedAsync(Exception? exception)
        {
            var userName = Context.User?.Identity?.Name;

            if (userName != null)
            {
                _connections.Remove(((IContext)this).GetCurrentUser().Id, Context.ConnectionId);
            }
            return base.OnDisconnectedAsync(exception);
        }
        User IContext.GetCurrentUser()
        {
            var name = Context.User?.Identity?.Name?? String.Empty;
            return new User(Ids.Parse(name), name);
        }
    }
}
