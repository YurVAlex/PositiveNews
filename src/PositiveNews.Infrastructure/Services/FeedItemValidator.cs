using HtmlAgilityPack;
using System.Net;
using System.Xml.Linq;

public class FeedItemValidator : IFeedItemValidator
{
    private static readonly XNamespace ContentNs = "http://purl.org/rss/1.0/modules/content/";
    private static readonly XNamespace DcNs = "http://purl.org/dc/elements/1.1/";

    public bool IsValid(XElement itemElement) //TODO: Additional validation
    {
       var fields = new[] { "title", "link", "description", ContentNs + "encoded" };

        if (fields.Any(t => string.IsNullOrWhiteSpace(itemElement.Element(t)?.Value)))
            return false;

        if (itemElement.Element(DcNs + "creator")?.Value == "tinybuddha")
            return false;

        var html = itemElement.Element(ContentNs + "encoded")!.Value;

        var doc = new HtmlDocument();
        doc.LoadHtml(html);

        if (doc.DocumentNode == null ||
            doc.DocumentNode.InnerHtml == null ||
            string.IsNullOrWhiteSpace(doc.DocumentNode.InnerText) ||
            (doc.DocumentNode.InnerText.Length < 25))
        {
            return false;
        }

        return true;
    }
}