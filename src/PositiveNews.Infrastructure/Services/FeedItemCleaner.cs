using HtmlAgilityPack;
using PositiveNews.Application.DTOs;
using PositiveNews.Application.Interfaces;
using PositiveNews.Domain.Entities;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;

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
        "hds-featured-link-list",
        "blocks-content-lists",
        "nasa-blocks-article-intro",
        "nasa-blocks-article-hero-header",
        "hds-featured-file-list",
        "listicle-layout-basic",
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
        //"For more information about",
        "Discover More Topics From",
        //"For more information on",
        "Click here to leave a comment on the site",
        "Members can also look for the following:",
        "What are you planning to play this weekend?",
        "in the comments below.",
        "-end-",
        "-fin-"
    };

    // Patterns that trigger removal of a single node
    private static readonly string[] RemoveNodePatterns =
    {
    "Learn more about this image",
    "Listen to this audio",
    "Follow NVIDIA Workstation on",
    "Plug in to NVIDIA AI PC on",
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

    private static readonly Regex HtmlTagRegex = new(
    @"<[^>]+>",
    RegexOptions.Compiled);

    private static readonly Regex RemoveTrailingPostLinksRegex = new(
    @"<\/p>\s*The post\s*<a.*?<\/a>.*",
    RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.Compiled);

    private static readonly Regex AppearedFirstOnTextRegex = new(
    @"appeared first on",
    RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private static readonly Regex OptimistDailyTextRegex = new(
    @"BY THE OPTIMIST DAILY",
    RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private static readonly Regex StartsWithThePostRegex = new(
    @"^The post ",
    RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private static readonly Regex ThePostSentenceRegex = new(
    @"The post .*? first appeared on .*?\.",
    RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.Compiled);

    private static readonly Regex OptimistDailySentenceRegex = new(
    @"BY THE OPTIMIST DAILY.*?(?=\.)",
    RegexOptions.IgnoreCase | RegexOptions.Compiled);

    public RssFeedItemDto Clean(RssFeedItemDto dto, TopicLookup lookup, HtmlNode? rawContentNode)
    {
        dto.Description = CleanDescription(dto.Description);
        dto.ContentRaw = CleanContent(dto.ContentRaw, rawContentNode);
        dto.Title = CleanTitle(dto.Title);
        dto.Topics = CleanTopics(dto.Topics, lookup);
        return dto;
    }

    public string? StripInnerHtmlWords(string? htmlContent, HtmlNode? htmlNode = null)
    {
        if (htmlNode != null)
            return htmlNode.InnerText;

        if (string.IsNullOrWhiteSpace(htmlContent))
            return htmlContent;

        return LoadDocument(htmlContent).DocumentNode.InnerText;
    }

    private static HtmlDocument LoadDocument(string html)
    {
        var doc = new HtmlDocument();
        doc.LoadHtml(html);
        return doc;
    }

    private static string CleanContent(string rawContent, HtmlNode? rawContentNode)
    {
        var rootNode = rawContentNode ?? LoadDocument(rawContent).DocumentNode;
        var builder = new StringBuilder();
        var stopProcessing = false;

        ProcessNodesIterative(rootNode, builder, ref stopProcessing);

        var cleaned = RemoveTrailingPostLinks(builder.ToString());
        return RemoveTildeAuthor(cleaned).Trim();  
    }

    private static void ProcessNodesIterative(HtmlNode root, StringBuilder builder, ref bool stopProcessing)
    {
        var stack = new Stack<HtmlNode>();
        PushChildrenInReverse(root, stack);

        while (stack.Count > 0)
        {
            if (stopProcessing)
                return;

            var node = stack.Pop();
            if (node.NodeType != HtmlNodeType.Element)
                continue;

            if (ShouldStopProcessing(node))
            {
                stopProcessing = true;
                return;
            }

            if (ShouldRemoveNode(node))
                continue;

            var tagName = node.Name.ToLowerInvariant();

            if (tagName == "div")
            {
                ProcessDiv(node, builder, stack);
                continue;
            }

            if (tagName == "a")
            {
                ProcessAnchor(node, builder, stack);
                continue;
            }

            if ((tagName == "ul" || tagName == "ol") && HasClassContaining(node, "wp-block-list"))
            {
                ProcessList(node, builder);
                continue;
            }

            if (AllowedTags.Contains(tagName))
            {
                ProcessAllowedNode(node, tagName, builder);
                continue;
            }

            PushChildrenInReverse(node, stack);
        }
    }

    private static void ProcessDiv(HtmlNode node, StringBuilder builder, Stack<HtmlNode> stack)
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
        PushChildrenInReverse(node, stack);
    }

    private static void ProcessAnchor(HtmlNode node, StringBuilder builder, Stack<HtmlNode> stack)
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
        PushChildrenInReverse(node, stack);
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
        RemoveUnsafeDescendants(node);
        SanitizeAttributesInSubtree(node);
        builder.AppendLine(node.OuterHtml);
    }

    private static void ProcessAllowedNode(HtmlNode node, string tagName, StringBuilder builder)
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
        RemoveUnsafeDescendants(node);

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
            }
        }

        // THEN clean paragraph in-place (without overriding media-specific classes)
        SanitizeAttributesInSubtree(
            node,
            skipTags: new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "img", "video", "iframe" });
        builder.AppendLine(node.OuterHtml);
    }

    private static void ProcessHeader(HtmlNode node, StringBuilder builder)
    {
        var text = HtmlEntity.DeEntitize(node.InnerText).Trim();

        if (string.IsNullOrWhiteSpace(text))
            return;

        RemoveUnsafeDescendants(node);
        SanitizeAttributesInSubtree(node);
        builder.AppendLine(node.OuterHtml);
    }

    private static void ProcessImage(HtmlNode node, StringBuilder builder)
    {
        var classAttr = node.GetAttributeValue("class", "");

        if (classAttr.Contains("attachment-thumbnail size-thumbnail", StringComparison.OrdinalIgnoreCase)||
            classAttr.Contains("wp-biographia-avatar", StringComparison.OrdinalIgnoreCase) ||
            classAttr.Contains("wp-smiley", StringComparison.OrdinalIgnoreCase) ||  // ← Add this for emoji
            classAttr.Contains("emoji", StringComparison.OrdinalIgnoreCase))
        {
            SanitizeElementAttributes(node, preserveClass: false);
            node.SetAttributeValue("class", "img-fluid w-5 rounded mb-3");
            builder.AppendLine(node.OuterHtml);
        }
        else
        {
            SanitizeElementAttributes(node, preserveClass: false);
            node.SetAttributeValue("class", "img-fluid w-100 rounded mb-3");
            builder.AppendLine(node.OuterHtml);
        }
    }

    private static void ProcessVideo(HtmlNode node, StringBuilder builder)
    {
        RemoveUnsafeDescendants(node);
        SanitizeAttributesInSubtree(node);
        node.SetAttributeValue("class", "w-100 rounded mb-3");
        node.SetAttributeValue("controls", "");

        var sources = node.SelectNodes(".//source");
        if (sources != null)
        {
            foreach (var source in sources)
                SanitizeElementAttributes(source, preserveClass: false);
        }

        var videoHtml = node.OuterHtml;
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

        RemoveUnsafeDescendants(node);
        SanitizeElementAttributes(node, preserveClass: false);
        node.SetAttributeValue("class", "w-100");
        builder.AppendLine($"<div class=\"ratio ratio-16x9 mb-3\">{node.OuterHtml}</div>");
    }

    private static void ProcessListItem(HtmlNode node, StringBuilder builder)
    {
        RemoveUnsafeDescendants(node);
        SanitizeAttributesInSubtree(node);
        builder.AppendLine(node.OuterHtml);
    }

    private static void PushChildrenInReverse(HtmlNode node, Stack<HtmlNode> stack)
    {
        for (var i = node.ChildNodes.Count - 1; i >= 0; i--)
        {
            stack.Push(node.ChildNodes[i]);
        }
    }

    private static void RemoveUnsafeDescendants(HtmlNode node)
    {
        var disallowedTags = node.SelectNodes(".//script | .//style | .//noscript");
        if (disallowedTags == null)
            return;

        foreach (var tag in disallowedTags.ToList())
            tag.Remove();
    }

    private static void SanitizeAttributesInSubtree(HtmlNode root, IReadOnlySet<string>? skipTags = null)
    {
        var stack = new Stack<HtmlNode>();
        stack.Push(root);

        while (stack.Count > 0)
        {
            var node = stack.Pop();

            if (node.NodeType == HtmlNodeType.Element)
            {
                var tagName = node.Name.ToLowerInvariant();
                if (skipTags == null || !skipTags.Contains(tagName))
                {
                    SanitizeElementAttributes(node, preserveClass: false);
                }
            }

            for (var i = node.ChildNodes.Count - 1; i >= 0; i--)
            {
                stack.Push(node.ChildNodes[i]);
            }
        }
    }

    private static void SanitizeElementAttributes(HtmlNode node, bool preserveClass)
    {
        if (node.NodeType != HtmlNodeType.Element)
            return;

        var toRemove = node.Attributes
            .Where(attr =>
                (!preserveClass && AttributesToRemove.Contains(attr.Name)) ||
                attr.Name.StartsWith("on", StringComparison.OrdinalIgnoreCase))
            .Select(attr => attr.Name)
            .ToList();

        foreach (var attrName in toRemove)
        {
            node.Attributes.Remove(attrName);
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
        return RemoveTrailingPostLinksRegex.Replace(content, "");
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

        if (!ContainsHtmlMarkup(html))
            return TrimAfterLastDot(html);

        // -------------------------------------------------------
        // 4. Parse HTML safely
        // -------------------------------------------------------
        var doc = LoadDocument(html);

        var paragraphs = doc.DocumentNode.SelectNodes(".//p");

        if (paragraphs == null || paragraphs.Count == 0)
            return TrimAfterLastDot(doc.DocumentNode.InnerText);

        var texts = paragraphs
    .Select(p => HtmlEntity.DeEntitize(p.InnerText).Trim())
    .Where(t => !string.IsNullOrWhiteSpace(t))
    .Where(t =>
        !AppearedFirstOnTextRegex.IsMatch(t) &&
        !OptimistDailyTextRegex.IsMatch(t) &&
        !StartsWithThePostRegex.IsMatch(t)
    );

        var result = string.Join(" ", texts);

        // Final safety cleanup (text-level)
        result = ThePostSentenceRegex.Replace(result, "");

        result = OptimistDailySentenceRegex.Replace(result, "");

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

        return StrongTildeRegex.Replace(html, "<strong>${left}</strong>");
    }

    private static bool ContainsHtmlMarkup(string input)
    {
        return HtmlTagRegex.IsMatch(input);
    }

    public List<string> CleanTopics(List<string> topics, TopicLookup lookup)
    {
        if (topics == null || topics.Count == 0)
            return [];

        var result = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var raw in topics)
        {
            // Only split and match individual words, don't add the original raw string
            var words = raw
                .Split(new[] { ' ', ',', ';', '&', '/', '|', '-' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(w => w.Trim().ToLowerInvariant())
                .Where(w => w.Length > 2); // Ignore short words like "new", "the", "and"

            foreach (var word in words)
            {
                if (lookup.BySlugWord.TryGetValue(word, out var matchedTopics))
                {
                    foreach (var topic in matchedTopics)
                    {
                        // Only add if the word meaningfully matches
                        if (IsMeaningfulMatch(word, topic))
                        {
                            result.Add(topic.Name);
                        }
                    }
                }
            }
        }

        return result.ToList();
    }

    private bool IsMeaningfulMatch(string word, Topic topic)
    {
        // Don't match common short words
        var commonWords = new[] { "new", "old", "big", "small", "good", "bad", "hot", "cold" };
        if (commonWords.Contains(word))
            return false;

        // Don't match if the word is just a number
        if (int.TryParse(word, out _))
            return false;

        return true;
    }
}