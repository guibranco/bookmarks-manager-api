namespace BookmarksManager.Api.Common;

public class NotFoundException(string message) : Exception(message);

public class ConflictException(string message) : Exception(message);

public class AppValidationException(IDictionary<string, string[]> errors)
    : Exception("One or more validation errors occurred.")
{
    public IDictionary<string, string[]> Errors { get; } = errors;
}
