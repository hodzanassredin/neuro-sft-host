using LlmFrontend.Identity;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

namespace LlmFrontend
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);
            builder.RootComponents.Add<App>("#app");
            builder.RootComponents.Add<HeadOutlet>("head::after");



            var front = builder.Configuration["FrontendUrl"] ?? "https://localhost:5002";
            var back = builder.Configuration["BackendUrl"] ?? "https://localhost:5001";
            builder.Services.AddScoped(sp =>
                new HttpClient { BaseAddress = new Uri(front) });
            // register the cookie handler
            builder.Services.AddScoped<CookieHandler>();
            // configure client for auth interactions
            builder.Services.AddHttpClient(
                "Auth",
                opt => opt.BaseAddress = new Uri(back))
                .AddHttpMessageHandler<CookieHandler>();

            

            // set up authorization
            builder.Services.AddAuthorizationCore();

            // register the custom state provider
            builder.Services.AddScoped<AuthenticationStateProvider, CookieAuthenticationStateProvider>();

            // register the account management interface
            builder.Services.AddScoped(
                sp => (IAccountManagement)sp.GetRequiredService<AuthenticationStateProvider>());


            await builder.Build().RunAsync();
        }
    }
}
