using Microsoft.AspNetCore.Razor.TagHelpers;
using HtmlAgilityPack;

[HtmlTargetElement("article-img")]
public class ArticleImageTagHelper : TagHelper
{
    public string? ImageTag { get; set; }

    public int Index { get; set; }

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        if (string.IsNullOrWhiteSpace(ImageTag))
        {
            output.SuppressOutput();
            return;
        }

        var doc = new HtmlDocument();
        doc.LoadHtml(ImageTag);

        var img = doc.DocumentNode.SelectSingleNode("//img");
        if (img == null)
        {
            output.SuppressOutput();
            return;
        }

        // 🚀 Dynamic strategies
        var loading = Index < 3 ? "eager" : "lazy";

        img.SetAttributeValue("loading", loading);

        output.TagName = null; // remove <article-img>
        output.Content.SetHtmlContent(img.OuterHtml);
    }
}