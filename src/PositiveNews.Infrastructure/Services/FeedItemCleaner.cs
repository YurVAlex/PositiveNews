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
        "content-lists-inner",
        "topic-cards",
        "twitter-tweet",
        "attachment-medium size-medium wp-post-image" //first image for Design You Trust
    };

    // Patterns that trigger "stop processing all following content"
    private static readonly string[] StopProcessingPatterns =
    {
        "About the Author",
        "For more information about",
        "Discover More Topics From",
        "For more information on",
        "Click here to leave a comment on the site",
        "Members can also look for the following:",
        "What are you planning to play this weekend?",
        "in the comments below.",
        "-end-"
    };

    // Patterns that trigger removal of a single node
    private static readonly string[] RemoveNodePatterns =
    {
    "Learn more about this image",
     "To learn more about",
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

    private static readonly Regex StrongTildeRegex = new(
    @"<strong>\s*(?<left>[^~<]+?)\s*~\s*[^<]*\s*</strong>",
    RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private static readonly Regex AppearedFirstOnRegex = new(
    @"<a[^>]*>.*?<\/a>\s*appeared first on\s*<a[^>]*>.*?<\/a>\.?",
    RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private static readonly Regex OptimistEditorialRegex = new(
    @"BY THE OPTIMIST DAILY(?:’S|'S)? EDITORIAL TEAM",
    RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private static readonly Regex ThePostRegex = new(
    @"The post\s*<a[^>]*>.*?<\/a>\s*first appeared on\s*<a[^>]*>.*?<\/a>\.?",
    RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.Singleline);

    public void Clean(RssFeedItemDto dto)
    {
        dto.Description = CleanDescription(dto.Description);
        dto.ContentClean = CleanContent(dto.ContentRaw);
        dto.Title = CleanTitle(dto.Title);
    }

    private static string CleanContent(string rawContent)
    {
        rawContent = RemoveTildeAuthor(rawContent);

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

        var images = node.SelectNodes(".//img");
        if (images != null)
        {
            foreach (var img in images.ToList())
            {
                var imgBuilder = new StringBuilder();
                ProcessImage(img, imgBuilder);

                // Replace original <img> with processed one
                var newImgDoc = new HtmlDocument();
                newImgDoc.LoadHtml(imgBuilder.ToString());
                var newImg = newImgDoc.DocumentNode.FirstChild;
                img.ParentNode.ReplaceChild(newImg, img);
            }
        }

        // THEN clean paragraph
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

        if (classAttr.Contains("attachment-thumbnail size-thumbnail", StringComparison.OrdinalIgnoreCase)||
            classAttr.Contains("wp-biographia-avatar", StringComparison.OrdinalIgnoreCase) ||
            classAttr.Contains("wp-smiley", StringComparison.OrdinalIgnoreCase) ||  // ← Add this for emoji
            classAttr.Contains("emoji", StringComparison.OrdinalIgnoreCase))
        {
            // Remove unwanted attributes
            var toRemove = node.Attributes
                .Where(attr => AttributesToRemove.Contains(attr.Name))
                .Select(attr => attr.Name)
                .ToList();

            foreach (var attrName in toRemove)
                node.Attributes.Remove(attrName);

            node.SetAttributeValue("class", "img-fluid w-5 rounded mb-3");
            builder.AppendLine(node.OuterHtml);
        }
        else
        {
            // Remove unwanted attributes
            var toRemove = node.Attributes
                .Where(attr => AttributesToRemove.Contains(attr.Name))
                .Select(attr => attr.Name)
                .ToList();

            foreach (var attrName in toRemove)
                node.Attributes.Remove(attrName);

            // Standard image styling
            node.SetAttributeValue("class", "img-fluid w-100 rounded mb-3");
            builder.AppendLine(node.OuterHtml);
        }
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
        if (node == null || node.Name.Equals("img", StringComparison.OrdinalIgnoreCase))
            return;

        if (node.NodeType == HtmlNodeType.Element)
        {

            // Remove script tags anywhere in the subtree
            var scripts = node.SelectNodes(".//script");
            if (scripts != null)
            {
                foreach (var script in scripts.ToList())
                    script.Remove();
            }

            // Remove other non-allowed tags from cloned content
            var disallowedTags = node.SelectNodes(".//script | .//style | .//noscript");
            if (disallowedTags != null)
            {
                foreach (var tag in disallowedTags.ToList())
                    tag.Remove();
            }

            // Remove unwanted attributes
            var toRemove = node.Attributes
                .Where(attr => AttributesToRemove.Contains(attr.Name))
                .Select(attr => attr.Name)
                .ToList();

            foreach (var attrName in toRemove)
                node.Attributes.Remove(attrName);
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

       
        foreach (var pattern in RemoveDivClassPatterns)
        {
            if (HasClassContaining(node, pattern))
                return true;
        }
        

        // Check for thumbnail images
        if (tagName == "img" && HasExactClass(node, "attachment-thumbnail size-thumbnail"))
            return true;

        // ADD: Always remove script tags
        if (tagName == "script")
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
        if (string.IsNullOrWhiteSpace(description))
            return string.Empty;

        // Decode HTML entities first
        var html = WebUtility.HtmlDecode(description);

        html = ThePostRegex.Replace(html, "");

        // -------------------------------------------------------
        // 1. Remove "~something" inside <strong>
        // -------------------------------------------------------
        html = StrongTildeRegex.Replace(html, "<strong>${left}</strong>");

        // -------------------------------------------------------
        // 2. Remove unwanted editorial paragraph
        // -------------------------------------------------------
        html = OptimistEditorialRegex.Replace(html, "");

        // -------------------------------------------------------
        // 3. Remove "appeared first on Colossal"
        // -------------------------------------------------------
        html = AppearedFirstOnRegex.Replace(html, "");

        // -------------------------------------------------------
        // 4. Parse HTML safely
        // -------------------------------------------------------
        var doc = new HtmlDocument();
        doc.LoadHtml(html);

        var paragraphs = doc.DocumentNode.SelectNodes("//p");

        if (paragraphs == null || paragraphs.Count == 0)
            return TrimAfterLastDot(doc.DocumentNode.InnerText);

        var texts = paragraphs
    .Select(p => HtmlEntity.DeEntitize(p.InnerText).Trim())
    .Where(t => !string.IsNullOrWhiteSpace(t))
    .Where(t =>
        !Regex.IsMatch(t, @"appeared first on", RegexOptions.IgnoreCase) &&
        !Regex.IsMatch(t, @"BY THE OPTIMIST DAILY", RegexOptions.IgnoreCase) &&
        !Regex.IsMatch(t, @"^The post ", RegexOptions.IgnoreCase)
    );

        var result = string.Join(" ", texts);

        // Final safety cleanup (text-level)
        result = Regex.Replace(
            result,
            @"The post .*? first appeared on .*?\.",
            "",
            RegexOptions.IgnoreCase | RegexOptions.Singleline);

        result = Regex.Replace(
            result,
            @"BY THE OPTIMIST DAILY.*?(?=\.)",
            "",
            RegexOptions.IgnoreCase);

        result = result.Length > 1999 ? result[..1999] : result;

        // -------------------------------------------------------
        // 5. Trim everything after last dot
        // -------------------------------------------------------
        return TrimAfterLastDot(result);
    }

    private static string TrimAfterLastDot(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return text;

        var lastDotIndex = text.LastIndexOf('.');

        if (lastDotIndex > 0)
            return text.Substring(0, lastDotIndex + 1).Trim();

        return text.Trim();
    }

    private static string CleanTitle(string title)
    {
        return title.Length > 500 ? title[..500] : title;
    }

    private static string RemoveTildeAuthor(string html)
    {
        if (string.IsNullOrWhiteSpace(html))
            return html;

        return Regex.Replace(
            html,
            @"<strong>\s*(?<left>[^~<]+?)\s*~\s*[^<]*\s*</strong>",
            "<strong>${left}</strong>",
            RegexOptions.IgnoreCase | RegexOptions.Compiled
        );
    }


}