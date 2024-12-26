using LlmFrontend.Identity;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.AspNetCore.SignalR.Client;

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

            // register the account management interface
            builder.Services.AddScoped(
                sp => (IAccountManagement)sp.GetRequiredService<AuthenticationStateProvider>());
            builder.Services.AddScoped(sp =>
            {
                //var navMan = sp.GetRequiredService<NavigationManager>();
                //var accessTokenProvider = sp.GetRequiredService<IAccessTokenProvider>();
                return new HubConnectionBuilder()
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
            });

            
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
