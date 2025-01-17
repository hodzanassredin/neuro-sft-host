using System.Web;
using System;
using static Crawler.OberonForumCrawler;
using System.Text.RegularExpressions;

namespace Crawler
{
    public class OberonForumCrawler : ICrawler<Message>
    {
        public class Message {
            public int SubForumNumber { get; set; }
            public string SubForum { get; set; }
            public int TopicNumber { get; set; }
            public string Topic { get; set; }
            public int MessageNumber { get; set; }
            public string Author { get; set; }
            public string Content { get; set; }
        }

        private readonly IWebClient client;

        public OberonForumCrawler()
        {
            this.client = new Client("https://forum.oberoncore.ru");
        }

        Regex topicCountRegex = new Regex(@"Тем:\s*(\d+)");
        Regex numberRegex = new Regex(@"\d+");
        private int ExtractNumber(string str) {

            Match match = numberRegex.Match(str);

            if (match.Success)
            {
                return int.Parse(match.Value);
            }
            throw new Exception("Cant parse");
        }

        public async IAsyncEnumerable<Message> Crawl() {
            for (int itemNumber = 1; itemNumber < 123; itemNumber++)
            {
                var forumStartPage = await client.CrawlIgnoreError($"/viewforum.php?f={itemNumber}");
                if (forumStartPage == null) {
                    continue;
                }
                var itemName = forumStartPage.GetTexts("//*[@id=\"pageheader\"]/h2/a").FirstOrDefault();
                if (itemName == default) continue;
                var tmp = forumStartPage.GetTexts("//*[@id=\"pagecontent\"]/table[1]/tr/td[3]", decode:false).First();
                var tmp2 = topicCountRegex.Match(tmp).Groups[1].Value;

                int topicCount = int.Parse(tmp2);
                for (int itemStart = 0; itemStart < topicCount; itemStart += 50)
                {
                    var topicsPage = await client.Crawl($"/viewforum.php?f={itemNumber}&start={itemStart}");
                    foreach (var topicLinkTmp in topicsPage.GetHrefs("//a[@class=\"topictitle\"]/@href"))
                    {
                        var topicLink = topicLinkTmp.Substring(1);
                        var q = new Uri("http://localhost" + topicLink).Query;
                        var topicNumber = Int32.Parse(HttpUtility.ParseQueryString(q).Get("t"));
                        var topicFirstPage = await client.Crawl(topicLink);
                        var topicName = topicFirstPage.GetTexts("//*[@id=\"pageheader\"]/h2/a").First();
                        int messageCount = ExtractNumber(topicFirstPage.GetTexts("//*[@id=\"pagecontent\"]/table[1]/tr/td[3]").First());


                        for (int messagesStart = 0; messagesStart < messageCount; messagesStart += 20)
                        {
                            var topicPage = await client.Crawl($"/viewtopic.php?f={itemNumber}&t={topicNumber}&start={messagesStart}");
                            var messageAuthors = topicFirstPage.GetTexts("//b[contains(@class, \"postauthor\")]");
                            var messageContents = topicFirstPage.GetMarkdowns("//div[@class=\"postbody\"]");
                            var messages = messageAuthors.Zip(messageContents).ToList();
                            for (int msgShift = 0; msgShift < messages.Count; msgShift++)
                            {
                                yield return new Message
                                {
                                    SubForum = itemName,
                                    SubForumNumber = itemNumber,
                                    Topic = topicName,
                                    TopicNumber = topicNumber,
                                    MessageNumber = messagesStart + msgShift,
                                    Author = messages[msgShift].First,
                                    Content = messages[msgShift].Second,
                                };
                            }
                            
                        }
                    }
                }
            }
            
            
        }
    }
}
