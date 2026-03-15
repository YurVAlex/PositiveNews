using System.Xml.Linq;

public interface IRssItemElementValidator
{
    bool IsValid(XElement itemElement, XNamespace contentNs);
}