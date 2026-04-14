using HtmlAgilityPack;
using Microsoft.Extensions.Logging;
using PositiveNews.Application.DTOs;
using PositiveNews.Application.Interfaces;
using PositiveNews.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Xml.Linq;
using PositiveNews.Infrastructure.Constants;

namespace PositiveNews.Infrastructure.Services;

public class PreviewImgTagExtractor : IImgTagExtractor
{
    private readonly ILogger<FeedReader> _logger;

    private static readonly XNamespace MediaNs = "http://search.yahoo.com/mrss/";

    public PreviewImgTagExtractor(ILogger<FeedReader> logger)
    {
        _logger = logger;
    }
    public string? ExtractImgTag(XElement itemElement, string feedUrl, HtmlNode? contentNode, HtmlNode? descriptionNode)
    {
        try
        {
            // 1. media:thumbnail (direct child or nested inside media:content)
            var thumbnail =
                itemElement.Element(MediaNs + "thumbnail") ??
                itemElement.Element(MediaNs + "content")?.Element(MediaNs + "thumbnail");

            if (thumbnail != null)
            {
                return BuildImgTag(
                    src: thumbnail.Attribute("url")?.Value,
                    width: thumbnail.Attribute("width")?.Value,
                    height: thumbnail.Attribute("height")?.Value,
                    alt: thumbnail.Attribute("alt")?.Value ?? "Article thumbnail"
                );
            }

            // 2. Preferred <img> inside <description>
            var imgFromDesc = ExtractPreferredImgFromNode(descriptionNode);
            if (imgFromDesc != null)
                return imgFromDesc;

            // 3. Preferred <img> inside content
            var imgFromEncoded = ExtractPreferredImgFromNode(contentNode);
            if (imgFromEncoded != null)
                return imgFromEncoded;

            // 4. media:content (only reached if needed)
            var mediaContent = itemElement.Element(MediaNs + "content");
            if (mediaContent != null)
            {
                return BuildImgTag(
                    src: mediaContent.Attribute("url")?.Value,
                    width: mediaContent.Attribute("width")?.Value,
                    height: mediaContent.Attribute("height")?.Value,
                    alt: mediaContent.Attribute("alt")?.Value ?? "Article image"
                );
            }

            // 5. Any <img> inside <description>
            imgFromDesc = ExtractAnyImgFromNode(descriptionNode);
            if (imgFromDesc != null)
                return imgFromDesc;

            // 6. Any <img> inside content
            imgFromEncoded = ExtractAnyImgFromNode(contentNode);
            if (imgFromEncoded != null)
                return imgFromEncoded;

            _logger.LogDebug("No thumbnail image tag extracted for current article. Default will be assigned");
            return AssignDefaultThumbnailImg(feedUrl);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during image tag extraction");
            return null;
        }
    }


    private static string? ExtractImgFromNode(HtmlNode? rootNode, string xpathQuery)
    {
        if (rootNode == null)
            return null;
        
        var images = rootNode.SelectNodes(xpathQuery);

        var img = images
            ?.OrderBy(node =>
            {
                var heightAttr = node.GetAttributeValue("height", "");
                var widthAttr = node.GetAttributeValue("width", "");

                var height = int.TryParse(heightAttr, out var h) ? h : int.MaxValue;
                var width = int.TryParse(widthAttr, out var w) ? w : int.MaxValue;

                return height == int.MaxValue || width == int.MaxValue ? int.MaxValue : height / (double)width;
            })
            .FirstOrDefault();

        if (img == null)
            return null;

        return BuildImgTag(
            src: img.GetAttributeValue("src", ""),
            width: img.GetAttributeValue("width", ""),
            height: img.GetAttributeValue("height", ""),
            alt: img.GetAttributeValue("alt", "Article image"),
            srcset: img.GetAttributeValue("srcset", ""),
            sizes: img.GetAttributeValue("sizes", "")
        );
    }

    private static string? ExtractPreferredImgFromNode(HtmlNode? rootNode)
    => ExtractImgFromNode(rootNode, ".//img[@srcset and not(@class=\"img-fluid w-5 rounded mb-3\")]");

    private static string? ExtractAnyImgFromNode(HtmlNode? rootNode)
        => ExtractImgFromNode(rootNode, ".//img");

    private static string? CleanSrcsetForPreview(string? srcset)
    {
        if (string.IsNullOrWhiteSpace(srcset))
            return null;

        // Matches: URL + optional descriptor (e.g. "300w")
        var matches = System.Text.RegularExpressions.Regex.Matches(
            srcset,
            @"(?<url>\S+)(?:\s+(?<w>\d+)w)?"
        );

        var candidates = new List<(string url, int? width)>();

        foreach (System.Text.RegularExpressions.Match match in matches)
        {
            var url = match.Groups["url"].Value;
            int? width = null;

            if (match.Groups["w"].Success &&
                int.TryParse(match.Groups["w"].Value, out var w))
            {
                width = w;
            }

            // Skip garbage matches
            if (string.IsNullOrWhiteSpace(url) || !url.StartsWith("http"))
                continue;

            candidates.Add((url, width));
        }

        if (candidates.Count == 0)
            return null;

        var maxWidth = GetPreferredMaxWidth(candidates);

        var filtered = candidates
            .Where(c => !c.width.HasValue || c.width <= maxWidth)
            .OrderBy(c => c.width ?? int.MaxValue)
            .ToList();

        // Fallback: if nothing <= 1024, take smallest available
        if (filtered.Count == 0)
        {
            var smallest = candidates
                .Where(c => c.width.HasValue)
                .OrderBy(c => c.width)
                .FirstOrDefault();

            if (smallest.url != null)
                filtered.Add(smallest);
        }

        return filtered.Count > 0
            ? string.Join(", ", filtered.Select(c =>
                c.width.HasValue ? $"{c.url} {c.width}w" : c.url))
            : null;
    }

    private static string? BuildImgTag(
        string? src = "",
        string? width = "",
        string? height = "",
        string? alt = "",
        string? srcset = "",
        string? sizes = "")
    {
        if (string.IsNullOrWhiteSpace(src))
            return null;

        alt ??= "Article image";

        // Clean and limit srcset
        srcset = CleanSrcsetForPreview(srcset);

        // Force safe sizes for card previews
        sizes = "(max-width: 576px) 100vw, (max-width: 992px) 80vw, 805px";

        var sb = new StringBuilder();

        var bestSrc = PickBestSrcFromSrcset(srcset) ?? src;

        sb.Append($"<img src=\"{WebUtility.HtmlEncode(bestSrc)}\" ");

        if (!string.IsNullOrWhiteSpace(width))
        {
            sb.Append($"width=\"{width}\" ");
        }

        if (!string.IsNullOrWhiteSpace(height))
        {
            sb.Append($"height=\"{height}\" ");
        }

        sb.Append("class=\"img-fluid w-100 rounded mb-3\" ");
        sb.Append($"alt=\"{WebUtility.HtmlEncode(alt)}\" ");

        if (!string.IsNullOrWhiteSpace(srcset))
        {
            sb.Append($"srcset=\"{WebUtility.HtmlEncode(srcset)}\" ");
            sb.Append($"sizes=\"{WebUtility.HtmlEncode(sizes)}\" ");
        }

        sb.Append("/>");
        return sb.ToString();
    }

    private static string? PickBestSrcFromSrcset(string? srcset, int targetWidth = 805)
    {
        if (string.IsNullOrWhiteSpace(srcset))
            return null;

        var matches = System.Text.RegularExpressions.Regex.Matches(
            srcset,
            @"(?<url>\S+)(?:\s+(?<w>\d+)w)?"
        );

        var candidates = new List<(string url, int width)>();

        foreach (System.Text.RegularExpressions.Match match in matches)
        {
            if (!match.Groups["w"].Success)
                continue;

            if (int.TryParse(match.Groups["w"].Value, out var w))
            {
                var url = match.Groups["url"].Value;
                candidates.Add((url, w));
            }
        }

        if (candidates.Count == 0)
            return null;

        return candidates
            .OrderBy(c => Math.Abs(c.width - targetWidth))
            .First()
            .url;
    }

    private static int GetPreferredMaxWidth(List<(string url, int? width)> candidates)
    {
        var widths = candidates
            .Where(c => c.width.HasValue)
            .Select(c => c.width!.Value)
            .OrderBy(w => w)
            .ToList();

        if (widths.Count == 0)
            return 805;

        bool hasPng = candidates.Any(c =>
            c.url.Contains(".png", StringComparison.OrdinalIgnoreCase));

        // Ideal preview range
        int target = hasPng ? 600 : 805;

        // Pick closest available width ABOVE target (not below → avoid blur)
        var best = widths.FirstOrDefault(w => w >= target);

        if (best != 0)
            return best;

        // fallback → largest available (if all smaller)
        return widths.Last();
    }

    private string? AssignDefaultThumbnailImg(string feedUrl)
    {
        return DefaultThumbnailTags.ThumbnailMap.FirstOrDefault(p => feedUrl.Contains(p.Key)).Value
            ?? LogAndReturnNull();

        string? LogAndReturnNull()
        {
            _logger.LogWarning("Error: No default thumbnail image tag assigned!");
            return null;
        }
    }
}

