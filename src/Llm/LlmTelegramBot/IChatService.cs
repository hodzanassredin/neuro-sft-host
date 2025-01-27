using Microsoft.Extensions.Logging;

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
                File.AppendAllText(fileName, $"{DateTime.Now}: {senderName}: {message}\n");
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

                var messages = File.ReadAllLines(fileName).ToList();
                _logger.LogInformation("Loaded {MessageCount} messages from chat {ChatId}", messages.Count, chatId);
                return messages;
            }
        }

        public void SaveBotMessage(long chatId, string message)
        {
            var fileName = GetPath(chatId);
            lock (_fileLock)
            {
                File.AppendAllText(fileName, $"{DateTime.Now}: Bot: {message}\n");
            }
            _logger.LogInformation("Saved bot message in chat {ChatId}", chatId);
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
