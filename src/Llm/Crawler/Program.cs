using System.Text;
using System.Text.Json;

namespace Crawler
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;
            var f = new OberonForumCrawler();

            string filePath = "frorum.json";

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

            Console.WriteLine("JSONL file written successfully.");
        }
    }
}
