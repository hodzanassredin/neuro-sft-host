using LlmBackend.Hubs;
using LlmCommon;
using LlmCommon.Abstractions;
using LlmCommon.Events;
using Microsoft.AspNetCore.SignalR;

namespace LlmBackend.Infrastructure
{
    public class ChatHubEventHandler : IEventHandler
    {
        private readonly ChatHub hub;

        public ChatHubEventHandler(ChatHub hub)
        {
            this.hub = hub;
        }

        private string GetGroupName(Ids.Id chatId) {
            return chatId.ToString();
        }

        public Task Visit(ChangedMessageEvent ev)
        {
            return this.hub.Clients.Group(GetGroupName(ev.ChatId)).SendAsync("HandleEvent", ev);
        }

        public Task Visit(CreatedChatEvent ev)
        {
            return this.hub.Clients.All.SendAsync("HandleEvent", ev);
        }

        public Task Visit(CreatedMessageEvent ev)
        {
            return this.hub.Clients.Group(GetGroupName(ev.ChatId)).SendAsync("HandleEvent", ev);
        }

        public Task Visit(RemovedChatEvent ev)
        {
            return this.hub.Clients.All.SendAsync("HandleEvent", ev);
        }

        public Task Visit(RemovedMessageEvent ev)
        {
            return this.hub.Clients.Group(GetGroupName(ev.ChatId)).SendAsync("HandleEvent", ev);
        }

        public async Task Visit(UserJoinEvent ev)
        {
            var connections = this.hub.GetCurrentUserConnections();
            foreach (var connId in connections)
            {
                await this.hub.Groups.AddToGroupAsync(connId, GetGroupName(ev.ChatId));
            }
        }

        public async Task Visit(UserLeaveEvent ev)
        {
            var connections = this.hub.GetCurrentUserConnections();
            foreach (var connId in connections)
            {
                await this.hub.Groups.RemoveFromGroupAsync(connId, GetGroupName(ev.ChatId));
            }
        }
    }
}
