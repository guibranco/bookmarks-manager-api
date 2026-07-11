using System.Net;
using System.Net.Http.Json;
using BookmarksManager.Api.Dtos;

namespace BookmarksManager.Api.Tests.Integration;

public class BookmarksEndpointsTests(ApiWebApplicationFactory factory)
    : IClassFixture<ApiWebApplicationFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    private static BookmarkRequest ValidRequest(string title = "Example") => new()
    {
        Title = title,
        Url = "https://example.com",
    };

    private HttpRequestMessage AuthorizedRequest(HttpMethod method, string uri, object? body = null)
    {
        var request = new HttpRequestMessage(method, uri);
        request.Headers.Add("X-Api-Key", ApiWebApplicationFactory.ApiKey);
        if (body is not null)
        {
            request.Content = JsonContent.Create(body);
        }

        return request;
    }

    [Fact]
    public async Task Get_all_bookmarks_does_not_require_authentication()
    {
        var response = await _client.GetAsync("/api/bookmarks");

        response.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task Get_bookmark_by_id_returns_not_found_for_unknown_id_without_authentication()
    {
        var response = await _client.GetAsync("/api/bookmarks/does-not-exist");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Post_bookmark_without_api_key_returns_unauthorized()
    {
        var response = await _client.PostAsJsonAsync("/api/bookmarks", ValidRequest());

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Post_bookmark_with_wrong_api_key_returns_unauthorized()
    {
        var request = new HttpRequestMessage(HttpMethod.Post, "/api/bookmarks")
        {
            Content = JsonContent.Create(ValidRequest()),
        };
        request.Headers.Add("X-Api-Key", "wrong-key");

        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Post_bookmark_with_valid_api_key_creates_and_returns_it()
    {
        var response = await _client.SendAsync(
            AuthorizedRequest(HttpMethod.Post, "/api/bookmarks", ValidRequest("Created via test")));

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var created = await response.Content.ReadFromJsonAsync<BookmarkDto>();
        Assert.NotNull(created);
        Assert.Equal("Created via test", created!.Title);
    }

    [Fact]
    public async Task Put_and_delete_bookmark_require_authentication_end_to_end()
    {
        var createResponse = await _client.SendAsync(
            AuthorizedRequest(HttpMethod.Post, "/api/bookmarks", ValidRequest("To update")));
        var created = await createResponse.Content.ReadFromJsonAsync<BookmarkDto>();

        var unauthorizedUpdate = await _client.PutAsJsonAsync($"/api/bookmarks/{created!.Id}", ValidRequest("Blocked"));
        Assert.Equal(HttpStatusCode.Unauthorized, unauthorizedUpdate.StatusCode);

        var authorizedUpdate = await _client.SendAsync(
            AuthorizedRequest(HttpMethod.Put, $"/api/bookmarks/{created.Id}", ValidRequest("Updated")));
        Assert.Equal(HttpStatusCode.OK, authorizedUpdate.StatusCode);

        var unauthorizedDelete = await _client.DeleteAsync($"/api/bookmarks/{created.Id}");
        Assert.Equal(HttpStatusCode.Unauthorized, unauthorizedDelete.StatusCode);

        var authorizedDelete = await _client.SendAsync(
            AuthorizedRequest(HttpMethod.Delete, $"/api/bookmarks/{created.Id}"));
        Assert.Equal(HttpStatusCode.NoContent, authorizedDelete.StatusCode);

        var getAfterDelete = await _client.GetAsync($"/api/bookmarks/{created.Id}");
        Assert.Equal(HttpStatusCode.NotFound, getAfterDelete.StatusCode);
    }
}
