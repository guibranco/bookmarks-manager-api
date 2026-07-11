using BookmarksManager.Api.Dtos;
using BookmarksManager.Api.Services;

namespace BookmarksManager.Api.Endpoints;

public static class BookmarkEndpoints
{
    public static IEndpointRouteBuilder MapBookmarkEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/bookmarks").WithTags("Bookmarks");

        group.MapGet("/", async (BookmarkService service, CancellationToken ct) =>
            Results.Ok(await service.GetAllAsync(ct)));

        group.MapGet("/{id}", async (string id, BookmarkService service, CancellationToken ct) =>
            Results.Ok(await service.GetByIdAsync(id, ct)));

        group.MapPost("/", async (BookmarkRequest request, BookmarkService service, CancellationToken ct) =>
        {
            var created = await service.CreateAsync(request, ct);
            return Results.Created($"/api/bookmarks/{created.Id}", created);
        }).RequireAuthorization();

        group.MapPut("/{id}", async (string id, BookmarkRequest request, BookmarkService service, CancellationToken ct) =>
            Results.Ok(await service.UpdateAsync(id, request, ct))).RequireAuthorization();

        group.MapDelete("/{id}", async (string id, BookmarkService service, CancellationToken ct) =>
        {
            await service.DeleteAsync(id, ct);
            return Results.NoContent();
        }).RequireAuthorization();

        return app;
    }
}
