using HtmlAgilityPack;
using PositiveNews.Application.DTOs;
using PositiveNews.Application.Interfaces;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;

namespace PositiveNews.Infrastructure.Services
{
    internal class FeedItemCleaner : IFeedItemCleaner
    {
        public RssFeedItemDto Clean(RssFeedItemDto dto)
        {
            dto.Description = CleanDescription(dto.Description);
            dto.ContentClean = CleanContent(dto.ContentRaw);

            return dto;
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

            return string.Join(" ", texts);
        }

        private static string CleanContent(string rawContent)
        {
            var doc = LoadDocument(rawContent);

            var builder = new StringBuilder();

            AppendParagraphs(doc, builder);
            AppendImages(doc, builder);

            var cleaned = RemoveTrailingPostLinks(builder.ToString());

            return cleaned.Trim();
        }

        private static HtmlDocument LoadDocument(string html)
        {
            var doc = new HtmlDocument();
            doc.LoadHtml(html);
            return doc;
        }

        private static void AppendParagraphs(HtmlDocument doc, StringBuilder builder)
        {
            var paragraphs = doc.DocumentNode.SelectNodes("//p");

            if (paragraphs == null)
                return;

            foreach (var p in paragraphs)
            {
                var text = HtmlEntity.DeEntitize(p.InnerText).Trim();

                if (string.IsNullOrWhiteSpace(text))
                    continue;

                if (ShouldRemoveParagraph(text))
                    continue;

                RemoveLinks(p);

                builder.AppendLine(p.OuterHtml);
            }
        }

        private static void AppendImages(HtmlDocument doc, StringBuilder builder)
        {
            var images = doc.DocumentNode.SelectNodes("//img");

            if (images == null)
                return;

            foreach (var img in images)
                builder.AppendLine(img.OuterHtml);
        }

        private static bool ShouldRemoveParagraph(string text)
        {
            var lower = text.ToLowerInvariant();

            return lower.Contains("did this solution stand out")
                || lower.Contains("becoming an emissary")
                || lower.StartsWith("the post ")
                || lower.Contains("appeared first on");
        }

        private static void RemoveLinks(HtmlNode node)
        {
            var links = node.SelectNodes(".//a");

            if (links == null)
                return;

            foreach (var link in links)
            {
                var textNode = HtmlTextNode.CreateNode(link.InnerText);
                link.ParentNode.ReplaceChild(textNode, link);
            }
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