
using Microsoft.Extensions.AI;
using System.Text.Json;

namespace DatasetGenerator
{
    public interface IDialogGenerator
    {
        Task<IEnumerable<Dialog>> GetDialogs(string text, string source);
    }

    public class AiDialogGenerator : IDialogGenerator
    {
        private readonly IChatClient client;
        const string codeStart = "```json";
        const string codeEnd = "```";


        public AiDialogGenerator(IChatClient client)
        {
            this.client = client;
        }
        static JsonSerializerOptions options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        };
        public async Task<IEnumerable<Dialog>> GetDialogs(string text, string source)
        {
            var msgs = new List<ChatMessage>() { 
                new ChatMessage(ChatRole.System, genPrompt),
                new ChatMessage(ChatRole.User, $"Путь:\n {source}\nТекст:\n {text} "),
            };

            var opts = new ChatOptions { 
                
            };

            var res = await client.CompleteAsync(msgs);
            if (res.FinishReason == ChatFinishReason.Stop && res.Choices.Count > 0)
            {
                var resText = res.Choices[0].Text;

                if (resText.StartsWith(codeStart) && resText.EndsWith(codeEnd))
                {
                    resText = resText.Substring(codeStart.Length, resText.Length - codeStart.Length - codeEnd.Length);
                    try
                    {
                        var dialog = JsonSerializer.Deserialize<Dialog[]>(resText, options);
                        foreach (var item in dialog)
                        {
                            item.Source = source;
                        }
                        return dialog;
                    }
                    catch (Exception ex)
                    {

                    }
                    
                }

            }

            return Enumerable.Empty<Dialog>();

        }
        
        private string genPrompt = @"

На основе следующего текста сгенерируй пары вопросов и ответов похожие на те какие бы задал новичок который не знает как пользоваться системой, а ответ должен быть продробным, как если бы отвечал эксперт. 
Все тексты описывают язык Component Pascal и систему BlackBox. Не путай Component Pascal с другими диалектами Pascal и Delphi.
Ответ должен быть представлен в формате JSON, где каждая пара вопрос-ответ будет отдельным объектом в массиве. Также тебе будет дано путь до документа. Можешь использовать его как контекст.
Ответ должен быть в формате маркдаун и может содержать фрагменты кода помеченные в начале и конце блоком ```
Пример: 
**Путь:**
Text/Docu/Main.odc
**Текст:**

""Париж — столица Франции, известная своими историческими достопримечательностями, такими как Эйфелева башня и Лувр. Город также славится своей кухней, включая круассаны и эклеры. Париж является важным культурным и экономическим центром Европы.""

**Формат ответа:**

```json
[
  {
    ""question"": ""Какая столица Франции?"",
    ""answer"": ""Париж""
  },
  {
    ""question"": ""Какие известные достопримечательности находятся в Париже?"",
    ""answer"": ""Эйфелева башня и Лувр""
  },
  {
    ""question"": ""Чем славится кухня Парижа?"",
    ""answer"": ""Круассанами и эклерами""
  },
  {
    ""question"": ""Какую роль играет Париж в Европе?"",
    ""answer"": ""Важный культурный и экономический центр""
  }
]
```
";
    }
}
