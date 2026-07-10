using System.ComponentModel.DataAnnotations;

namespace BookmarksManager.Api.Common;

public static class Validation
{
    public static void Validate(object dto)
    {
        var context = new ValidationContext(dto);
        var results = new List<ValidationResult>();
        if (Validator.TryValidateObject(dto, context, results, validateAllProperties: true))
        {
            return;
        }

        var errors = results
            .SelectMany(r => r.MemberNames.DefaultIfEmpty(string.Empty).Select(m => (Member: m, r.ErrorMessage)))
            .GroupBy(x => x.Member)
            .ToDictionary(
                g => g.Key,
                g => g.Select(x => x.ErrorMessage ?? "Invalid value.").ToArray());

        throw new AppValidationException(errors);
    }

    /// <summary>Trims tag names, drops empties, and removes case-sensitive duplicates while preserving order.</summary>
    public static string[] NormalizeTags(IEnumerable<string> tags)
    {
        return tags
            .Select(t => t.Trim())
            .Where(t => t.Length > 0)
            .Distinct()
            .ToArray();
    }
}
