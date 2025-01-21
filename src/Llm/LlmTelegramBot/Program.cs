using Telegram.Bot;
using Microsoft.Extensions.Hosting;
using Serilog;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;

namespace LlmTelegramBot
{
    //dotnet user-secrets init
    //dotnet user-secrets set "BOT_TOKEN" "XXX"

    public class Program
    {
        public static void Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .WriteTo.Console()
                .CreateLogger();

            try
            {
                Log.Information("Starting up");
                CreateHostBuilder(args).Build().Run();
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Application start-up failed");
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration(builder=>{
                    builder.AddUserSecrets(typeof(Program).Assembly);
                })
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddLogging(builder =>
                    {
                        builder.SetMinimumLevel(LogLevel.Trace);
                        builder.AddSerilog(logger: Log.Logger, dispose: true);
                    });
                    services.AddHostedService<Worker>();
                    services.AddSingleton<IChatService, ChatService>();
                    services.AddSingleton<ILllmService, LlmService>();
                    services.AddSingleton(provider => new TelegramBotClient(hostContext.Configuration["BOT_TOKEN"]!));
                });
    }
}
