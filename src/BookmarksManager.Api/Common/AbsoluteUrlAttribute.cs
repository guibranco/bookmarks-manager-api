using System.ComponentModel.DataAnnotations;

namespace BookmarksManager.Api.Common;

public class AbsoluteUrlAttribute : ValidationAttribute
{
    public AbsoluteUrlAttribute()
    {
        ErrorMessage = "The {0} field must be a valid absolute URL.";
    }

    public override bool IsValid(object? value)
    {
        return value is string s
            && Uri.TryCreate(s, UriKind.Absolute, out var uri)
            && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
    }
}
