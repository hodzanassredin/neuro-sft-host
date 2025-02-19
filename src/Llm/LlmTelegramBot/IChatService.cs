using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace LlmTelegramBot
{
    public class Message
    {
        public DateTime PublicationTime { get; set; }
        public string MessageText { get; set; }
        public string Author { get; set; }
    }
    public interface IChatService
    {
        void SaveMessage(long chatId, Message msg);
        List<Message> LoadMessages(long chatId);
        void ClearChatHistory(long chatId);
    }

    public class ChatService : IChatService
    {


        JsonSerializerOptions jso = new JsonSerializerOptions() {
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };




        private readonly ILogger<ChatService> _logger;
        private readonly string folder;
        private readonly object _fileLock = new object();

        public ChatService(ILogger<ChatService> logger, string folder)
        {
            _logger = logger;
            this.folder = folder;
            Directory.CreateDirectory(folder);
        }

        public void SaveMessage(long chatId, Message message)
        {
            string fileName = GetPath(chatId);
            lock (_fileLock)
            {
                string jsonLine = JsonSerializer.Serialize(message, jso);
                File.AppendAllText(fileName, jsonLine + System.Environment.NewLine);
            }
            _logger.LogInformation("Saved message from {SenderName} in chat {ChatId}", message.Author, chatId);
        }

        private string GetPath(long chatId)
        {
            return $"{folder}/chat_{chatId}.txt";
        }

        public List<Message> LoadMessages(long chatId)
        {
            var fileName = GetPath(chatId);
            lock (_fileLock)
            {
                if (!File.Exists(fileName))
                {
                    return new List<Message>();
                }
                var lines = File.ReadAllLines(fileName);
                var messages = lines.Select(x=> JsonSerializer.Deserialize<Message>(x)).ToList();
                _logger.LogInformation("Loaded {MessageCount} messages from chat {ChatId}", messages.Count, chatId);
                return messages;
            }
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
