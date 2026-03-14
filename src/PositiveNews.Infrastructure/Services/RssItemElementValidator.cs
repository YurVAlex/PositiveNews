using System.Xml.Linq;

public class RssItemElementValidator : IRssItemElementValidator
{
    public bool IsValid(XElement itemElement) //TODO: Additional validation add
    {
        if (string.IsNullOrWhiteSpace(itemElement.Element("title")?.Value))
        {
            return false;
        }

        if (string.IsNullOrWhiteSpace(itemElement.Element("link")?.Value))
        {
            return false;
        }

        

        return true;
    }
}