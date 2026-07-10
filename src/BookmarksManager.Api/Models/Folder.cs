namespace BookmarksManager.Api.Models;

public class Folder
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? ParentId { get; set; }
    public string? Icon { get; set; }

    public Folder? Parent { get; set; }
    public ICollection<Folder> Children { get; set; } = new List<Folder>();
    public ICollection<Bookmark> Bookmarks { get; set; } = new List<Bookmark>();
}
