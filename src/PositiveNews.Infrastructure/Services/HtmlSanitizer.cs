using HtmlAgilityPack;
using PositiveNews.Application.DTOs;
using PositiveNews.Application.Interfaces;
using System.Text;

namespace PositiveNews.Infrastructure.Services;

public class HtmlSanitizer : IHtmlSanitizer
{
    private readonly IMediaEmbedder _mediaEmbedder;

    public HtmlSanitizer(IMediaEmbedder mediaEmbedder)
    {
        _mediaEmbedder = mediaEmbedder;
    }

    public string SanitizeContent(HtmlNode rootNode, CommonIngestionRules rules)
    {
        var builder = new StringBuilder();
        var stopProcessing = false;

        ProcessNodesIterative(rootNode, builder, rules, ref stopProcessing);

        return builder.ToString();
    }

    public string? StripToPlainText(string? htmlContent, HtmlNode? htmlNode = null)
    {
        if (htmlNode != null)
            return htmlNode.InnerText;

        if (string.IsNullOrWhiteSpace(htmlContent))
            return htmlContent;

        var doc = new HtmlDocument();
        doc.LoadHtml(htmlContent);
        return doc.DocumentNode.InnerText;
    }

    private void ProcessNodesIterative(
        HtmlNode root, StringBuilder builder, CommonIngestionRules rules, ref bool stopProcessing)
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

            if (ShouldStopProcessing(node, rules.StopProcessingPatterns))
            {
                stopProcessing = true;
                return;
            }

            if (ShouldRemoveNode(node, rules.RemoveNodePatterns, rules.RemoveDivClassPatterns))
                continue;

            var tagName = node.Name.ToLowerInvariant();

            if (tagName == "div")
            {
                ProcessDiv(node, builder, stack, rules.RemoveDivClassPatterns);
                continue;
            }

            if (tagName == "a")
            {
                if (!_mediaEmbedder.TryEmbed(node, builder))
                    PushChildrenInReverse(node, stack);
                continue;
            }

            if ((tagName == "ul" || tagName == "ol") && HasClassContaining(node, "wp-block-list"))
            {
                ProcessList(node, builder);
                continue;
            }

            if (rules.AllowedTags.Contains(tagName))
            {
                ProcessAllowedNode(node, tagName, builder, rules);
                continue;
            }

            PushChildrenInReverse(node, stack);
        }
    }

    private void ProcessDiv(
        HtmlNode node, StringBuilder builder, Stack<HtmlNode> stack, IReadOnlyList<string> removeDivPatterns)
    {
        foreach (var pattern in removeDivPatterns)
        {
            if (HasClassContaining(node, pattern))
                return;
        }

        if (HasClassContaining(node, "hds-caption-text"))
        {
            var text = HtmlEntity.DeEntitize(node.InnerText).Trim();
            if (!string.IsNullOrWhiteSpace(text))
                builder.AppendLine($"<p class=\"small fst-italic\">{System.Net.WebUtility.HtmlEncode(text)}</p>");
            return;
        }

        PushChildrenInReverse(node, stack);
    }

    private void ProcessAllowedNode(HtmlNode node, string tagName, StringBuilder builder, CommonIngestionRules rules)
    {
        switch (tagName)
        {
            case "p":
                ProcessParagraph(node, builder, rules);
                break;
            case "h1":
            case "h2":
            case "h3":
            case "h4":
                ProcessHeader(node, builder);
                break;
            case "img":
                _mediaEmbedder.EmbedImage(node, builder);
                break;
            case "video":
                _mediaEmbedder.EmbedVideo(node, builder);
                break;
            case "ul":
            case "ol":
                ProcessList(node, builder);
                break;
            case "li":
                ProcessListItem(node, builder);
                break;
            case "iframe":
                _mediaEmbedder.EmbedIframe(node, builder);
                break;
        }
    }

    private void ProcessParagraph(HtmlNode node, StringBuilder builder, CommonIngestionRules rules)
    {
        RemoveUnsafeDescendants(node);

        var iframes = node.SelectNodes(".//iframe");
        if (iframes != null)
        {
            foreach (var iframe in iframes)
                _mediaEmbedder.EmbedIframe(iframe, builder);
        }

        var text = HtmlEntity.DeEntitize(node.InnerText).Trim();

        var hasMedia = node.SelectNodes(".//img | .//video | .//iframe") != null;
        var hasText = !string.IsNullOrWhiteSpace(text);

        if (!hasText && iframes != null && node.SelectNodes(".//img | .//video") == null)
            return;

        if (!hasText && !hasMedia)
            return;

        if (hasText && ShouldRemoveParagraph(text, rules.ShouldRemoveParagraphPatterns))
            return;

        var images = node.SelectNodes(".//img");
        if (images != null)
        {
            foreach (var img in images.ToList())
            {
                var imgBuilder = new StringBuilder();
                _mediaEmbedder.EmbedImage(img, imgBuilder);
            }
        }

        SanitizeAttributesInSubtree(
            node,
            rules.AttributesToRemove,
            skipTags: new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "img", "video", "iframe" });
        builder.AppendLine(node.OuterHtml);
    }

    private static void ProcessHeader(HtmlNode node, StringBuilder builder)
    {
        var text = HtmlEntity.DeEntitize(node.InnerText).Trim();
        if (string.IsNullOrWhiteSpace(text))
            return;

        RemoveUnsafeDescendants(node);
        SanitizeAttributesInSubtree(node, null);
        builder.AppendLine(node.OuterHtml);
    }

    private static void ProcessList(HtmlNode node, StringBuilder builder)
    {
        RemoveUnsafeDescendants(node);
        SanitizeAttributesInSubtree(node, null);
        builder.AppendLine(node.OuterHtml);
    }

    private static void ProcessListItem(HtmlNode node, StringBuilder builder)
    {
        RemoveUnsafeDescendants(node);
        SanitizeAttributesInSubtree(node, null);
        builder.AppendLine(node.OuterHtml);
    }

    private static bool ShouldStopProcessing(HtmlNode node, IReadOnlyList<string> stopPatterns)
    {
        var text = HtmlEntity.DeEntitize(node.InnerText).Trim();

        if (text.Equals("Share", StringComparison.OrdinalIgnoreCase))
            return true;

        foreach (var pattern in stopPatterns)
        {
            if (text.Contains(pattern, StringComparison.OrdinalIgnoreCase))
                return true;
        }

        return false;
    }

    private static bool ShouldRemoveNode(
        HtmlNode node, IReadOnlyList<string> removeNodePatterns, IReadOnlyList<string> removeDivClassPatterns)
    {
        var text = HtmlEntity.DeEntitize(node.InnerText);

        foreach (var pattern in removeNodePatterns)
        {
            if (text.Contains(pattern, StringComparison.OrdinalIgnoreCase))
                return true;
        }

        foreach (var pattern in removeDivClassPatterns)
        {
            if (HasClassContaining(node, pattern))
                return true;
        }

        var tagName = node.Name.ToLowerInvariant();

        if (tagName == "img" && HasExactClass(node, "attachment-thumbnail size-thumbnail"))
            return true;

        if (tagName == "script")
            return true;

        return false;
    }

    private static bool ShouldRemoveParagraph(string text, IReadOnlyList<string> removeParagraphPatterns)
    {
        var lower = text.ToLowerInvariant();

        foreach (var pattern in removeParagraphPatterns)
        {
            if (lower.Contains(pattern, StringComparison.OrdinalIgnoreCase))
                return true;
        }

        return false;
    }

    private static void PushChildrenInReverse(HtmlNode node, Stack<HtmlNode> stack)
    {
        for (var i = node.ChildNodes.Count - 1; i >= 0; i--)
            stack.Push(node.ChildNodes[i]);
    }

    private static void RemoveUnsafeDescendants(HtmlNode node)
    {
        var disallowedTags = node.SelectNodes(".//script | .//style | .//noscript");
        if (disallowedTags == null) return;

        foreach (var tag in disallowedTags.ToList())
            tag.Remove();
    }

    private static void SanitizeAttributesInSubtree(
        HtmlNode root, IReadOnlySet<string>? attributesToRemove, IReadOnlySet<string>? skipTags = null)
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
                    SanitizeElementAttributes(node, attributesToRemove);
            }

            for (var i = node.ChildNodes.Count - 1; i >= 0; i--)
                stack.Push(node.ChildNodes[i]);
        }
    }

    private static void SanitizeElementAttributes(HtmlNode node, IReadOnlySet<string>? attributesToRemove)
    {
        if (node.NodeType != HtmlNodeType.Element) return;

        var defaultAttrs = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "class", "style", "block_context" };
        var attrsToRemove = attributesToRemove ?? defaultAttrs;

        var toRemove = node.Attributes
            .Where(attr =>
                attrsToRemove.Contains(attr.Name) ||
                attr.Name.StartsWith("on", StringComparison.OrdinalIgnoreCase))
            .Select(attr => attr.Name)
            .ToList();

        foreach (var attrName in toRemove)
            node.Attributes.Remove(attrName);
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
}
