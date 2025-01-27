using Microsoft.Extensions.AI;
namespace LlmTelegramBot
{
    public interface ILllmService
    {
        Task<string> Generate(List<string> msgs);
    }

    public class LlmService : ILllmService
    {
        private readonly string System = @"
Ты помощник для работы в системе BlackBox с использованием языка Component Pascal. Твоя задача информативно отвечать на вопросы. 
Ответ необходимо предоставить в формате markdown и выделять код символами ```. 
        ";

        private readonly IChatClient client;

        public LlmService(IChatClient client)
        {
            this.client = client;
        }
        ChatOptions chatOptions = new ChatOptions()
        {
            Temperature = 0.4f,
            TopP = 0.3f,
            ResponseFormat = ChatResponseFormatText.Text
        };
        public async Task<string> Generate(List<string> msgs)
        {
            var inpmsgs = await MapMsgs(msgs, System);
            var res = await this.client.CompleteAsync(inpmsgs, chatOptions);
            return res.Message.Text??"";
        }

        private async Task<List<ChatMessage>> MapMsgs(IEnumerable<string> msgsRaw, string? system)
        {
            var msgs = msgsRaw.Select(x => new ChatMessage()
            {
                Text = x,
                Role = ChatRole.User
            }).ToList();
            if (!String.IsNullOrWhiteSpace(system))
            {
                msgs.Insert(0, new ChatMessage
                {
                    Role = ChatRole.System,
                    Text = system,
                });
            }
            return msgs;
        }
    }
}
