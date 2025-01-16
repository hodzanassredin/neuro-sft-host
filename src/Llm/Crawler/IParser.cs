
using HtmlAgilityPack;

namespace Crawler
{
    public interface IParser
    {
        IEnumerable<string> GetHrefs(string xpath);
        IEnumerable<string> GetHtmls(string xpath, bool decode = true);
        IEnumerable<string> GetItems(string xpath,string attr, bool decode = true);
        IEnumerable<string> GetTexts(string xpath, bool decode = true);
    }

    public class Parser : IParser
    {
        HtmlDocument doc;
        public Parser(string html)
        {
            doc = new HtmlDocument();
            doc.LoadHtml(html);
        }
        public IEnumerable<string> GetItems(string xpath, string attr, bool decode = true)
        {
            foreach(HtmlNode link in doc.DocumentNode.SelectNodes(xpath))
            {
                HtmlAttribute att = link.Attributes[attr];
                if (decode)
                {
                    yield return att.DeEntitizeValue;
                }
                else {
                    yield return att.Value;
                }
            }
        }

        public IEnumerable<string> GetHrefs(string xpath)
        {
            return GetItems(xpath, "href");
        }
        public IEnumerable<string> GetTexts(string xpath, bool decode = true)
        {
            var res = doc.DocumentNode.SelectNodes(xpath);
            if (res != null) { 
                foreach (HtmlNode link in res)
                {
                    if (decode)
                    {
                        yield return HtmlEntity.DeEntitize(link.InnerText);
                    }
                    else
                    {
                        yield return link.InnerText;
                    }
                }
            }
        }

        public IEnumerable<string> GetHtmls(string xpath, bool decode = true)
        {
            var res = doc.DocumentNode.SelectNodes(xpath);
            if (res != null)
            {
                foreach (HtmlNode link in res)
                {
                    if (decode)
                    {
                        yield return HtmlEntity.DeEntitize(link.InnerHtml);
                    }
                    else
                    {
                        yield return link.InnerHtml;
                    }
                }
            }
        }
    }
}
