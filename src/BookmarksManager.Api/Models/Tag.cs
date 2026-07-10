namespace BookmarksManager.Api.Models;

public class Tag
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;

    public ICollection<BookmarkTag> BookmarkTags { get; set; } = new List<BookmarkTag>();
}
