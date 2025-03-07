﻿using Microsoft.Extensions.Logging;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types;
using Telegram.Bot;
using Microsoft.Extensions.Hosting;
using Telegram.Bot.Types.ReplyMarkups;

namespace LlmTelegramBot
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
            var me = await _botClient.GetMe();
            var commands = new List<BotCommand>
            {
                new BotCommand { Command = "reset", Description = "Reset chat history" }
            };
            await _botClient.SetMyCommands(commands, cancellationToken: stoppingToken);
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
            if (update.Type != UpdateType.Message && update.Type != UpdateType.CallbackQuery)
                return;

            if (update.Type == UpdateType.Message)
            {
                try
                {
                    var message = update.Message;
                    if (message.Type != MessageType.Text)
                        return;

                    var chatId = message.Chat.Id;
                    var text = message.Text;
                    var senderName = message.From?.Username ?? message.From?.FirstName ?? "Unknown";

                    if (text.Equals("/reset", StringComparison.OrdinalIgnoreCase))
                    {
                        // Clear the chat history
                        _chatService.ClearChatHistory(chatId);

                        // Notify the user that the chat history has been cleared
                        await botClient.SendMessage(chatId, "Chat history has been cleared.", cancellationToken: cancellationToken);
                    }
                    else if (text.Equals("/start", StringComparison.OrdinalIgnoreCase))
                    {
                        // Notify the user that the chat history has been cleared
                        await botClient.SendMessage(chatId, "Привет. Я помощник по системе BlackBox. Задайте ваш вопрос.", cancellationToken: cancellationToken);
                    }
                    else
                    {
                        // Save the message to a file
                        _chatService.SaveMessage(chatId, new Message { PublicationTime = DateTime.UtcNow, Author = senderName, MessageText = text });

                        // Load all messages from the file
                        var allMessages = _chatService.LoadMessages(chatId);

                        // Generate a response using the LLM service
                        var response = await _llmService.Generate(allMessages);

                        // Send the response back to the user with a Reset button
                        IReplyMarkup? replyMarkup = null;
                        //new InlineKeyboardMarkup(new[]
                        //{
                        //    new[]
                        //    {
                        //        InlineKeyboardButton.WithCallbackData("Reset Chat", "reset_chat")
                        //    }
                        //});

                        await botClient.SendMessage(chatId, response, replyMarkup: replyMarkup, cancellationToken: cancellationToken);

                        // Save the bot's response to the file with a special tag
                        _chatService.SaveMessage(chatId, new Message { PublicationTime = DateTime.UtcNow, Author = Consts.botName, MessageText = response });
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Cant proccess message");
                    if (update.Message != null)
                    {
                        await botClient.SendMessage(update.Message.Chat.Id, "ERROR: try reset chat", replyMarkup: null, cancellationToken: cancellationToken);
                    }
                }

            }
            //else if (update.Type == UpdateType.CallbackQuery)
            //{
            //    var callbackQuery = update.CallbackQuery;
            //    var chatId = callbackQuery.Message.Chat.Id;
            //    var data = callbackQuery.Data;

            //    if (data == "reset_chat")
            //    {
            //        // Clear the chat history
            //        _chatService.ClearChatHistory(chatId);

            //        // Notify the user that the chat history has been cleared
            //        await botClient.SendMessage(chatId, "Chat history has been cleared.", cancellationToken: cancellationToken);
            //    }
            //}
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
