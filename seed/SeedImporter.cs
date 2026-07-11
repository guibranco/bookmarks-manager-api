using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using BookmarksManager.Api.Data;
using BookmarksManager.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace BookmarksManager.Api.Data;

/// <summary>
/// Imports the frontend's sample.json export into the database. Safe to re-run: folders,
/// bookmarks and tags are matched by their existing Id/Name and skipped if already present.
/// </summary>
public static class SeedImporter
{
    private record SeedFolder(string Id, string Name, string? ParentId, string? Icon);

    private record SeedBookmark(
        string Id,
        string Title,
        string Url,
        string? Description,
        string? Thumbnail,
        string[]? Tags,
        string? FolderId,
        bool Favorite,
        string DateAdded);

    private record SeedFile(SeedFolder[] Folders, SeedBookmark[] Bookmarks);

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNameCaseInsensitive = true,
    };

    public static async Task RunAsync(AppDbContext db, string jsonPath, ILogger logger, CancellationToken ct = default)
    {
        if (!File.Exists(jsonPath))
        {
            throw new FileNotFoundException($"Seed file not found at '{jsonPath}'.", jsonPath);
        }

        await using var stream = File.OpenRead(jsonPath);
        var seed = await JsonSerializer.DeserializeAsync<SeedFile>(stream, JsonOptions, ct)
            ?? throw new InvalidDataException("Seed file is empty or malformed.");

        var existingFolderIds = await db.Folders.Select(f => f.Id).ToHashSetAsync(ct);
        var newFolders = seed.Folders
            .Where(f => !existingFolderIds.Contains(f.Id))
            .Select(f => new Folder { Id = f.Id, Name = f.Name, ParentId = f.ParentId, Icon = f.Icon })
            .ToList();

        if (newFolders.Count > 0)
        {
            db.Folders.AddRange(newFolders);
            await db.SaveChangesAsync(ct);
        }

        logger.LogInformation("Seed: imported {Count} folders ({Skipped} already present).",
            newFolders.Count, seed.Folders.Length - newFolders.Count);

        var existingBookmarkIds = await db.Bookmarks.Select(b => b.Id).ToHashSetAsync(ct);
        var existingTags = await db.Tags.ToDictionaryAsync(t => t.Name, ct);

        var importedBookmarks = 0;
        foreach (var sb in seed.Bookmarks)
        {
            if (existingBookmarkIds.Contains(sb.Id))
            {
                continue;
            }

            var bookmark = new Bookmark
            {
                Id = sb.Id,
                Title = sb.Title,
                Url = sb.Url,
                Description = sb.Description ?? string.Empty,
                Thumbnail = sb.Thumbnail ?? string.Empty,
                FolderId = sb.FolderId,
                Favorite = sb.Favorite,
                DateAdded = DateTime.Parse(sb.DateAdded, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal),
            };

            foreach (var tagName in (sb.Tags ?? []).Select(t => t.Trim()).Where(t => t.Length > 0).Distinct())
            {
                if (!existingTags.TryGetValue(tagName, out var tag))
                {
                    tag = new Tag { Name = tagName };
                    existingTags[tagName] = tag;
                    db.Tags.Add(tag);
                }

                bookmark.BookmarkTags.Add(new BookmarkTag { BookmarkId = bookmark.Id, Tag = tag });
            }

            db.Bookmarks.Add(bookmark);
            importedBookmarks++;
        }

        await db.SaveChangesAsync(ct);

        logger.LogInformation("Seed: imported {Count} bookmarks ({Skipped} already present).",
            importedBookmarks, seed.Bookmarks.Length - importedBookmarks);
    }
}
