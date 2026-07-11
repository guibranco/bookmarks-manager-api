using BookmarksManager.Api.Common;
using BookmarksManager.Api.Dtos;
using BookmarksManager.Api.Services;
using BookmarksManager.Api.Tests.Support;

namespace BookmarksManager.Api.Tests.Unit;

public class FolderServiceTests : IDisposable
{
    private readonly SqliteDbContext _fixture = new();
    private readonly FolderService _sut;
    private readonly BookmarkService _bookmarks;

    public FolderServiceTests()
    {
        _sut = new FolderService(_fixture.Db);
        _bookmarks = new BookmarkService(_fixture.Db);
    }

    public void Dispose() => _fixture.Dispose();

    private static FolderRequest ValidRequest(string name = "Folder", string? parentId = null) =>
        new() { Name = name, ParentId = parentId };

    [Fact]
    public async Task CreateAsync_persists_folder()
    {
        var created = await _sut.CreateAsync(ValidRequest("Reading list"));

        Assert.NotEmpty(created.Id);
        Assert.Equal("Reading list", created.Name);
        Assert.Null(created.ParentId);
    }

    [Fact]
    public async Task CreateAsync_throws_validation_exception_when_parent_does_not_exist()
    {
        var request = ValidRequest(parentId: "missing-parent");

        var ex = await Assert.ThrowsAsync<AppValidationException>(() => _sut.CreateAsync(request));

        Assert.True(ex.Errors.ContainsKey(nameof(FolderRequest.ParentId)));
    }

    [Fact]
    public async Task UpdateAsync_throws_validation_exception_when_folder_would_become_its_own_ancestor()
    {
        var parent = await _sut.CreateAsync(ValidRequest("Parent"));
        var child = await _sut.CreateAsync(ValidRequest("Child", parent.Id));

        var request = ValidRequest("Parent", child.Id);

        var ex = await Assert.ThrowsAsync<AppValidationException>(() => _sut.UpdateAsync(parent.Id, request));

        Assert.True(ex.Errors.ContainsKey(nameof(FolderRequest.ParentId)));
    }

    [Fact]
    public async Task DeleteAsync_throws_conflict_when_folder_has_bookmarks_and_not_cascading()
    {
        var folder = await _sut.CreateAsync(ValidRequest());
        await _bookmarks.CreateAsync(new BookmarkRequest
        {
            Title = "Bookmark",
            Url = "https://example.com",
            FolderId = folder.Id,
        });

        await Assert.ThrowsAsync<ConflictException>(() => _sut.DeleteAsync(folder.Id, cascade: false));
    }

    [Fact]
    public async Task DeleteAsync_cascades_by_detaching_bookmarks_and_reparenting_subfolders()
    {
        var grandparent = await _sut.CreateAsync(ValidRequest("Grandparent"));
        var parent = await _sut.CreateAsync(ValidRequest("Parent", grandparent.Id));
        var child = await _sut.CreateAsync(ValidRequest("Child", parent.Id));
        var bookmark = await _bookmarks.CreateAsync(new BookmarkRequest
        {
            Title = "Bookmark",
            Url = "https://example.com",
            FolderId = parent.Id,
        });

        await _sut.DeleteAsync(parent.Id, cascade: true);

        var reparentedChild = await _sut.GetByIdAsync(child.Id);
        Assert.Equal(grandparent.Id, reparentedChild.ParentId);

        var detachedBookmark = await _bookmarks.GetByIdAsync(bookmark.Id);
        Assert.Null(detachedBookmark.FolderId);

        await Assert.ThrowsAsync<NotFoundException>(() => _sut.GetByIdAsync(parent.Id));
    }
}
