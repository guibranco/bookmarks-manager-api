using System.ComponentModel.DataAnnotations;

namespace BookmarksManager.Api.Dtos;

public record FolderDto(string Id, string Name, string? ParentId, string? Icon);

public class FolderRequest
{
    [Required(AllowEmptyStrings = false)]
    public string Name { get; set; } = string.Empty;

    public string? ParentId { get; set; }

    public string? Icon { get; set; }
}
