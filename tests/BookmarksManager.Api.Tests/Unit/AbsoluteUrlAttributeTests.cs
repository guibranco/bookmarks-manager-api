using BookmarksManager.Api.Common;

namespace BookmarksManager.Api.Tests.Unit;

public class AbsoluteUrlAttributeTests
{
    private readonly AbsoluteUrlAttribute _sut = new();

    [Theory]
    [InlineData("https://example.com")]
    [InlineData("http://example.com/path?query=1")]
    public void IsValid_returns_true_for_absolute_http_or_https_urls(string url)
    {
        Assert.True(_sut.IsValid(url));
    }

    [Theory]
    [InlineData("/relative/path")]
    [InlineData("not-a-url")]
    [InlineData("ftp://example.com")]
    [InlineData(null)]
    [InlineData("")]
    public void IsValid_returns_false_for_non_absolute_http_urls(string? url)
    {
        Assert.False(_sut.IsValid(url));
    }
}
