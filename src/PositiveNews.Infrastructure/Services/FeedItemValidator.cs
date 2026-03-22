using System.Xml.Linq;

public class FeedItemValidator : IFeedItemValidator
{
    private static readonly XNamespace ContentNs = "http://purl.org/rss/1.0/modules/content/";
    public bool IsValid(XElement itemElement) //TODO: Additional validation
    {
       var fields = new[] { "title", "link", "description", ContentNs + "encoded" };

        if (fields.Any(t => string.IsNullOrWhiteSpace(itemElement.Element(t)?.Value)))
            return false;

        return true;
    }
}