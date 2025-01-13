using LlmCommon;
using LlmCommon.Abstractions;
using LlmCommon.Dtos;
using LlmCommon.Transport;
using LlmCommon.Views;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Diagnostics;

namespace LlmBackend.Hubs
{
    public interface IFrontendChatClient
    {
        Task HandleEvent(Envelope e);
    }
    public class ChatHub : Hub<IFrontendChatClient>, IContext
    {

        public static string GetGroupName(Ids.Id chatId)
        {
            return chatId.ToString();
        }

        public readonly static ConnectionMapping<Ids.Id> _connections =
            new ConnectionMapping<Ids.Id>();
        private readonly IExecutor executor;
        private readonly ViewStorage viewStorage;
        private readonly IUnitOfWork unitOfWork;

        public ChatHub(IExecutor executor, ViewStorage viewStorage, IUnitOfWork unitOfWork)
        {
            this.executor = executor;
            this.viewStorage = viewStorage;
            this.unitOfWork = unitOfWork;
        }

        [Authorize]
        public async Task ExecCommand(Envelope e)
        {
            var cmd = e.Get<Command>();

            var sw = Stopwatch.StartNew();
            await cmd.Accept(executor, this);
            await unitOfWork.SaveChangesAsync();
        }
        [Authorize]
        public async Task<Envelope> ExecQuery(Envelope e)
        {
            var q = e.Get<Query>();
            var res = await q.Accept(viewStorage);
            var user = ((IContext)this).GetCurrentUser();
            if (res is ChatView acq) {
                foreach (var connId in _connections.GetConnections(user.Id)) {
                    await Groups.AddToGroupAsync(connId, GetGroupName(acq.Chat.Id));
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
