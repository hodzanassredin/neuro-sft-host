
namespace Crawler
{
    public interface IWebClient
    {
        Task<IParser> Crawl(string path);
    }

    public class Client : IWebClient
    {
        HttpClient client;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="baseAddress">like "https://jsonplaceholder.typicode.com"</param>
        public Client(String baseAddress)
        {
            client = new()
            {
                BaseAddress = new Uri(baseAddress)
            };
        }
        public async Task<IParser> Crawl(string path)
        {
            using HttpResponseMessage response = await client.GetAsync(path);

            response.EnsureSuccessStatusCode();

            var resp = await response.Content.ReadAsStringAsync();
            return new Parser(resp);
        }
    }
}
