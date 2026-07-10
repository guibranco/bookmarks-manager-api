using BookmarksManager.Api.Common;
using BookmarksManager.Api.Data;
using BookmarksManager.Api.Dtos;
using BookmarksManager.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace BookmarksManager.Api.Services;

public class FolderService(AppDbContext db)
{
    public async Task<List<FolderDto>> GetAllAsync(CancellationToken ct = default)
    {
        var folders = await db.Folders.AsNoTracking().ToListAsync(ct);
        return folders.Select(ToDto).ToList();
    }

    public async Task<FolderDto> GetByIdAsync(string id, CancellationToken ct = default)
    {
        var folder = await FindTrackedAsync(id, ct);
        return ToDto(folder);
    }

    public async Task<FolderDto> CreateAsync(FolderRequest request, CancellationToken ct = default)
    {
        Validation.Validate(request);
        await EnsureParentExistsAsync(request.ParentId, ct);

        var folder = new Folder
        {
            Id = Guid.NewGuid().ToString("n"),
            Name = request.Name.Trim(),
            ParentId = request.ParentId,
            Icon = string.IsNullOrWhiteSpace(request.Icon) ? null : request.Icon.Trim(),
        };

        db.Folders.Add(folder);
        await db.SaveChangesAsync(ct);

        return ToDto(folder);
    }

    public async Task<FolderDto> UpdateAsync(string id, FolderRequest request, CancellationToken ct = default)
    {
        Validation.Validate(request);
        await EnsureParentExistsAsync(request.ParentId, ct);

        var folder = await FindTrackedAsync(id, ct);

        if (request.ParentId is not null)
        {
            await EnsureNoCycleAsync(id, request.ParentId, ct);
        }

        folder.Name = request.Name.Trim();
        folder.ParentId = request.ParentId;
        folder.Icon = string.IsNullOrWhiteSpace(request.Icon) ? null : request.Icon.Trim();

        await db.SaveChangesAsync(ct);

        return ToDto(folder);
    }

    public async Task DeleteAsync(string id, bool cascade, CancellationToken ct = default)
    {
        var folder = await FindTrackedAsync(id, ct);

        var hasBookmarks = await db.Bookmarks.AsNoTracking().AnyAsync(b => b.FolderId == id, ct);
        var subfolders = await db.Folders.Where(f => f.ParentId == id).ToListAsync(ct);

        if ((hasBookmarks || subfolders.Count > 0) && !cascade)
        {
            throw new ConflictException(
                $"Folder '{id}' has bookmarks or subfolders. Pass cascade=true to detach them before deleting.");
        }

        if (cascade)
        {
            if (hasBookmarks)
            {
                await db.Bookmarks
                    .Where(b => b.FolderId == id)
                    .ExecuteUpdateAsync(setters => setters.SetProperty(b => b.FolderId, (string?)null), ct);
            }

            foreach (var subfolder in subfolders)
            {
                subfolder.ParentId = folder.ParentId;
            }
        }

        db.Folders.Remove(folder);
        await db.SaveChangesAsync(ct);
    }

    private async Task<Folder> FindTrackedAsync(string id, CancellationToken ct)
    {
        var folder = await db.Folders.FirstOrDefaultAsync(f => f.Id == id, ct);
        return folder ?? throw new NotFoundException($"Folder '{id}' was not found.");
    }

    private async Task EnsureParentExistsAsync(string? parentId, CancellationToken ct)
    {
        if (parentId is null)
        {
            return;
        }

        var exists = await db.Folders.AsNoTracking().AnyAsync(f => f.Id == parentId, ct);
        if (!exists)
        {
            throw new AppValidationException(new Dictionary<string, string[]>
            {
                [nameof(FolderRequest.ParentId)] = [$"Folder '{parentId}' was not found."],
            });
        }
    }

    private async Task EnsureNoCycleAsync(string folderId, string parentId, CancellationToken ct)
    {
        var currentId = parentId;
        while (currentId is not null)
        {
            if (currentId == folderId)
            {
                throw new AppValidationException(new Dictionary<string, string[]>
                {
                    [nameof(FolderRequest.ParentId)] = ["A folder cannot be its own ancestor."],
                });
            }

            currentId = await db.Folders.AsNoTracking()
                .Where(f => f.Id == currentId)
                .Select(f => f.ParentId)
                .FirstOrDefaultAsync(ct);
        }
    }

    private static FolderDto ToDto(Folder f) => new(f.Id, f.Name, f.ParentId, f.Icon);
}
