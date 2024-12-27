using LlmCommon;
using LlmCommon.Abstractions;
using Microsoft.AspNetCore.SignalR.Client;

namespace LlmFrontend.Infrastructure
{
    public class SignalRRequestHandler : IRequestHandler
    {
        private readonly HubConnection hubConnection;

        public SignalRRequestHandler(HubConnection hubConnection)
        {
            this.hubConnection = hubConnection;
        }

        public bool IsConnected => hubConnection?.State == HubConnectionState.Connected;

        public Task Handle(Command cmd)
        {
            return hubConnection.InvokeAsync("ExecCommand", cmd);
        }

        public Task<TV> HandleQuery<TV>(Query<TV> query) where TV : View
        {
            return hubConnection.InvokeAsync<TV>("ExecQuery", query);
        }
    }
}
