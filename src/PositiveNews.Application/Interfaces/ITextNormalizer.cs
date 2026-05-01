namespace PositiveNews.Application.Interfaces;

public interface ITextNormalizer
{
    string NormalizeContent(string htmlContent);
    string NormalizeDescription(string description);
    string NormalizeTitle(string title);
}
