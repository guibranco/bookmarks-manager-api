namespace BookmarksManager.Api.Models;

public class Bookmark
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Thumbnail { get; set; } = string.Empty;
    public string? FolderId { get; set; }
    public bool Favorite { get; set; }
    public DateTime DateAdded { get; set; }

    public Folder? Folder { get; set; }
    public ICollection<BookmarkTag> BookmarkTags { get; set; } = new List<BookmarkTag>();
}
