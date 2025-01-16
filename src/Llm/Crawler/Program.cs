namespace Crawler
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            var f = new OberonForumCrawler();
            await foreach (var item in f.Crawl())
            {
                Console.WriteLine(item.Content);
            }
        }
    }
}
