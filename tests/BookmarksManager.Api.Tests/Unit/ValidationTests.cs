using BookmarksManager.Api.Common;
using BookmarksManager.Api.Dtos;

namespace BookmarksManager.Api.Tests.Unit;

public class ValidationTests
{
    [Fact]
    public void Validate_does_not_throw_for_a_valid_dto()
    {
        var request = new BookmarkRequest { Title = "Title", Url = "https://example.com" };

        var exception = Record.Exception(() => Validation.Validate(request));

        Assert.Null(exception);
    }

    [Fact]
    public void Validate_throws_app_validation_exception_with_field_errors_for_an_invalid_dto()
    {
        var request = new BookmarkRequest { Title = "", Url = "not-a-url" };

        var ex = Assert.Throws<AppValidationException>(() => Validation.Validate(request));

        Assert.True(ex.Errors.ContainsKey(nameof(BookmarkRequest.Title)));
        Assert.True(ex.Errors.ContainsKey(nameof(BookmarkRequest.Url)));
    }

    [Fact]
    public void NormalizeTags_trims_drops_empties_and_deduplicates_while_preserving_order()
    {
        var result = Validation.NormalizeTags([" b ", "a", "a", "  ", "b", ""]);

        Assert.Equal(["b", "a"], result);
    }
}
