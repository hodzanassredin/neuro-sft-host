using LlmBackend.Hubs;
using LlmCommon;
using LlmCommon.Abstractions;
using LlmCommon.Events;
using Microsoft.AspNetCore.SignalR;

namespace LlmBackend.Infrastructure
{
    public class ChatHubEventHandler : IChatsEventHandler, IEventHandler
    {
        private readonly ChatHub hub;

        public ChatHubEventHandler(ChatHub hub)
        {
            this.hub = hub;
        }

        private string GetGroupName(Ids.Id chatId) {
            return chatId.ToString();
        }

        public async Task<bool> Visit(ChangedMessageEvent ev)
        {
            await this.hub.Clients.Group(GetGroupName(ev.ChatId)).SendAsync("HandleEvent", ev);
            return true;
        }

        public async Task<bool> Visit(CreatedChatEvent ev)
        {
            await this.hub.Clients.All.SendAsync("HandleEvent", ev);
            return true;
        }

        public async Task<bool> Visit(CreatedMessageEvent ev)
        {
            await this.hub.Clients.Group(GetGroupName(ev.ChatId)).SendAsync("HandleEvent", ev);
            return true;
        }

        public async Task<bool> Visit(RemovedChatEvent ev)
        {
            await this.hub.Clients.All.SendAsync("HandleEvent", ev);
            return true;
        }

        public async Task<bool> Visit(RemovedMessageEvent ev)
        {
            await this.hub.Clients.Group(GetGroupName(ev.ChatId)).SendAsync("HandleEvent", ev);
            return true;
        }

        public async Task<bool> Visit(UserJoinEvent ev)
        {
            var connections = this.hub.GetCurrentUserConnections();
            foreach (var connId in connections)
            {
                await this.hub.Groups.AddToGroupAsync(connId, GetGroupName(ev.ChatId));
            }
            return true;
        }

        public async Task<bool> Visit(UserLeaveEvent ev)
        {
            var connections = this.hub.GetCurrentUserConnections();
            foreach (var connId in connections)
            {
                await this.hub.Groups.RemoveFromGroupAsync(connId, GetGroupName(ev.ChatId));
            }
            return true;
        }

        public async Task Handle(Event ev)
        {
            if (ev is ChatEvent cev) { 
                await cev.Accept(this);
            }
        }
    }
}
