using System.ComponentModel.DataAnnotations;
using BookmarksManager.Api.Common;

namespace BookmarksManager.Api.Dtos;

public record BookmarkDto(
    string Id,
    string Title,
    string Url,
    string Description,
    string Thumbnail,
    string[] Tags,
    string? FolderId,
    bool Favorite,
    DateTime DateAdded);

public class BookmarkRequest
{
    [Required(AllowEmptyStrings = false)]
    public string Title { get; set; } = string.Empty;

    [Required(AllowEmptyStrings = false)]
    [AbsoluteUrl]
    public string Url { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string Thumbnail { get; set; } = string.Empty;

    public string[] Tags { get; set; } = [];

    public string? FolderId { get; set; }

    public bool Favorite { get; set; }
}
