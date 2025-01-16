
namespace Crawler
{
    public interface ICrawler<T>
    {
        IAsyncEnumerable<T> Crawl();
    }
}
