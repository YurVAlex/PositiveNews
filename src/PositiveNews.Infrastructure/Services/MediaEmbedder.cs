using HtmlAgilityPack;
using PositiveNews.Application.Abstractions.IngestionPipeline;
using System.Text;
using System.Text.RegularExpressions;

namespace PositiveNews.Infrastructure.Services;

/// <summary>
/// Rewrites anchors, images, video, and iframe nodes into Bootstrap-friendly markup and YouTube embeds when applicable.
/// </summary>
public class MediaEmbedder : IMediaEmbedder
{
    private static readonly Regex YoutubeRegex = new(
        @"(?:https?://)?(?:www\.)?(?:youtube\.com/watch\?v=|youtu\.be/|youtube\.com/embed/)([a-zA-Z0-9_-]{11})",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    /// <inheritdoc />
    public bool TryEmbed(HtmlNode node, StringBuilder builder)
    {
        var tagName = node.Name.ToLowerInvariant();

        return tagName switch
        {
            "a" => TryEmbedAnchor(node, builder),
            "iframe" => TryEmbedIframe(node, builder),
            _ => false
        };
    }

    /// <inheritdoc />
    public void EmbedImage(HtmlNode imgNode, StringBuilder builder)
    {
        var classAttr = imgNode.GetAttributeValue("class", "");

        var isSmallImage =
            classAttr.Contains("attachment-thumbnail size-thumbnail", StringComparison.OrdinalIgnoreCase) ||
            classAttr.Contains("wp-biographia-avatar", StringComparison.OrdinalIgnoreCase) ||
            classAttr.Contains("wp-smiley", StringComparison.OrdinalIgnoreCase) ||
            classAttr.Contains("emoji", StringComparison.OrdinalIgnoreCase);

        SanitizeElementAttributes(imgNode);

        imgNode.SetAttributeValue("class", isSmallImage
            ? "img-fluid w-5 rounded mb-3"
            : "img-fluid w-100 rounded mb-3");

        builder.AppendLine(imgNode.OuterHtml);
    }

    /// <inheritdoc />
    public void EmbedVideo(HtmlNode videoNode, StringBuilder builder)
    {
        RemoveUnsafeDescendants(videoNode);
        SanitizeAttributesInSubtree(videoNode);
        videoNode.SetAttributeValue("class", "w-100 rounded mb-3");
        videoNode.SetAttributeValue("controls", "");

        var sources = videoNode.SelectNodes(".//source");
        if (sources != null)
        {
            foreach (var source in sources)
                SanitizeElementAttributes(source);
        }

        builder.AppendLine($"<div class=\"ratio ratio-16x9 mb-3\">{videoNode.OuterHtml}</div>");
    }

    /// <inheritdoc />
    public void EmbedIframe(HtmlNode iframeNode, StringBuilder builder)
    {
        var src = iframeNode.GetAttributeValue("src", "");

        var youtubeMatch = YoutubeRegex.Match(src);
        if (youtubeMatch.Success)
        {
            builder.AppendLine(CreateYouTubeEmbed(youtubeMatch.Groups[1].Value));
            return;
        }

        RemoveUnsafeDescendants(iframeNode);
        SanitizeElementAttributes(iframeNode);
        iframeNode.SetAttributeValue("class", "w-100");
        builder.AppendLine($"<div class=\"ratio ratio-16x9 mb-3\">{iframeNode.OuterHtml}</div>");
    }

    /// <inheritdoc />
    public string CreateYouTubeEmbed(string videoId)
    {
        return $@"<div class=""ratio ratio-16x9 mb-3"">
                <iframe src=""https://www.youtube.com/embed/{videoId}"" 
                title=""YouTube video"" 
                allow=""accelerometer; autoplay; clipboard-write; encrypted-media; gyroscope; picture-in-picture; web-share"" 
                allowfullscreen>
                </iframe>
                </div>";
    }

    private bool TryEmbedAnchor(HtmlNode node, StringBuilder builder)
    {
        var href = node.GetAttributeValue("href", "");

        var youtubeMatch = YoutubeRegex.Match(href);
        if (youtubeMatch.Success)
        {
            builder.AppendLine(CreateYouTubeEmbed(youtubeMatch.Groups[1].Value));
            return true;
        }

        var innerMatch = YoutubeRegex.Match(node.InnerHtml);
        if (innerMatch.Success)
        {
            builder.AppendLine(CreateYouTubeEmbed(innerMatch.Groups[1].Value));
            return true;
        }

        return false;
    }

    private bool TryEmbedIframe(HtmlNode node, StringBuilder builder)
    {
        EmbedIframe(node, builder);
        return true;
    }

    private static void RemoveUnsafeDescendants(HtmlNode node)
    {
        var disallowedTags = node.SelectNodes(".//script | .//style | .//noscript");
        if (disallowedTags == null) return;

        foreach (var tag in disallowedTags.ToList())
            tag.Remove();
    }

    private static void SanitizeAttributesInSubtree(HtmlNode root)
    {
        var stack = new Stack<HtmlNode>();
        stack.Push(root);

        while (stack.Count > 0)
        {
            var node = stack.Pop();

            if (node.NodeType == HtmlNodeType.Element)
                SanitizeElementAttributes(node);

            for (var i = node.ChildNodes.Count - 1; i >= 0; i--)
                stack.Push(node.ChildNodes[i]);
        }
    }

    private static void SanitizeElementAttributes(HtmlNode node)
    {
        if (node.NodeType != HtmlNodeType.Element) return;

        var toRemove = node.Attributes
            .Where(attr =>
                attr.Name.Equals("class", StringComparison.OrdinalIgnoreCase) ||
                attr.Name.Equals("style", StringComparison.OrdinalIgnoreCase) ||
                attr.Name.Equals("block_context", StringComparison.OrdinalIgnoreCase) ||
                attr.Name.StartsWith("on", StringComparison.OrdinalIgnoreCase))
            .Select(attr => attr.Name)
            .ToList();

        foreach (var attrName in toRemove)
            node.Attributes.Remove(attrName);
    }
}
