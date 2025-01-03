using LlmBackend.Auth;
using LlmBackend.Hubs;
using LlmBackend.Infrastructure;
using LlmCommon.Abstractions;
using LlmCommon.Entities;
using LlmCommon.Implementations;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.EntityFrameworkCore;
using Microsoft.Net.Http.Headers;
using System.Security.Claims;
using Microsoft.Extensions.AI;
using OpenAI;
using NuGet.Configuration;
using System.ClientModel;

namespace LlmBackend
{
    public class Program
    {
        
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
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
            builder.Services.AddSingleton<IEntityStorage<ChatEntity>, InMemoryEntityStorage<ChatEntity>>();

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
            builder.Services.AddScoped<AiManager>();

            builder.Services.AddSingleton<ChatHubEventHandler>();
            builder.Services.AddSingleton<IExecutor, Executor>();
            builder.Services.AddSingleton<IViewStorage, Executor>();
            





            builder.Services.AddControllers();
            // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
            builder.Services.AddOpenApi();

            

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

            builder.Services.AddChatClient(b => 
                new OpenAIClient(new ApiKeyCredential("asd"), new OpenAI.OpenAIClientOptions { Endpoint = new Uri(llm) })
                    .AsChatClient("/models/toxic_sft_cotype/merged")
                    .AsBuilder()
                            .UseLogging()
                            //.UseFunctionInvocation()
                            //.UseDistributedCache()
                            //.UseOpenTelemetry()
                            .Build(b));


            var app = builder.Build();



            app.UseResponseCompression();
            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
            }

            app.UseHttpsRedirection();
            
            //app.UseAuthentication();
            //app.UseAuthorization();


            

            // provide an end point to clear the cookie for logout
            app.MapPost("/Logout", async (ClaimsPrincipal user, SignInManager<AppUser> signInManager) =>
            {
                await signInManager.SignOutAsync();
                return TypedResults.Ok();
            });

            app.UseCors("wasm");
            app.MapIdentityApi<AppUser>();
            app.MapControllers();
            app.MapHub<ChatHub>("/chathub");
            
            

            using (var scope = app.Services.CreateScope())
            {
                await Auth.SeedData.InitializeAsync(scope.ServiceProvider);
            }
            var eventBus = app.Services.GetRequiredService<IEventBus>();
            var handler = app.Services.GetRequiredService<ChatHubEventHandler>();

            eventBus.Subscribe(handler);
            app.Run();
        }
    }
}
