
namespace PositiveNews.Infrastructure.Constants;

public class DefaultThumbnailTags
{
    private const string _defaultNvidiaThumbnailTagPart1 = 
        "<img src=\"/Defaults/nvidia.png\" width=\"800\" height=\"800\"";
    private const string _defaultBuddhaThumbnailTagPart1 = 
        "<img src=\"/Defaults/buddha.png\" width=\"612\" height=\"306\"";
    private const string _defaultDesignyoutrustThumbnailTagPart1 = 
        "<img src=\"/Defaults/designyoutrust.png\" width=\"600\" height=\"600\"";
    private const string _defaultNASAThumbnailTagPart1 = 
        "<img src=\"/Defaults/nasa.png\" width=\"512\" height=\"512\"";
    private const string _defaultOptimistdailyThumbnailTagPart1 = 
        "<img src=\"/Defaults/optimistdaily.png\" width=\"600\" height=\"153\"";
    private const string _defaultThisiscolossalThumbnailTagPart1 = 
        "<img src=\"/Defaults/thisiscolossal.png\" width=\"988\" height=\"988\"";

    private const string _defaultThumbnailTagPart2 = 
        " alt=\"Default article image\" class=\"img-fluid w-100 rounded mb-3\">";

    public static readonly Dictionary<string, string> ThumbnailMap = new(StringComparer.OrdinalIgnoreCase)
    {
        ["nvidia"] = string.Concat(_defaultNvidiaThumbnailTagPart1, _defaultThumbnailTagPart2),
        ["optimistdaily"] = string.Concat(_defaultOptimistdailyThumbnailTagPart1, _defaultThumbnailTagPart2),
        ["nasa"] = string.Concat(_defaultNASAThumbnailTagPart1, _defaultThumbnailTagPart2),
        ["thisiscolossal"] = string.Concat(_defaultThisiscolossalThumbnailTagPart1, _defaultThumbnailTagPart2),
        ["designyoutrust"] = string.Concat(_defaultDesignyoutrustThumbnailTagPart1, _defaultThumbnailTagPart2),
        ["tinybuddha"] = string.Concat(_defaultBuddhaThumbnailTagPart1, _defaultThumbnailTagPart2)
    };
}
