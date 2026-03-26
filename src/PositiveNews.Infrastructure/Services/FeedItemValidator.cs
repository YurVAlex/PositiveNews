using HtmlAgilityPack;
using System.Xml.Linq;

public class FeedItemValidator : IFeedItemValidator
{
    private static readonly XNamespace ContentNs = "http://purl.org/rss/1.0/modules/content/";
    public bool IsValid(XElement itemElement) //TODO: Additional validation
    {
       var fields = new[] { "title", "link", "description", ContentNs + "encoded" };

        if (fields.Any(t => string.IsNullOrWhiteSpace(itemElement.Element(t)?.Value)))
            return false;

        var html = itemElement.Element(ContentNs + "encoded")!.Value;

        var doc = new HtmlDocument();
        doc.LoadHtml(html);

        if (doc.DocumentNode == null ||
            doc.DocumentNode.InnerHtml == null ||
            string.IsNullOrWhiteSpace(doc.DocumentNode.InnerText))
        {
            return false;
        }

        return true;
    }
}