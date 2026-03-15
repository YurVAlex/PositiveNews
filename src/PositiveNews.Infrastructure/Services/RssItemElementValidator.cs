using System.Xml.Linq;

public class RssItemElementValidator : IRssItemElementValidator
{
    public bool IsValid(XElement itemElement, XNamespace contentNs) //TODO: Additional validation
    {
       var fields = new[] { "title", "link", "description", contentNs + "encoded" };

        if (fields.Any(t => string.IsNullOrWhiteSpace(itemElement.Element(t)?.Value)))
            return false;

        return true;
    }
}