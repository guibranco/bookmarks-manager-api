using System.Net;
using System.Net.Http.Json;
using BookmarksManager.Api.Dtos;

namespace BookmarksManager.Api.Tests.Integration;

public class FoldersEndpointsTests(ApiWebApplicationFactory factory)
    : IClassFixture<ApiWebApplicationFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

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
    public async Task Get_all_folders_does_not_require_authentication()
    {
        var response = await _client.GetAsync("/api/folders");

        response.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task Post_folder_without_api_key_returns_unauthorized()
    {
        var response = await _client.PostAsJsonAsync("/api/folders", new FolderRequest { Name = "Blocked" });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Post_folder_with_valid_api_key_creates_and_returns_it()
    {
        var response = await _client.SendAsync(
            AuthorizedRequest(HttpMethod.Post, "/api/folders", new FolderRequest { Name = "Reading list" }));

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var created = await response.Content.ReadFromJsonAsync<FolderDto>();
        Assert.Equal("Reading list", created!.Name);
    }

    [Fact]
    public async Task Delete_folder_with_bookmarks_without_cascade_returns_conflict()
    {
        var folderResponse = await _client.SendAsync(
            AuthorizedRequest(HttpMethod.Post, "/api/folders", new FolderRequest { Name = "Non-empty" }));
        var folder = await folderResponse.Content.ReadFromJsonAsync<FolderDto>();

        await _client.SendAsync(AuthorizedRequest(HttpMethod.Post, "/api/bookmarks", new BookmarkRequest
        {
            Title = "Inside folder",
            Url = "https://example.com",
            FolderId = folder!.Id,
        }));

        var deleteResponse = await _client.SendAsync(
            AuthorizedRequest(HttpMethod.Delete, $"/api/folders/{folder.Id}"));

        Assert.Equal(HttpStatusCode.Conflict, deleteResponse.StatusCode);
    }

    [Fact]
    public async Task Delete_folder_with_cascade_true_detaches_bookmarks_and_succeeds()
    {
        var folderResponse = await _client.SendAsync(
            AuthorizedRequest(HttpMethod.Post, "/api/folders", new FolderRequest { Name = "Cascade me" }));
        var folder = await folderResponse.Content.ReadFromJsonAsync<FolderDto>();

        var deleteResponse = await _client.SendAsync(
            AuthorizedRequest(HttpMethod.Delete, $"/api/folders/{folder!.Id}?cascade=true"));

        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);
    }
}
