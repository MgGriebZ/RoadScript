using RoadScript.Services;

namespace RoadScript.Tests;

public class MarkdownRendererTests
{
    [Fact]
    public void Text_can_wrap_after_a_slash()
    {
        var html = MarkdownRenderer.RenderToHtml("- Azure Hosting/Services");

        Assert.Contains("Hosting/<wbr>Services", html);
    }

    [Fact]
    public void Link_addresses_keep_their_slashes()
    {
        var html = MarkdownRenderer.RenderToHtml("See https://roadscript.net/showcase/portfolio");

        Assert.Contains("href=\"https://roadscript.net/showcase/portfolio\"", html);
    }

    [Fact]
    public void Formatting_tags_are_not_split()
    {
        var html = MarkdownRenderer.RenderToHtml("**Dart/GO** and `src/app`");

        Assert.Contains("<strong style=\"font-weight:700;\">Dart/<wbr>GO</strong>", html);
        Assert.Contains("src/<wbr>app</code>", html);
        Assert.DoesNotContain("</<wbr>", html);
    }
}
