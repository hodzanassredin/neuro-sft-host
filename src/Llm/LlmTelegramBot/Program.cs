using Telegram.Bot;
using Microsoft.Extensions.Hosting;
using Serilog;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using OpenAI;
using System.ClientModel;
using Microsoft.Extensions.AI;
using System.Text;

namespace LlmTelegramBot
{
    //dotnet user-secrets init
    //dotnet user-secrets set "BOT_TOKEN" "XXX"

    public class Program
    {
        public static void Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;
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
                    builder.AddEnvironmentVariables();
                })
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddLogging(builder =>
                    {
                        builder.SetMinimumLevel(LogLevel.Trace);
                        builder.AddSerilog(logger: Log.Logger, dispose: true);
                    });
                    var llmEndpoint = hostContext.Configuration["OPENAI_ENDPOINT"] ?? "http://127.0.0.1:9001/v1";
                    var chatsFolder = hostContext.Configuration["CHATS_FOLDER"] ?? "chats";
                    services.AddChatClient(b =>
                         new OpenAIClient(new ApiKeyCredential("nokey"), new OpenAI.OpenAIClientOptions { Endpoint = new Uri(llmEndpoint) })
                            .AsChatClient("/models/BlackBox-Coder-3B-F32.gguf")
                            .AsBuilder()
                            .UseLogging()
                            .UseFunctionInvocation()
                            //.UseDistributedCache()
                            //.UseOpenTelemetry(null, sourceName, c => c.EnableSensitiveData = true)
                            .Build(b));
                    services.AddHostedService<Worker>();
                    services.AddSingleton<IChatService, ChatService>(provider => new ChatService(provider.GetRequiredService<ILogger<ChatService>>(), chatsFolder!));
                    services.AddSingleton<ILllmService, LlmService>();
                    services.AddSingleton(provider => new TelegramBotClient(hostContext.Configuration["BOT_TOKEN"]!));
                });
    }
}
