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

namespace PositiveNews.Infrastructure.Services;

public class ImgTagExtractor : IImgTagExtractor
{
    private readonly ILogger<FeedReader> _logger;

    private static readonly XNamespace MediaNs = "http://search.yahoo.com/mrss/";
    private static readonly XNamespace ContentNs = "http://purl.org/rss/1.0/modules/content/";

    public ImgTagExtractor(ILogger<FeedReader> logger)
    {
        _logger = logger;
    }
    public string? ExtractImgTag(XElement itemElement)
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

            // 2. Prefered <img> inside <description>
            var descriptionHtml = itemElement.Element("description")?.Value;
            if (!string.IsNullOrWhiteSpace(descriptionHtml))
            {
                var imgFromDesc = ExtractPreferedImgFromHtml(descriptionHtml);
                if (imgFromDesc != null)
                    return imgFromDesc;
            }

            // 3. Prefered <img> inside <content:encoded> 
            var encodedHtml = itemElement.Element(ContentNs + "encoded")?.Value;
            if (!string.IsNullOrWhiteSpace(encodedHtml))
            {
                var imgFromEncoded = ExtractPreferedImgFromHtml(encodedHtml);
                if (imgFromEncoded != null)
                    return imgFromEncoded;
            }

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

            // 5. Eny <img> inside <description>
            if (!string.IsNullOrWhiteSpace(descriptionHtml))
            {
                var imgFromDesc = ExtractEnyImgFromHtml(descriptionHtml);
                if (imgFromDesc != null)
                    return imgFromDesc;
            }

            // 6. Eny <img> inside <content:encoded> 
            if (!string.IsNullOrWhiteSpace(encodedHtml))
            {
                var imgFromEncoded = ExtractEnyImgFromHtml(encodedHtml);
                if (imgFromEncoded != null)
                    return imgFromEncoded;
            }

            _logger.LogWarning("No thumbnail image tag extracted for current article.");
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error durring image tag extracting");
            return null;
        }
        }


    private static string? ExtractPreferedImgFromHtml(string? html)
    {
        if (string.IsNullOrWhiteSpace(html))
            return null;

        var doc = new HtmlDocument();
        doc.LoadHtml(html);

        // fallback: any img without slot
        var img = doc.DocumentNode.SelectSingleNode("//img[not(@slot) and @srcset]");

        if (img == null)
            return null;

        // ← IMPORTANT: Now we forward srcset and sizes
        return BuildImgTag(
            src: img.GetAttributeValue("src", ""),
            width: img.GetAttributeValue("width", ""),
            height: img.GetAttributeValue("height", ""),
            alt: img.GetAttributeValue("alt", "Article image"),
            srcset: img.GetAttributeValue("srcset", ""),
            sizes: img.GetAttributeValue("sizes", "")
        );
    }

    private static string? ExtractEnyImgFromHtml(string? html)
    {
        if (string.IsNullOrWhiteSpace(html))
            return null;

        var doc = new HtmlDocument();
        doc.LoadHtml(html);

        var img = doc.DocumentNode.SelectSingleNode("//img");

        if (img == null)
            return null;

        // ← IMPORTANT: Now we forward srcset and sizes
        return BuildImgTag(
            src: img.GetAttributeValue("src", ""),
            width: img.GetAttributeValue("width", ""),
            height: img.GetAttributeValue("height", ""),
            alt: img.GetAttributeValue("alt", "Article image"),
            srcset: img.GetAttributeValue("srcset", ""),
            sizes: img.GetAttributeValue("sizes", "")
        );
    }

    // ===================================================================
    // CleanSrcsetForPreview - FIXED: properly handles commas inside URLs
    // ===================================================================
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

    // ===================================================================
    // BuildImgTag - Simplified and safe for previews
    // ===================================================================
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
}


/*
    /// <summary>
    /// Validates that URL is a proper image URL
    /// </summary>
    private bool IsValidImageUrl(string? url)
    {
        if (string.IsNullOrWhiteSpace(url))
            return false;

        // Must start with http/https
        if (!url.StartsWith("http://") && !url.StartsWith("https://"))
            return false;

        // Must have image extension
        var lowerUrl = url.ToLower();
        var validExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
        return validExtensions.Any(ext => lowerUrl.Contains(ext));
    }

    private static ImgTagDto? ExtractThumbnailImgData(XElement itemElement)
    {
        // 1. media:thumbnail (direct child or nested inside media:content)
        var thumbnail =
            itemElement.Element(MediaNs + "thumbnail") ??
            itemElement.Element(MediaNs + "content")?.Element(MediaNs + "thumbnail");
        if (thumbnail != null)
            return ExtractImgDataFromXelement(thumbnail);


        // 2. First <img> inside <description>
        var descriptionHtml = itemElement.Element("description")?.Value;
        if (!string.IsNullOrWhiteSpace(descriptionHtml))
        {
            var imgFromDesc = ExtractImgDataFromHtml(descriptionHtml);

            if (imgFromDesc != null)
                return imgFromDesc;
        }

        // 3. media:content (full image)
        var mediaContent = itemElement.Element(MediaNs + "content");
        if (mediaContent != null)
            return ExtractImgDataFromXelement(mediaContent);

        // 4. First <img> inside <content:encoded> (only reached if needed)
        var encodedHtml = itemElement.Element(ContentNs + "encoded")?.Value;
        if (!string.IsNullOrWhiteSpace(encodedHtml))
        {
            var imgFromEncoded = ExtractImgDataFromHtml(encodedHtml);

            if (imgFromEncoded != null)
                return imgFromEncoded;
        }
        return null;
    }

    private static ImgTagDto? ExtractImgDataFromHtml(string? html)
    {
        if (string.IsNullOrWhiteSpace(html))
            return null;

        var doc = new HtmlDocument();
        doc.LoadHtml(html);

        var img = doc.DocumentNode.SelectSingleNode("//img");
        if (img == null)
            return null;

        return new ImgTagDto
        {
            Url = img.GetAttributeValue("src", ""),
            Width = (img.GetAttributeValue("width", 800)),
            Height = img.GetAttributeValue("height", 600),
            Alt = img.GetAttributeValue("alt", "Article image"),
            SrcSet = img.GetAttributeValue("srcset", "")
        };
    }

    private static string? CreateNewThumbnailTag(ImgTagDto imgDto)
    {
        if(!string.IsNullOrWhiteSpace(imgDto.SrcSet) &&
            (imgDto.Width > 1024 || imgDto.Height > 768))
        {
            LightenImg(imgDto);
        }
    }

    private static void LightenImg(ImgTagDto imgDto)
    {
        var srcset = ParseSrcset(imgDto.SrcSet!);

        if (srcset.Count > 0)
        {
            var LightImgWidth = FindThumbnailWidth(srcset, 800);

            if (LightImgWidth != null)
            {
                var ratio = imgDto.Width / imgDto.Height;
                imgDto.Width = (int)LightImgWidth;
                imgDto.Height = imgDto.Width / ratio;

                imgDto.Url = srcset[imgDto.Width];
            }
        }
    }

    private static int? FindThumbnailWidth(Dictionary<int, string> srcset, int target)
    {
        if (srcset == null || srcset.Count == 0)
            return null;

        int closest = srcset.Keys.First();
        int minDiff = Math.Abs(closest - target);

        foreach (var key in srcset.Keys)
        {
            int diff = Math.Abs(key - target);

            if (diff < minDiff)
            {
                minDiff = diff;
                closest = key;
            }
        }
        return closest;
    }

    private static Dictionary<int, string> ParseSrcset(string srcset)
    {
        var result = new Dictionary<int, string>();

        if (string.IsNullOrWhiteSpace(srcset))
            return result;

        var candidates = srcset.Split(',');

        foreach (var candidate in candidates)
        {
            var parts = candidate
                .Trim()
                .Split(' ', StringSplitOptions.RemoveEmptyEntries);

            if (parts.Length == 0)
                continue;

            var url = parts[0];

            if (parts.Length > 1)
            {
                var descriptor = parts[1];

                // Handle "480w" → 480
                if (descriptor.EndsWith("w") &&
                    int.TryParse(descriptor[..^1], out int width))
                {
                    result[width] = url;
                }
                // Optional: handle "2x" → 2
                else if (descriptor.EndsWith("x") &&
                         int.TryParse(descriptor[..^1], out int density))
                {
                    result[density] = url;
                }
            }
        }

        return result;
    }*/