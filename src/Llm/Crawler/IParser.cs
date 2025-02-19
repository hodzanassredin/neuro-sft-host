
using HtmlAgilityPack;

namespace Crawler
{
    public interface IParser
    {
        IEnumerable<string> GetHrefs(string xpath);
        IEnumerable<string> GetHtmls(string xpath, bool decode = true);
        IEnumerable<string> GetItems(string xpath,string attr, bool decode = true);
        IEnumerable<string> GetTexts(string xpath, bool decode = true);

        IEnumerable<string> GetMarkdowns(string xpath);
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
        static string ConvertHtmlToMarkdown(HtmlNode node)
        {
            if (node.NodeType == HtmlNodeType.Text)
            {
                return HtmlEntity.DeEntitize(node.InnerText).Trim();
            }

            if (node.Name == "br")
            {
                return "\n";
            }

            if (node.Name == "div" && node.GetAttributeValue("class", "") == "codetitle")
            {
                return $"## {HtmlEntity.DeEntitize(node.InnerText).Trim()}\n";
            }

            if (node.Name == "div" && node.GetAttributeValue("class", "") == "quotetitle")
            {
                return $"> -- {HtmlEntity.DeEntitize(node.InnerText).Trim()}\n";
            }
            if (node.Name == "div" && node.GetAttributeValue("class", "") == "quotecontent")
            {
                var lines = HtmlEntity.DeEntitize(node.GetDirectInnerText()).Trim().Split('\n');
                var quote = "> " + String.Join("\n> ", lines);
                return quote;
            }
            if (node.Name == "div" && node.GetAttributeValue("class", "") == "codecontent")
            {
                string content = HtmlEntity.DeEntitize(node.InnerHtml)
                    .Replace("<br>", "\n")
                    .Replace("&nbsp;", " ")
                    .Replace("&lt;", "<")
                    .Replace("&gt;", ">")
                    .Trim();
                return $"```\n{content}\n```\n";
            }

            string result = "";
            foreach (var child in node.ChildNodes)
            {
                result += ConvertHtmlToMarkdown(child);
            }
            return result;
        }
        public IEnumerable<string> GetMarkdowns(string xpath)
        {
            var res = doc.DocumentNode.SelectNodes(xpath);
            if (res != null)
            {
                foreach (HtmlNode node in res)
                {
                     yield return ConvertHtmlToMarkdown(node);

                }
            }
        }
    }
}
