using LlmBackend.Hubs;
using LlmCommon;
using LlmCommon.Abstractions;
using LlmCommon.Events;
using LlmCommon.Transport;

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
            await this.hub.Clients.Group(GetGroupName(ev.ChatId)).HandleEvent(new Envelope(Ids.dir.GenerateId(), ev));
            return true;
        }

        public async Task<bool> Visit(CreatedChatEvent ev)
        {
            await this.hub.Clients.All.HandleEvent(new Envelope(Ids.dir.GenerateId(), ev));
            await AddCurrentUserToGroup(ev.ChatId);
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
            await AddCurrentUserToGroup(ev.ChatId);
            return true;
        }

        private async Task AddCurrentUserToGroup(Ids.Id chatId)
        {
            var connections = this.hub.GetCurrentUserConnections();
            foreach (var connId in connections)
            {
                await this.hub.Groups.AddToGroupAsync(connId, GetGroupName(chatId));
            }
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

        public async Task<bool> Visit(ChangedChatEvent ev)
        {
            await this.hub.Clients.All.HandleEvent(new Envelope(Ids.dir.GenerateId(), ev));
            return true;
        }
    }
}
