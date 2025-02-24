using LlmBackend.Auth;
using LlmBackend.Hubs;
using LlmBackend.Infrastructure;
using LlmCommon.Abstractions;
using LlmCommon.Implementations;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.EntityFrameworkCore;
using Microsoft.Net.Http.Headers;
using System.Security.Claims;
using Microsoft.Extensions.AI;
using OpenAI;
using System.ClientModel;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;
using OpenTelemetry.Resources;

namespace LlmBackend
{
    public class Program
    {


        private const string ModelId = "cp-lora";

        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            builder.Logging.AddConsole();

            var back = builder.Configuration["BackendUrl"] ?? "https://localhost:5001";
            var front = builder.Configuration["FrontendUrl"] ?? "https://localhost:5002";
            
            var llm = builder.Configuration["LlmEndpoint"] ?? "http://localhost:9001/v1";
            builder.Services.AddCors(
                options => options.AddPolicy(
                    "wasm",
                    policy => policy.WithOrigins([back, front])
                        //.AllowAnyOrigin()
                        .AllowAnyMethod()
                        .SetIsOriginAllowed(pol => true)
                        .AllowAnyHeader()
                        .AllowCredentials()
                        
                        ));

            builder.Services.AddSignalR().AddHubOptions<ChatHub>(options =>
            {
                options.EnableDetailedErrors = true;
            });
            // Add services to the container.
            //builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            //    .AddMicrosoftIdentityWebApi(builder.Configuration.GetSection("AzureAd"));

            // authentication
            var auth = builder.Services.AddAuthentication(options =>
            {
                options.DefaultScheme = "JWT_OR_COOKIE";
                options.DefaultChallengeScheme = "JWT_OR_COOKIE";
            });

            auth.AddPolicyScheme("JWT_OR_COOKIE", "JWT_OR_COOKIE", options =>
            {
                options.ForwardDefaultSelector = context =>
                {
                    string authorization = context.Request.Headers[HeaderNames.Authorization];
                    if (!string.IsNullOrEmpty(authorization) && authorization.StartsWith("Bearer "))
                        return IdentityConstants.BearerScheme;

                    return IdentityConstants.ApplicationScheme;
                };
            });

            auth.AddIdentityCookies();
            auth.AddBearerToken(IdentityConstants.BearerScheme);

            // configure authorization
            builder.Services.AddAuthorizationBuilder();


            // add the database (in memory for the sample)
            builder.Services.AddDbContext<AppDbContext>(options => options.UseNpgsql(builder.Configuration.GetConnectionString("AppDbContext")));
            //builder.Services.AddDbContext<StoreDbContext>(options => options.UseNpgsql(builder.Configuration.GetConnectionString("StoreDbContext")));
            // add identity and opt-in to endpoints
            builder.Services.AddIdentityCore<AppUser>()
                .AddRoles<IdentityRole>()
                .AddEntityFrameworkStores<AppDbContext>()
                .AddApiEndpoints()
                .AddDefaultTokenProviders();


            builder.Services.AddSingleton<IEventBus, SimpleEventBus>();


            builder.Services.AddSingleton<ChatHubEventHandler>();
            builder.Services.AddScoped<IExecutor, Executor>();
            builder.Services.AddScoped<ViewStorage, DbViewsStorage>();
            builder.Services.AddScoped<IUnitOfWork>(c => c.GetRequiredService<AppDbContext>());
            builder.Services.AddScoped<AiManager>();
            builder.Services.AddScoped<IEntityStorage, DbEntityStorage>();

            builder.Services.AddControllers();
            // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
            builder.Services.AddOpenApi();

            builder.Services.AddSingleton<IMetrics, MetricsHost>();

            builder.Services.AddResponseCompression(opts =>
            {
                opts.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(
                    ["application/octet-stream"]);
            });

            //builder.Host.UseOrleans(static siloBuilder =>
            //{
            //    siloBuilder.UseLocalhostClustering()
            //               .AddMemoryGrainStorage("PubSubStore")
            //               .AddMemoryStreams(Constants.ChatsStreamStorage);
            //});
            var sourceName = Guid.NewGuid().ToString();

            builder.Services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = builder.Configuration.GetConnectionString("Cache");
                options.InstanceName = "main";
            });

            builder.Services.AddOpenTelemetry()
                .ConfigureResource(resource => resource
                    .AddService(serviceName: builder.Environment.ApplicationName))
                .WithTracing(builder => {
                    builder.AddSource(sourceName)
                        .AddAspNetCoreInstrumentation()
                        .AddHttpClientInstrumentation()
                        .AddSqlClientInstrumentation()
                        .AddConsoleExporter();
                })
                .WithMetrics(builder =>
                {
                    builder
                        .AddMeter(MetricsHost.Name)
                        //.AddRuntimeInstrumentation()
                        .AddHttpClientInstrumentation()
                        .AddAspNetCoreInstrumentation()
                        .AddSqlClientInstrumentation()
                        .AddPrometheusExporter();
                });

            builder.Services.AddChatClient(b =>
                new OpenAIClient(new ApiKeyCredential("nokey"), new OpenAI.OpenAIClientOptions { Endpoint = new Uri(llm) })
                    .AsChatClient(ModelId)
                    .AsBuilder()
                            .UseLogging()
                            .UseFunctionInvocation()
                            //.UseDistributedCache()
                            .UseOpenTelemetry(null, sourceName, c => c.EnableSensitiveData = true)
                            .Build(b));
            

            builder.Services.AddHostedService<AiService>();
            builder.Services.AddSingleton<ITaskQueue>(ctx =>
            {
                if (!int.TryParse(builder.Configuration["QueueCapacity"], out var queueCapacity))
                    queueCapacity = 100;
                return new DefaultBackgroundTaskQueue(queueCapacity);
            });

            //builder.Services.UseHttpClientMetrics();
            builder.Services.AddHealthChecks()
                .AddCheck<HealthCheck>(nameof(HealthCheck));
            //.ForwardToPrometheus();


            var app = builder.Build();

            app.UseCors("wasm");

            app.UseResponseCompression();
            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
            } 
            app.UseHttpsRedirection();
            //app.UseAuthentication();
            //app.UseAuthorization();



            app.UseOpenTelemetryPrometheusScrapingEndpoint();
            // provide an end point to clear the cookie for logout
            app.MapPost("/Logout", async (ClaimsPrincipal user, SignInManager<AppUser> signInManager) =>
            {
                await signInManager.SignOutAsync();
                return TypedResults.Ok();
            });

            
            app.MapIdentityApi<AppUser>();
            app.MapControllers();
            app.MapHub<ChatHub>("/chathub");
            //app.UseHttpMetrics(options =>
            //{
            //    // This will preserve only the first digit of the status code.
            //    // For example: 200, 201, 203 -> 2xx
            //    options.ReduceStatusCodeCardinality();
            //    options.AddCustomLabel("host", context => context.Request.Host.Host);

            //});


            //app.MapMetrics();
                //.RequireAuthorization("ReadMetrics");
            using (var scope = app.Services.CreateScope())
            {
                await Auth.SeedData.InitializeAsync(scope.ServiceProvider);
            }
            var eventBus = app.Services.GetRequiredService<IEventBus>();
            var handler = app.Services.GetRequiredService<ChatHubEventHandler>();

            eventBus.Subscribe(handler);

            Registrator.Register(app.Services);
            System.Console.WriteLine($"frontend {front}");
            app.Run();
        }
    }
}
