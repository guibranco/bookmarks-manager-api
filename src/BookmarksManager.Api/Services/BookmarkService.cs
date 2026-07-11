using BookmarksManager.Api.Common;
using BookmarksManager.Api.Data;
using BookmarksManager.Api.Dtos;
using BookmarksManager.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace BookmarksManager.Api.Services;

public class BookmarkService(AppDbContext db)
{
    public async Task<List<BookmarkDto>> GetAllAsync(CancellationToken ct = default)
    {
        var bookmarks = await db.Bookmarks
            .AsNoTracking()
            .Include(b => b.BookmarkTags).ThenInclude(bt => bt.Tag)
            .ToListAsync(ct);

        return bookmarks.Select(ToDto).ToList();
    }

    public async Task<BookmarkDto> GetByIdAsync(string id, CancellationToken ct = default)
    {
        var bookmark = await FindTrackedAsync(id, ct);
        return ToDto(bookmark);
    }

    public async Task<BookmarkDto> CreateAsync(BookmarkRequest request, CancellationToken ct = default)
    {
        Validation.Validate(request);
        await EnsureFolderExistsAsync(request.FolderId, ct);

        var bookmark = new Bookmark
        {
            Id = Guid.NewGuid().ToString("n"),
            Title = request.Title.Trim(),
            Url = request.Url.Trim(),
            Description = request.Description.Trim(),
            Thumbnail = request.Thumbnail.Trim(),
            FolderId = request.FolderId,
            Favorite = request.Favorite,
            DateAdded = DateTime.UtcNow,
        };

        await SetTagsAsync(bookmark, request.Tags, ct);

        db.Bookmarks.Add(bookmark);
        await db.SaveChangesAsync(ct);

        return ToDto(bookmark);
    }

    public async Task<BookmarkDto> UpdateAsync(string id, BookmarkRequest request, CancellationToken ct = default)
    {
        Validation.Validate(request);
        await EnsureFolderExistsAsync(request.FolderId, ct);

        var bookmark = await FindTrackedAsync(id, ct);

        bookmark.Title = request.Title.Trim();
        bookmark.Url = request.Url.Trim();
        bookmark.Description = request.Description.Trim();
        bookmark.Thumbnail = request.Thumbnail.Trim();
        bookmark.FolderId = request.FolderId;
        bookmark.Favorite = request.Favorite;

        await SetTagsAsync(bookmark, request.Tags, ct);

        await db.SaveChangesAsync(ct);

        return ToDto(bookmark);
    }

    public async Task DeleteAsync(string id, CancellationToken ct = default)
    {
        var bookmark = await FindTrackedAsync(id, ct);
        db.Bookmarks.Remove(bookmark);
        await db.SaveChangesAsync(ct);
    }

    private async Task<Bookmark> FindTrackedAsync(string id, CancellationToken ct)
    {
        var bookmark = await db.Bookmarks
            .Include(b => b.BookmarkTags).ThenInclude(bt => bt.Tag)
            .FirstOrDefaultAsync(b => b.Id == id, ct);

        return bookmark ?? throw new NotFoundException($"Bookmark '{id}' was not found.");
    }

    private async Task EnsureFolderExistsAsync(string? folderId, CancellationToken ct)
    {
        if (folderId is null)
        {
            return;
        }

        var exists = await db.Folders.AsNoTracking().AnyAsync(f => f.Id == folderId, ct);
        if (!exists)
        {
            throw new AppValidationException(new Dictionary<string, string[]>
            {
                [nameof(BookmarkRequest.FolderId)] = [$"Folder '{folderId}' was not found."],
            });
        }
    }

    private async Task SetTagsAsync(Bookmark bookmark, string[] rawTags, CancellationToken ct)
    {
        var names = Validation.NormalizeTags(rawTags);

        // List<T>.Contains (rather than an array) so EF Core translates this to a SQL IN clause
        // instead of trying to resolve System.MemoryExtensions' span-based Contains overload.
        var namesList = names.ToList();
        var existingTags = await db.Tags.Where(t => namesList.Contains(t.Name)).ToListAsync(ct);
        var missingNames = names.Except(existingTags.Select(t => t.Name)).ToArray();
        var newTags = missingNames.Select(name => new Tag { Name = name }).ToList();
        if (newTags.Count > 0)
        {
            db.Tags.AddRange(newTags);
        }

        var allTags = existingTags.Concat(newTags).ToList();

        bookmark.BookmarkTags.Clear();
        foreach (var tag in allTags)
        {
            bookmark.BookmarkTags.Add(new BookmarkTag { BookmarkId = bookmark.Id, Tag = tag });
        }
    }

    private static BookmarkDto ToDto(Bookmark b) => new(
        b.Id,
        b.Title,
        b.Url,
        b.Description,
        b.Thumbnail,
        b.BookmarkTags.Select(bt => bt.Tag.Name).OrderBy(n => n, StringComparer.Ordinal).ToArray(),
        b.FolderId,
        b.Favorite,
        b.DateAdded);
}
