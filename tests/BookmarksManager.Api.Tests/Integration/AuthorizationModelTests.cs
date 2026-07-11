using System.Net;
using System.Net.Http.Json;

namespace BookmarksManager.Api.Tests.Integration;

/// <summary>
/// Locks in the API's authorization model end-to-end: every GET is public, every
/// create/update/delete requires the X-Api-Key header.
/// </summary>
public class AuthorizationModelTests(ApiWebApplicationFactory factory)
    : IClassFixture<ApiWebApplicationFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    public static IEnumerable<object[]> PublicGetRoutes =>
    [
        ["/api/bookmarks"],
        ["/api/folders"],
        ["/api/tags"],
        ["/health"],
    ];

    public static IEnumerable<object[]> ProtectedWriteRoutes =>
    [
        [HttpMethod.Post, "/api/bookmarks"],
        [HttpMethod.Put, "/api/bookmarks/any-id"],
        [HttpMethod.Delete, "/api/bookmarks/any-id"],
        [HttpMethod.Post, "/api/folders"],
        [HttpMethod.Put, "/api/folders/any-id"],
        [HttpMethod.Delete, "/api/folders/any-id"],
    ];

    [Theory]
    [MemberData(nameof(PublicGetRoutes))]
    public async Task Get_endpoints_never_return_unauthorized(string route)
    {
        var response = await _client.GetAsync(route);

        Assert.NotEqual(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Theory]
    [MemberData(nameof(ProtectedWriteRoutes))]
    public async Task Write_endpoints_return_unauthorized_without_an_api_key(HttpMethod method, string route)
    {
        var request = new HttpRequestMessage(method, route) { Content = JsonContent.Create(new { }) };

        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Theory]
    [MemberData(nameof(ProtectedWriteRoutes))]
    public async Task Write_endpoints_return_unauthorized_with_a_wrong_api_key(HttpMethod method, string route)
    {
        var request = new HttpRequestMessage(method, route) { Content = JsonContent.Create(new { }) };
        request.Headers.Add("X-Api-Key", "definitely-wrong");

        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
