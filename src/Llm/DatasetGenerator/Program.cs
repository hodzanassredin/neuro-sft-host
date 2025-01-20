using Microsoft.Extensions.AI;
using System;
using System.IO;
using System.Text;
using System.Text.Json;

namespace DatasetGenerator
{
    internal class Program
    {
        private static readonly string DatasetsPath = Path.GetFullPath(Path.Combine(Environment.CurrentDirectory,"../../../../../../datasets"));

        const string model = "qwen2.5-coder:3b";
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
            Console.OutputEncoding = Encoding.UTF8;
            JsonSerializerOptions jso = new JsonSerializerOptions();
            jso.Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping;

            IChatClient client = new OllamaChatClient("http://127.0.0.1:11434", modelId: model);

            var gen = new AiDialogGenerator(client);
            var chunker = new LineChunker(30);
            var dir = Path.Combine(DatasetsPath, "oberon", "bb");

            string subFolder = "./Docu";
            var texts = new RecursiveDirDataset(Path.Combine(dir, subFolder));

            await using (var fileStream = new FileStream(Path.Combine(DatasetsPath, $"./oberon/questions-{model}.json"), FileMode.Create, FileAccess.Write))
            await using (var writer = new StreamWriter(fileStream, Encoding.UTF8))
            {
                foreach (var textDto in texts.GetTexts())
                {
                    var chunks = chunker.GetChunks(textDto.Text);
                    foreach (var chunk in chunks) {
                        var dialogs = await gen.GetDialogs(chunk, GetSub(textDto.TextSource, dir + Path.DirectorySeparatorChar));
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
                    }

                    
                }
            }
        }
    }
}
