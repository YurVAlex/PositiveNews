using HtmlAgilityPack;
using PositiveNews.Application.DTOs;
using PositiveNews.Application.Interfaces;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using static Microsoft.CodeAnalysis.CSharp.SyntaxTokenParser;

namespace PositiveNews.Infrastructure.Services;

public class FeedItemCleaner : IFeedItemCleaner
{
    private static readonly HashSet<string> AllowedTags = new(StringComparer.OrdinalIgnoreCase)
    {
        "h1", "h2", "h3", "h4", "p", "img", "video", "source", "ul", "ol", "li", "a", "iframe"
    };

    private static readonly HashSet<string> AttributesToRemove = new(StringComparer.OrdinalIgnoreCase)
    {
        "class", "style", "block_context"
    };

    private static readonly string[] RemoveDivClassPatterns =
    {
        "navigation-menu",
        "blocks-content-lists",
        "hds-featured-file-list",
        "hds-audio-player-",
        "secondary-navigation",
        "comparison-slider-parent",
        "two-up-view",
        "wp-biographia-pic",
        "content-lists-inner",
        "topic-cards"
    };

    // Patterns that trigger "stop processing all following content"
    private static readonly string[] StopProcessingPatterns =
    {
        "About the Author",
        "For more information about",
        "To learn more about",
        "Discover More Topics From",
        "For more information on",
        "Click here to leave a comment on the site",
        "Members can also look for the following:",
        "What are you planning to play this weekend?",
        "in the comments below."
    };

    // Patterns that trigger removal of a single node
    private static readonly string[] RemoveNodePatterns =
    {
    "Learn more about this image",
    "Listen to this audio",
    "follow us on",
    "Donate link:",
    "The Optimist Daily is a project of the World Business Academy",
    "Subscribe to our",
    
    "Want to be part of the Optimism Movement?",
    "If you have questions, comments, feedback, suggestions, or just want to say hi, send a message to:"
    };

    private static readonly Regex YoutubeRegex = new(
    @"(?:https?://)?(?:www\.)?(?:youtube\.com/watch\?v=|youtu\.be/|youtube\.com/embed/)([a-zA-Z0-9_-]{11})",
    RegexOptions.IgnoreCase | RegexOptions.Compiled);




    public void Clean(RssFeedItemDto dto)
    {
        dto.Description = CleanDescription(dto.Description);
        dto.ContentClean = CleanContent(dto.ContentRaw);
        dto.Title = CleanTitle(dto.Title);
    }












    private static string CleanContent(string rawContent)
    {
        var doc = LoadDocument(rawContent);
        var builder = new StringBuilder();
        var stopProcessing = false;

        ProcessNodesRecursively(doc.DocumentNode, builder, ref stopProcessing);

        var cleaned = RemoveTrailingPostLinks(builder.ToString());
        return cleaned.Trim();
    }

    private static HtmlDocument LoadDocument(string html)
    {
        var doc = new HtmlDocument();
        doc.LoadHtml(html);
        return doc;
    }

    private static void ProcessNodesRecursively(HtmlNode node, StringBuilder builder, ref bool stopProcessing)
    {
        foreach (var child in node.ChildNodes)
        {
            if (stopProcessing)
                return;

            if (child.NodeType != HtmlNodeType.Element)
                continue;

            // Check for stop-processing triggers first
            if (ShouldStopProcessing(child))
            {
                stopProcessing = true;
                return;
            }

            // Check if node should be skipped entirely
            if (ShouldRemoveNode(child))
                continue;

            var tagName = child.Name.ToLowerInvariant();

            // Handle special div transformations
            if (tagName == "div")
            {
                ProcessDiv(child, builder, ref stopProcessing);
                continue;
            }

            // Handle anchor tags (check for YouTube links)
            if (tagName == "a")
            {
                ProcessAnchor(child, builder, ref stopProcessing);
                continue;
            }

            // Handle lists with wp-block-list class
            if ((tagName == "ul" || tagName == "ol") && HasClassContaining(child, "wp-block-list"))
            {
                ProcessList(child, builder);
                continue;
            }

            if (AllowedTags.Contains(tagName))
            {
                ProcessAllowedNode(child, tagName, builder, ref stopProcessing);
            }
            else
            {
                // Recursively process children of non-allowed tags
                ProcessNodesRecursively(child, builder, ref stopProcessing);
            }
        }
    }

    private static void ProcessDiv(HtmlNode node, StringBuilder builder, ref bool stopProcessing)
    {
        // Check if div should be removed by class
        foreach (var pattern in RemoveDivClassPatterns)
        {
            if (HasClassContaining(node, pattern))
                return;
        }

        // Transform hds-caption-text divs to small italic paragraph
        if (HasClassContaining(node, "hds-caption-text"))
        {
            var text = HtmlEntity.DeEntitize(node.InnerText).Trim();
            if (!string.IsNullOrWhiteSpace(text))
            {
                builder.AppendLine($"<p class=\"small fst-italic\">{System.Net.WebUtility.HtmlEncode(text)}</p>");
            }
            return;
        }

        // Otherwise, process children of the div
        ProcessNodesRecursively(node, builder, ref stopProcessing);
    }

    private static void ProcessAnchor(HtmlNode node, StringBuilder builder, ref bool stopProcessing)
    {
        var href = node.GetAttributeValue("href", "");

        // Check if it's a YouTube link
        var youtubeMatch = YoutubeRegex.Match(href);
        if (youtubeMatch.Success)
        {
            var videoId = youtubeMatch.Groups[1].Value;
            builder.AppendLine(CreateYouTubeEmbed(videoId));
            return;
        }

        // Check for YouTube links in the anchor's inner content (nested links)
        var innerHtml = node.InnerHtml;
        var innerMatch = YoutubeRegex.Match(innerHtml);
        if (innerMatch.Success)
        {
            var videoId = innerMatch.Groups[1].Value;
            builder.AppendLine(CreateYouTubeEmbed(videoId));
            return;
        }

        // Otherwise, process anchor normally (keep it, process children)
        ProcessNodesRecursively(node, builder, ref stopProcessing);
    }

    private static string CreateYouTubeEmbed(string videoId)
    {
        return $@"<div class=""ratio ratio-16x9 mb-3"">
  <iframe src=""https://www.youtube.com/embed/{videoId}"" 
          title=""YouTube video"" 
          allow=""accelerometer; autoplay; clipboard-write; encrypted-media; gyroscope; picture-in-picture; web-share"" 
          allowfullscreen>
  </iframe>
</div>";
    }

    private static void ProcessList(HtmlNode node, StringBuilder builder)
    {
        var cleanedNode = CleanAttributes(node);
        builder.AppendLine(cleanedNode.OuterHtml);
    }

    private static void ProcessAllowedNode(HtmlNode node, string tagName, StringBuilder builder, ref bool stopProcessing)
    {
        switch (tagName)
        {
            case "p":
                ProcessParagraph(node, builder);
                break;

            case "h1":
            case "h2":
            case "h3":
            case "h4":
                ProcessHeader(node, builder);
                break;

            case "img":
                ProcessImage(node, builder);
                break;

            case "video":
                ProcessVideo(node, builder);
                break;

            case "ul":
            case "ol":
                ProcessList(node, builder);
                break;

            case "li":
                ProcessListItem(node, builder);
                break;

            case "iframe":
                ProcessIframe(node, builder);
                break;
        }
    }

    private static void ProcessParagraph(HtmlNode node, StringBuilder builder)
    {
        // Check for YouTube iframes FIRST and extract them
        var iframes = node.SelectNodes(".//iframe");
        if (iframes != null)
        {
            foreach (var iframe in iframes)
            {
                var src = iframe.GetAttributeValue("src", "");
                var youtubeMatch = YoutubeRegex.Match(src);
                if (youtubeMatch.Success)
                {
                    var videoId = youtubeMatch.Groups[1].Value;
                    builder.AppendLine(CreateYouTubeEmbed(videoId));
                }
            }
        }

        var text = HtmlEntity.DeEntitize(node.InnerText).Trim();

        // Include iframe in media check
        var hasMedia = node.SelectNodes(".//img | .//video | .//iframe") != null;
        var hasText = !string.IsNullOrWhiteSpace(text);

        // If paragraph ONLY contained iframe (already processed above), skip outputting empty <p>
        if (!hasText && iframes != null && node.SelectNodes(".//img | .//video") == null)
            return;

        if (!hasText && !hasMedia)
            return;

        if (hasText && ShouldRemoveParagraph(text))
            return;

        var cleanedNode = CleanAttributes(node);
        builder.AppendLine(cleanedNode.OuterHtml);
    }

    private static void ProcessHeader(HtmlNode node, StringBuilder builder)
    {
        var text = HtmlEntity.DeEntitize(node.InnerText).Trim();

        if (string.IsNullOrWhiteSpace(text))
            return;

        var cleanedNode = CleanAttributes(node);
        builder.AppendLine(cleanedNode.OuterHtml);
    }

    private static void ProcessImage(HtmlNode node, StringBuilder builder)
    {
        var classAttr = node.GetAttributeValue("class", "");

        // Skip thumbnail images
        if (HasExactClass(node, "attachment-thumbnail size-thumbnail"))
            return;

        var cleanedNode = CleanAttributes(node);

        // Standard image styling
        cleanedNode.SetAttributeValue("class", "img-fluid w-100 rounded mb-3");
        builder.AppendLine(cleanedNode.OuterHtml);
    }

    private static void ProcessVideo(HtmlNode node, StringBuilder builder)
    {
        var cleanedNode = CleanAttributes(node);

        // Add Bootstrap responsive video classes
        cleanedNode.SetAttributeValue("class", "w-100 rounded mb-3");
        cleanedNode.SetAttributeValue("controls", "");

        var sources = cleanedNode.SelectNodes(".//source");
        if (sources != null)
        {
            foreach (var source in sources)
                CleanAttributesRecursively(source);
        }

        // Wrap in responsive container
        var videoHtml = cleanedNode.OuterHtml;
        builder.AppendLine($"<div class=\"ratio ratio-16x9 mb-3\">{videoHtml}</div>");
    }

    private static void ProcessIframe(HtmlNode node, StringBuilder builder)
    {
        var src = node.GetAttributeValue("src", "");

        // Check if it's a YouTube embed
        var youtubeMatch = YoutubeRegex.Match(src);
        if (youtubeMatch.Success)
        {
            var videoId = youtubeMatch.Groups[1].Value;
            builder.AppendLine(CreateYouTubeEmbed(videoId));
            return;
        }

        // Generic iframe - wrap in responsive container
        var cleanedNode = CleanAttributes(node);
        cleanedNode.SetAttributeValue("class", "w-100");
        builder.AppendLine($"<div class=\"ratio ratio-16x9 mb-3\">{cleanedNode.OuterHtml}</div>");
    }

    private static void ProcessListItem(HtmlNode node, StringBuilder builder)
    {
        var cleanedNode = CleanAttributes(node);
        builder.AppendLine(cleanedNode.OuterHtml);
    }

    private static HtmlNode CleanAttributes(HtmlNode original)
    {
        var clone = original.CloneNode(true);
        CleanAttributesRecursively(clone);
        return clone;
    }

    private static void CleanAttributesRecursively(HtmlNode node)
    {
        if (node.NodeType == HtmlNodeType.Element)
        {
            // Remove thumbnail images from cloned content
            var thumbnails = node.SelectNodes(".//img[contains(@class, 'attachment-thumbnail size-thumbnail')]");
            if (thumbnails != null)
            {
                foreach (var thumb in thumbnails.ToList())
                    thumb.Remove();
            }

            // Remove unwanted attributes
            var toRemove = node.Attributes
                .Where(attr => AttributesToRemove.Contains(attr.Name))
                .Select(attr => attr.Name)
                .ToList();

            foreach (var attrName in toRemove)
                node.Attributes.Remove(attrName);

            // Set standard class for images
            if (node.Name.Equals("img", StringComparison.OrdinalIgnoreCase))
            {
                node.SetAttributeValue("class", "img-fluid w-100 rounded mb-3");
            }
        }

        foreach (var child in node.ChildNodes)
        {
            CleanAttributesRecursively(child);
        }
    }

    private static bool HasClassContaining(HtmlNode node, string pattern)
    {
        var classAttr = node.GetAttributeValue("class", "");
        return classAttr.Contains(pattern, StringComparison.OrdinalIgnoreCase);
    }

    private static bool HasExactClass(HtmlNode node, string exactClass)
    {
        var classAttr = node.GetAttributeValue("class", "");
        return classAttr.Equals(exactClass, StringComparison.OrdinalIgnoreCase);
    }

    private static bool ShouldStopProcessing(HtmlNode node)
    {
        var text = HtmlEntity.DeEntitize(node.InnerText).Trim();

        // Check for single "Share" text
        if (text.Equals("Share", StringComparison.OrdinalIgnoreCase))
            return true;

        // Check for stop-processing patterns
        foreach (var pattern in StopProcessingPatterns)
        {
            if (text.Contains(pattern, StringComparison.OrdinalIgnoreCase))
                return true;
        }

        return false;
    }

    private static bool ShouldRemoveNode(HtmlNode node)
    {
        var text = HtmlEntity.DeEntitize(node.InnerText);

        foreach (var pattern in RemoveNodePatterns)
        {
            if (text.Contains(pattern, StringComparison.OrdinalIgnoreCase))
                return true;
        }

        var tagName = node.Name.ToLowerInvariant();

        // Check for divs that should be removed by class
        if (tagName == "div")
        {
            foreach (var pattern in RemoveDivClassPatterns)
            {
                if (HasClassContaining(node, pattern))
                    return true;
            }
        }

        // Check for thumbnail images
        if (tagName == "img" && HasExactClass(node, "attachment-thumbnail size-thumbnail"))
            return true;

        return false;
    }

    private static bool ShouldRemoveParagraph(string text)
    {
        var lower = text.ToLowerInvariant();
        return lower.Contains("did this solution stand out")
            || lower.Contains("becoming an emissary")
            || lower.StartsWith("the post ")
            || lower.Contains("appeared first on");
    }

    private static string RemoveTrailingPostLinks(string content)
    {
        return Regex.Replace(
            content,
            @"<\/p>\s*The post\s*<a.*?<\/a>.*",
            "",
            RegexOptions.IgnoreCase | RegexOptions.Singleline);
    }

    private static string CleanDescription(string description)
    {
        if (!description.Contains('<'))
            return WebUtility.HtmlDecode(description).Trim();

        var doc = LoadDocument(description);

        var paragraphs = doc.DocumentNode.SelectNodes("//p");

        if (paragraphs == null || paragraphs.Count == 0)
            return WebUtility.HtmlDecode(doc.DocumentNode.InnerText).Trim();

        var texts = paragraphs
            .Select(p => WebUtility.HtmlDecode(p.InnerText).Trim())
            .Where(t => !string.IsNullOrWhiteSpace(t));

        var result = string.Join(" ", texts);

        return result.Length > 1999 ? texts.First() : result;
    }

    private static string CleanTitle(string title)
    {
        return title.Length > 500 ? title[..500] : title;
    }

}