using HtmlAgilityPack;
using System.Text;

namespace PositiveNews.Application.Interfaces;

public interface IMediaEmbedder
{
    bool TryEmbed(HtmlNode node, StringBuilder builder);
    void EmbedImage(HtmlNode imgNode, StringBuilder builder);
    void EmbedVideo(HtmlNode videoNode, StringBuilder builder);
    void EmbedIframe(HtmlNode iframeNode, StringBuilder builder);
    string CreateYouTubeEmbed(string videoId);
}
