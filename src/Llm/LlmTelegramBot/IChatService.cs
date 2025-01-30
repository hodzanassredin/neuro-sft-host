using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace LlmTelegramBot
{
    public interface IChatService
    {
        void SaveMessage(long chatId, string senderName, string message);
        List<string> LoadMessages(long chatId);
        void SaveBotMessage(long chatId, string message);
        void ClearChatHistory(long chatId);
    }

    public class ChatService : IChatService
    {
        public class Message
        {
            public DateTime PublicationTime { get; set; }
            public string MessageText { get; set; }
            public string Author { get; set; }
        }

        JsonSerializerOptions jso = new JsonSerializerOptions() { 
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };


        const string folder = "chats";

        private readonly ILogger<ChatService> _logger;
        private readonly object _fileLock = new object();

        public ChatService(ILogger<ChatService> logger)
        {
            _logger = logger;
            Directory.CreateDirectory(folder);
        }

        public void SaveMessage(long chatId, string senderName, string message)
        {
            string fileName = GetPath(chatId);
            lock (_fileLock)
            {
                var msg = new Message { 
                    PublicationTime = DateTime.Now,
                    MessageText = message,
                    Author = senderName,
                };
                string jsonLine = JsonSerializer.Serialize(msg, jso);
                File.AppendAllText(fileName, jsonLine + System.Environment.NewLine);
            }
            _logger.LogInformation("Saved message from {SenderName} in chat {ChatId}", senderName, chatId);
        }

        private static string GetPath(long chatId)
        {
            return $"{folder}/chat_{chatId}.txt";
        }

        public List<string> LoadMessages(long chatId)
        {
            var fileName = GetPath(chatId);
            lock (_fileLock)
            {
                if (!File.Exists(fileName))
                {
                    return new List<string>();
                }
                var lines = File.ReadAllLines(fileName);
                var messages = lines.Select(x=> JsonSerializer.Deserialize<Message>(x).MessageText).ToList();
                _logger.LogInformation("Loaded {MessageCount} messages from chat {ChatId}", messages.Count, chatId);
                return messages;
            }
        }

        public void SaveBotMessage(long chatId, string message)
        {
            SaveMessage(chatId, "BOT", message);
        }

        public void ClearChatHistory(long chatId)
        {
            var fileName = GetPath(chatId);
            lock (_fileLock)
            {
                if (File.Exists(fileName))
                {
                    File.Move(fileName, $"{fileName}.bkp.{DateTime.UtcNow:yyyy-dd-M--HH-mm-ss}");
                }
            }
            _logger.LogInformation("Cleared chat history for chat {ChatId}", chatId);
        }
    }
}
