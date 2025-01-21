using Microsoft.Extensions.AI;
using System.IO;
using System.Text;
using System.Text.Json;

namespace DatasetGenerator
{
    internal class Program
    {
        private static readonly string DatasetsPath = Path.GetFullPath(Path.Combine(Environment.CurrentDirectory,"../../../../../../datasets"));

        static IChatClient GetOllama(string model) {
            return new OllamaChatClient("http://127.0.0.1:11434", modelId: model);
        }
        //static IChatClient GetOpenAI(string model)
        //{
        //    return new OpenAIClient(new ApiKeyCredential("nokey"), new OpenAI.OpenAIClientOptions { Endpoint = new Uri("https://127.0.0.1:9999/v1") })
        //            .AsChatClient(model);
        //}

        const string model = "qwen2.5-coder:7b";
        static string GetSub(string path, string dir)
        {
            Uri fromUri = new Uri(path);
            Uri toUri = new Uri(dir);

            Uri relativeUri = toUri.MakeRelativeUri(fromUri);
            string relativePath = Uri.UnescapeDataString(relativeUri.ToString().Replace('/', '\\'));
            return relativePath;
        }
        static async Task Main(string[] args)
        {
            var name = "bb_ru";

            var inPath = Path.Combine(DatasetsPath, "oberon", "docs", name);
            var outPath = Path.Combine(DatasetsPath, "oberon", "qa", name, $"questions-{model.Replace(":","_")}-{DateTime.UtcNow:yyyy-dd-M--HH-mm-ss}.json");
            Directory.CreateDirectory(Path.GetDirectoryName(outPath));


            Console.OutputEncoding = Encoding.UTF8;
            JsonSerializerOptions jso = new JsonSerializerOptions();
            jso.Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping;

            IChatClient client = GetOllama(model);

            var gen = new AiDialogGenerator(client);
            var chunker = new LineChunker(30);
            

            string subFolder = String.Empty;// "./Docu";
            var texts = new RecursiveDirDataset(Path.Combine(inPath, subFolder));

            await using (var fileStream = new FileStream(outPath, FileMode.Create, FileAccess.Write))
            await using (var writer = new StreamWriter(fileStream, Encoding.UTF8))
            {
                foreach (var textDto in texts.GetTexts())
                {
                    var sub = GetSub(textDto.TextSource, inPath + Path.DirectorySeparatorChar);


                    var chunks = chunker.GetChunks(textDto.Text);
                    foreach (var chunk in chunks) {
                        var dialogs = await gen.GetDialogs(chunk, sub);

                        foreach (var dialog in dialogs)
                        {
                            Console.Write(dialog.Source + ": ");
                            Console.WriteLine(dialog.Question);
                            Console.WriteLine(">>>");
                            Console.WriteLine(dialog.Answer);
                            Console.WriteLine("\n======================================================\n");
                            string jsonLine = JsonSerializer.Serialize(dialog, jso);
                            await writer.WriteLineAsync(jsonLine);
                        }
                        writer.Flush();
                        fileStream.Flush();
                    }
                    //var tasks = chunks.Select(chunk => gen.GetDialogs(chunk, sub)).ToList();
                    //await Task.WhenAll(tasks);
                    //var results = tasks.Select(x => x.Result);


                    


                }
            }
        }
    }
}
