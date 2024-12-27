﻿using LlmBackend.Infrastructure;
using LlmCommon;
using LlmCommon.Abstractions;
using LlmCommon.Dtos;
using LlmCommon.Entities;
using LlmCommon.Implementations;
using LlmCommon.Transport;
using LlmCommon.Views;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using static LlmCommon.Ids;

namespace LlmBackend.Hubs
{
    public interface IChatClient
    {
        Task HandleEvent(Envelope e);
    }
    public class ChatHub : Hub<IChatClient>, IContext
    {
        private readonly static ConnectionMapping<string> _connections =
            new ConnectionMapping<string>();

        private readonly IEntityStorage<ChatEntity> chats;

        public ChatHub(IEntityStorage<ChatEntity> chats)
        {
            this.chats = chats;
        }
        [Authorize]
        public IEnumerable<string> GetCurrentUserConnections() {
            var userName = Context.User?.Identity?.Name;
            return _connections.GetConnections(userName);
        }

        private Excecutor GetExecutor() {
            var eventBus = new SimpleEventBus();
            eventBus.Subscribe(new ChatHubEventHandler(this));
            return new Excecutor(chats, this, eventBus);
        }

        [Authorize]
        public async Task ExecCommand(Envelope e)
        {
            var cmd = e.Get<Command>();
            await cmd.Accept(GetExecutor());
        }
        [Authorize]
        public async Task<Envelope> ExecQuery(Envelope e)
        {
            var q = e.Get<Query>();
            var res = await q.Accept(GetExecutor());
            var user = GetCurrentUser();
            if (res is AllChatsView acq) {
                foreach (var chat in acq.Chats)
                {
                    if (chat.Subscribers.Any(x => x.Id == user.Id)) {
                        foreach (var connId in _connections.GetConnections(user.Id.ToString())) {
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
                _connections.Add(userName, Context.ConnectionId);
            }

            await base.OnConnectedAsync();
        }

        public override Task OnDisconnectedAsync(Exception? exception)
        {
            var userName = Context.User?.Identity?.Name;

            if (userName != null)
            {
                _connections.Remove(userName, Context.ConnectionId);
            }
            return base.OnDisconnectedAsync(exception);
        }
        [Authorize]
        public User GetCurrentUser()
        {
            var name = Context.User?.Identity?.Name?? String.Empty;
            return new User(Ids.Parse(name), name);
        }
    }
}
