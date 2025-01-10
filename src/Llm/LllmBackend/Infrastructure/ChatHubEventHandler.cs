using LlmBackend.Hubs;
using LlmCommon;
using LlmCommon.Abstractions;
using LlmCommon.Dtos;
using LlmCommon.Events;
using LlmCommon.Transport;
using Microsoft.AspNetCore.SignalR;

namespace LlmBackend.Infrastructure
{
    public class ChatHubEventHandler : IChatsEventHandler, IEventHandler
    {
        private readonly IHubContext<ChatHub, IFrontendChatClient> hub;

        public ChatHubEventHandler(IHubContext<ChatHub, IFrontendChatClient> hub)
        {
            this.hub = hub;

        }

        private string GetGroupName(Ids.Id chatId) {
            return chatId.ToString();
        }

        public async Task<bool> Visit(ChangedMessageEvent ev)
        {
            await this.hub.Clients.Group(GetGroupName(ev.ChatId)).HandleEvent(new Envelope(Ids.dir.GenerateId(), ev));
            return true;
        }

        public async Task<bool> Visit(CreatedChatEvent ev)
        {
            await this.hub.Clients.All.HandleEvent(new Envelope(Ids.dir.GenerateId(), ev));
            await AddUserToGroup(ev.ChatId, ev.Owner);
            return true;
        }

        public async Task<bool> Visit(CreatedMessageEvent ev)
        {
            await this.hub.Clients.Group(GetGroupName(ev.ChatId)).HandleEvent(new Envelope(Ids.dir.GenerateId(), ev));
            return true;
        }

        public async Task<bool> Visit(RemovedChatEvent ev)
        {
            await this.hub.Clients.All.HandleEvent(new Envelope(Ids.dir.GenerateId(), ev));
            return true;
        }

        public async Task<bool> Visit(RemovedMessageEvent ev)
        {
            await this.hub.Clients.Group(GetGroupName(ev.ChatId)).HandleEvent(new Envelope(Ids.dir.GenerateId(), ev));
            return true;
        }

        public async Task<bool> Visit(UserJoinEvent ev)
        {
            await AddUserToGroup(ev.ChatId, ev.User);
            return true;
        }
        public IEnumerable<string> GetUserConnections(User user)
        {
            return ChatHub._connections.GetConnections(user.Id);
        }
        private async Task AddUserToGroup(Ids.Id chatId, User user)
        {
            var connections = this.GetUserConnections(user);
            foreach (var connId in connections)
            {
                await this.hub.Groups.AddToGroupAsync(connId, GetGroupName(chatId));
            }
        }

        public async Task<bool> Visit(UserLeaveEvent ev)
        {
            var connections = this.GetUserConnections(ev.User);
            foreach (var connId in connections)
            {
                await this.hub.Groups.RemoveFromGroupAsync(connId, GetGroupName(ev.ChatId));
            }
            return true;
        }

        public Task<bool> Handle(Event ev)
        {
            if (ev is ChatEvent cev) { 
                return cev.Accept(this);
            }
            return Task.FromResult(false);
        }

        public async Task<bool> Visit(ChangedChatEvent ev)
        {
            await this.hub.Clients.All.HandleEvent(new Envelope(Ids.dir.GenerateId(), ev));
            return true;
        }
    }
}
