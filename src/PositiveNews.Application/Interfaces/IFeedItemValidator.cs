using System.Xml.Linq;

public interface IFeedItemValidator
{
    bool IsValid(XElement itemElement, XNamespace contentNs);
}