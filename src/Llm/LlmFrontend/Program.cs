using LlmCommon;
using LlmCommon.Abstractions;
using LlmCommon.Implementations;
using LlmCommon.Transport;
using LlmFrontend.Identity;
using LlmFrontend.Infrastructure;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.AspNetCore.SignalR.Client;
using System.Diagnostics;

namespace LlmFrontend
{
    public class Program
    {
        public static async Task Main(string[] args)
        {


            var builder = WebAssemblyHostBuilder.CreateDefault(args);

            var front = builder.Configuration["FrontendUrl"] ?? "https://localhost:5002";
            var back = builder.Configuration["BackendUrl"] ?? "https://localhost:5001";

            builder.RootComponents.Add<App>("#app");
            builder.RootComponents.Add<HeadOutlet>("head::after");

            builder.Services.AddScoped<CookieHandler>();

            // set up authorization
            builder.Services.AddAuthorizationCore();

            // register the custom state provider
            builder.Services.AddScoped<AuthenticationStateProvider, CookieAuthenticationStateProvider>();
            builder.Services.AddSingleton<IEventBus, SimpleEventBus>();
            builder.Services.AddSingleton<AppState>();

            // register the account management interface
            builder.Services.AddScoped(
                sp => (IAccountManagement)sp.GetRequiredService<AuthenticationStateProvider>());
            builder.Services.AddSingleton(sp =>
            {
                //var navMan = sp.GetRequiredService<NavigationManager>();
                //var accessTokenProvider = sp.GetRequiredService<IAccessTokenProvider>();
                var hubConnection = new HubConnectionBuilder()
                    //.WithUrl(navMan.ToAbsoluteUri("/hub"), options =>
                    //{
                    //    options.AccessTokenProvider = async () =>
                    //    {
                    //        var accessTokenResult = await accessTokenProvider.RequestAccessToken();
                    //        accessTokenResult.TryGetToken(out var accessToken);
                    //        return accessToken.Value;
                    //    };
                    //})
                    .WithUrl(back + "/chathub",
                        options => {
                            options.HttpMessageHandlerFactory = innerHandler => new CookieHandler { InnerHandler = innerHandler };
                        })
                    .WithAutomaticReconnect()
                    .Build();

                hubConnection.On<Envelope>("HandleEvent", envelope =>
                {
                    var logger = sp.GetRequiredService<ILogger<HubConnection>>();
                    var eventBus = sp.GetRequiredService<IEventBus>();
                    try
                    {
                        var ev = envelope.Get<Event>();
                        Debug.Assert(ev != null);
                        eventBus.Publish(ev);
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, $"Cant proccess event from eventhub {envelope.CorellationId}");

                        throw;
                    }
                });
                return hubConnection;
            });
            builder.Services.AddSingleton<IRequestHandler, SignalRRequestHandler>();


            builder.Services.AddScoped(sp =>
                new HttpClient { BaseAddress = new Uri(front) });
            // configure client for auth interactions
            builder.Services.AddHttpClient(
                "Auth",
                opt => opt.BaseAddress = new Uri(back))
                .AddHttpMessageHandler<CookieHandler>();

            await builder.Build().RunAsync();
        }
    }
}
