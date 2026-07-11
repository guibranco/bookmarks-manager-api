using BookmarksManager.Api.Common;
using BookmarksManager.Api.Dtos;
using BookmarksManager.Api.Models;
using BookmarksManager.Api.Services;
using BookmarksManager.Api.Tests.Support;

namespace BookmarksManager.Api.Tests.Unit;

public class BookmarkServiceTests : IDisposable
{
    private readonly SqliteDbContext _fixture = new();
    private readonly BookmarkService _sut;

    public BookmarkServiceTests()
    {
        _sut = new BookmarkService(_fixture.Db);
    }

    public void Dispose() => _fixture.Dispose();

    private static BookmarkRequest ValidRequest(string title = "Example", string url = "https://example.com") =>
        new()
        {
            Title = title,
            Url = url,
            Description = "  a description  ",
            Thumbnail = "",
            Tags = ["b", "a", "a", "  ", "b"],
            FolderId = null,
            Favorite = true,
        };

    [Fact]
    public async Task CreateAsync_persists_bookmark_with_trimmed_fields_and_normalized_tags()
    {
        var created = await _sut.CreateAsync(ValidRequest());

        Assert.NotEmpty(created.Id);
        Assert.Equal("a description", created.Description);
        Assert.Equal(["a", "b"], created.Tags);
        Assert.True(created.Favorite);
    }

    [Fact]
    public async Task CreateAsync_throws_validation_exception_when_folder_does_not_exist()
    {
        var request = ValidRequest();
        request.FolderId = "missing-folder";

        var ex = await Assert.ThrowsAsync<AppValidationException>(() => _sut.CreateAsync(request));

        Assert.True(ex.Errors.ContainsKey(nameof(BookmarkRequest.FolderId)));
    }

    [Fact]
    public async Task CreateAsync_throws_validation_exception_when_title_is_missing()
    {
        var request = ValidRequest();
        request.Title = "";

        await Assert.ThrowsAsync<AppValidationException>(() => _sut.CreateAsync(request));
    }

    [Fact]
    public async Task GetByIdAsync_throws_not_found_exception_when_missing()
    {
        await Assert.ThrowsAsync<NotFoundException>(() => _sut.GetByIdAsync("missing"));
    }

    [Fact]
    public async Task UpdateAsync_replaces_fields_and_retags_bookmark()
    {
        var created = await _sut.CreateAsync(ValidRequest());

        var updateRequest = ValidRequest("Renamed", "https://renamed.example.com");
        updateRequest.Tags = ["only-tag"];
        var updated = await _sut.UpdateAsync(created.Id, updateRequest);

        Assert.Equal("Renamed", updated.Title);
        Assert.Equal("https://renamed.example.com", updated.Url);
        Assert.Equal(["only-tag"], updated.Tags);
    }

    [Fact]
    public async Task DeleteAsync_removes_bookmark()
    {
        var created = await _sut.CreateAsync(ValidRequest());

        await _sut.DeleteAsync(created.Id);

        await Assert.ThrowsAsync<NotFoundException>(() => _sut.GetByIdAsync(created.Id));
    }

    [Fact]
    public async Task GetAllAsync_returns_all_created_bookmarks()
    {
        await _sut.CreateAsync(ValidRequest("First", "https://first.example.com"));
        await _sut.CreateAsync(ValidRequest("Second", "https://second.example.com"));

        var all = await _sut.GetAllAsync();

        Assert.Equal(2, all.Count);
        Assert.Contains(all, b => b.Title == "First");
        Assert.Contains(all, b => b.Title == "Second");
    }
}
