namespace BookmarksManager.Api.Models;

public class BookmarkTag
{
    public string BookmarkId { get; set; } = string.Empty;
    public int TagId { get; set; }

    public Bookmark Bookmark { get; set; } = null!;
    public Tag Tag { get; set; } = null!;
}
