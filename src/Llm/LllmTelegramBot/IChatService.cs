using Microsoft.Extensions.Logging;

namespace LllmTelegramBot
{
    public interface IChatService
    {
        void SaveMessage(long chatId, string senderName, string message);
        List<string> LoadMessages(long chatId);
        void SaveBotMessage(long chatId, string message);
    }

    public class ChatService : IChatService
    {
        private readonly ILogger<ChatService> _logger;
        private readonly object _fileLock = new object();

        public ChatService(ILogger<ChatService> logger)
        {
            _logger = logger;
        }

        public void SaveMessage(long chatId, string senderName, string message)
        {
            var fileName = $"chat_{chatId}.txt";
            lock (_fileLock)
            {
                File.AppendAllText(fileName, $"{DateTime.Now}: {senderName}: {message}\n");
            }
            _logger.LogInformation("Saved message from {SenderName} in chat {ChatId}", senderName, chatId);
        }

        public List<string> LoadMessages(long chatId)
        {
            var fileName = $"chat_{chatId}.txt";
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
            var fileName = $"chat_{chatId}.txt";
            lock (_fileLock)
            {
                File.AppendAllText(fileName, $"{DateTime.Now}: Bot: {message}\n");
            }
            _logger.LogInformation("Saved bot message in chat {ChatId}", chatId);
        }
    }
}
