using LlmCommon;
using LlmCommon.Abstractions;
using LlmCommon.Transport;
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
            return hubConnection.InvokeAsync("ExecCommand", new Envelope(Ids.dir.GenerateId(), cmd));
        }

        public async Task<View> HandleQuery(Query query)
        {
            var e = new Envelope(Ids.dir.GenerateId(), query);
            var res = await hubConnection.InvokeAsync<Envelope>("ExecQuery", e);

            return res.Get<View>();
        }
    }
}
