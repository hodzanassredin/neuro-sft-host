using Microsoft.Extensions.Logging;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types;
using Telegram.Bot;
using Microsoft.Extensions.Hosting;

namespace LllmTelegramBot
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly TelegramBotClient _botClient;
        private readonly ILllmService _llmService;
        private readonly IChatService _chatService;

        public Worker(ILogger<Worker> logger, TelegramBotClient botClient, ILllmService llmService, IChatService chatService)
        {
            _logger = logger;
            _botClient = botClient;
            _llmService = llmService;
            _chatService = chatService;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var me = await _botClient.GetMeAsync();
            _logger.LogInformation("Start listening for @{Username}", me.Username);

            var receiverOptions = new ReceiverOptions
            {
                AllowedUpdates = Array.Empty<UpdateType>() // receive all update types
            };

            _botClient.StartReceiving(
                HandleUpdateAsync,
                HandleErrorAsync,
                receiverOptions,
                stoppingToken
            );

            _logger.LogInformation("Press [Enter] to exit.");
            Console.ReadLine();
        }

        private async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            if (update.Type != UpdateType.Message)
                return;

            var message = update.Message;
            if (message.Type != MessageType.Text)
                return;

            var chatId = message.Chat.Id;
            var text = message.Text;
            var senderName = message.From?.Username ?? message.From?.FirstName ?? "Unknown";

            // Save the message to a file
            _chatService.SaveMessage(chatId, senderName, text);

            // Load all messages from the file
            var allMessages = _chatService.LoadMessages(chatId);

            // Generate a response using the LLM service
            var response = await _llmService.Generate(allMessages);

            // Send the response back to the user
            var sentMessage = await botClient.SendTextMessageAsync(chatId, response, cancellationToken: cancellationToken);

            // Save the bot's response to the file with a special tag
            _chatService.SaveBotMessage(chatId, response);
        }

        private Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            var errorMessage = exception switch
            {
                ApiRequestException apiRequestException => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
                _ => exception.ToString()
            };

            _logger.LogError(exception, errorMessage);
            return Task.CompletedTask;
        }
    }
}
