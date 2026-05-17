using HtmlAgilityPack;
using System.Text;

namespace PositiveNews.Application.Interfaces;

/// <summary>
/// Converts media-related HTML nodes into safe embedded markup for article bodies.
/// </summary>
public interface IMediaEmbedder
{
    /// <summary>
    /// Attempts to embed supported media from the node into the string builder.
    /// </summary>
    /// <param name="node">HTML node under consideration.</param>
    /// <param name="builder">Output buffer for embedded markup.</param>
    /// <returns><see langword="true"/> when the node was handled.</returns>
    bool TryEmbed(HtmlNode node, StringBuilder builder);

    /// <summary>
    /// Appends sanitized image markup for an <c>img</c> node.
    /// </summary>
    /// <param name="imgNode">Image element.</param>
    /// <param name="builder">Output buffer.</param>
    void EmbedImage(HtmlNode imgNode, StringBuilder builder);

    /// <summary>
    /// Appends markup for a native <c>video</c> element.
    /// </summary>
    /// <param name="videoNode">Video element.</param>
    /// <param name="builder">Output buffer.</param>
    void EmbedVideo(HtmlNode videoNode, StringBuilder builder);

    /// <summary>
    /// Appends embedded iframe content when allowed (e.g. video hosts).
    /// </summary>
    /// <param name="iframeNode">Iframe element.</param>
    /// <param name="builder">Output buffer.</param>
    void EmbedIframe(HtmlNode iframeNode, StringBuilder builder);

    /// <summary>
    /// Builds a standard YouTube embed snippet from a video identifier.
    /// </summary>
    /// <param name="videoId">YouTube video id.</param>
    /// <returns>Embed HTML string.</returns>
    string CreateYouTubeEmbed(string videoId);
}
