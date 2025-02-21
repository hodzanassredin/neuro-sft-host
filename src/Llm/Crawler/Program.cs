using System.CommandLine;
using System.Text;
using System.Text.Json;

namespace Crawler
{
    internal class Program
    {
        static async Task<int> Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;
            
            // Создаем корневой команду
            var rootCommand = new RootCommand("Загрузчик данных из URL в файл.");

            // Добавляем опции
            var urlOption = new Option<string>(
                name: "--url",
                getDefaultValue : () => "https://forum.oberoncore.ru",
                description: "URL для загрузки данных.")
            {
                IsRequired = false
            };

            var outputOption = new Option<string>(
                name: "--output",
                description: "Файл для сохранения данных.")
            {   
                
                IsRequired = true
            };

            rootCommand.AddOption(urlOption);
            rootCommand.AddOption(outputOption);

            rootCommand.SetHandler(Run, urlOption, outputOption);

            return await rootCommand.InvokeAsync(args);
            // Устанавливаем обработчик команды
        }


        static async Task Run(string url, string filePath)
        {
            var f = new OberonForumCrawler(url);
            JsonSerializerOptions jso = new JsonSerializerOptions();
            jso.Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping;


            await using (var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write))
            await using (var writer = new StreamWriter(fileStream, Encoding.UTF8))
            {
                await foreach (var item in f.Crawl())
                {
                    Console.WriteLine($"{item.SubForumNumber}:{item.SubForum}:{item.Topic}:{item.MessageNumber}");
                    string jsonLine = JsonSerializer.Serialize(item, jso);
                    await writer.WriteLineAsync(jsonLine);
                }
            }
        }
    }
}
