using HtmlAgilityPack;
using Microsoft.Extensions.Logging;
using PositiveNews.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;

namespace PositiveNews.Infrastructure.Services;

internal class ThumbnailTagExtractor
{
    private readonly ILogger<FeedReader> _logger;

    private static readonly XNamespace MediaNs = "http://search.yahoo.com/mrss/";
    private static readonly XNamespace ContentNs = "http://purl.org/rss/1.0/modules/content/";

    public ThumbnailTagExtractor(ILogger<FeedReader> logger)
    {
        _logger = logger;
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
    }
}