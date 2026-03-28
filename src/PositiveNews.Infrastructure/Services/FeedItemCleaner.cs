using HtmlAgilityPack;
using PositiveNews.Application.DTOs;
using PositiveNews.Application.Interfaces;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using static Microsoft.CodeAnalysis.CSharp.SyntaxTokenParser;

namespace PositiveNews.Infrastructure.Services
{
    public class FeedItemCleaner : IFeedItemCleaner
    {
        private static readonly HashSet<string> AllowedTags = new(StringComparer.OrdinalIgnoreCase)
        {
            "h1", "h2", "h3", "h4", "p", "img", "video", "source", "ul", "ol", "li"
        };

        private static readonly HashSet<string> AttributesToRemove = new(StringComparer.OrdinalIgnoreCase)
        {
            "class", "style", "block_context"
        };

        public void Clean(RssFeedItemDto dto)
        {
            dto.Description = CleanDescription(dto.Description);
            dto.ContentClean = CleanContent(dto.ContentRaw);
            dto.Title = CleanTitle(dto.Title);
        }
        private static string CleanTitle(string title)
        {
            return title.Length > 500 ? title[..500] : title;
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



        

        // Patterns that trigger "stop processing all following content"
        private static readonly string[] StopProcessingPatterns =
        {
    "About the Author",
    "For more information about",
    "To learn more about",
    "Discover More Topics From",
    "For more information on",
    "send a message to:",
    "Want to be part of the Optimism Movement?",
    "Subscribe to",
    "follow us on",
    "Donate link:",
    "Click here to leave a comment on the site"
};

        // Patterns that trigger removal of a single node
        private static readonly string[] RemoveNodePatterns =
        {
    "Learn more about this image",
    "Listen to this audio"
};

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

                // Handle lists with wp-block-list class
                if ((tagName == "ul" || tagName == "ol") && HasClassStartingWith(child, "wp-block-list"))
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
            // Transform hds-caption-text divs to italic h4
            if (HasClassStartingWith(node, "hds-caption-text"))
            {
                var text = HtmlEntity.DeEntitize(node.InnerText).Trim();
                if (!string.IsNullOrWhiteSpace(text))
                {
                    builder.AppendLine($"<h4><em>{System.Net.WebUtility.HtmlEncode(text)}</em></h4>");
                }
                return;
            }

            // Skip divs that should be removed
            if (HasClassStartingWith(node, "hds-featured-file-list") ||
                HasClassStartingWith(node, "hds-audio-player-"))
            {
                return;
            }

            // Otherwise, process children of the div
            ProcessNodesRecursively(node, builder, ref stopProcessing);
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
                    // li should be handled as part of ul/ol, but if standalone:
                    ProcessListItem(node, builder);
                    break;
            }
        }

        private static void ProcessParagraph(HtmlNode node, StringBuilder builder)
        {
            var text = HtmlEntity.DeEntitize(node.InnerText).Trim();

            var hasMedia = node.SelectNodes(".//img | .//video") != null;
            var hasText = !string.IsNullOrWhiteSpace(text);

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
            // Skip thumbnail images
            if (HasExactClass(node, "attachment-thumbnail size-thumbnail"))
                return;

            var cleanedNode = CleanAttributes(node);
            cleanedNode.SetAttributeValue("class", "img-fluid w-100 rounded mb-3");
            builder.AppendLine(cleanedNode.OuterHtml);
        }

        private static void ProcessVideo(HtmlNode node, StringBuilder builder)
        {
            var cleanedNode = CleanAttributes(node);

            var sources = cleanedNode.SelectNodes(".//source");
            if (sources != null)
            {
                foreach (var source in sources)
                    CleanAttributesRecursively(source);
            }

            builder.AppendLine(cleanedNode.OuterHtml);
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

        private static bool HasClassStartingWith(HtmlNode node, string prefix)
        {
            var classAttr = node.GetAttributeValue("class", "");
            if (string.IsNullOrEmpty(classAttr))
                return false;

            var classes = classAttr.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            return classes.Any(c => c.StartsWith(prefix, StringComparison.OrdinalIgnoreCase));
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

            // Check for divs/elements that should be removed by class
            var tagName = node.Name.ToLowerInvariant();

            if (tagName == "div" &&
                (HasClassStartingWith(node, "hds-featured-file-list") ||
                 HasClassStartingWith(node, "hds-audio-player-")))
            {
                return true;
            }

            // Check for thumbnail images
            if (tagName == "img" && HasExactClass(node, "attachment-thumbnail size-thumbnail"))
            {
                return true;
            }

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

    }
}