namespace BookmarksManager.Api.Tests.Integration;

public class TagsAndHealthEndpointsTests(ApiWebApplicationFactory factory)
    : IClassFixture<ApiWebApplicationFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task Get_tags_does_not_require_authentication()
    {
        var response = await _client.GetAsync("/api/tags");

        response.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task Get_health_does_not_require_authentication()
    {
        var response = await _client.GetAsync("/health");

        response.EnsureSuccessStatusCode();
    }
}
