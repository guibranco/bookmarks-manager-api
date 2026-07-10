using BookmarksManager.Api.Data;
using Microsoft.EntityFrameworkCore;

namespace BookmarksManager.Api.Endpoints;

public static class TagEndpoints
{
    public static IEndpointRouteBuilder MapTagEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/tags", async (AppDbContext db, CancellationToken ct) =>
        {
            var names = await db.Tags
                .OrderBy(t => t.Name)
                .Select(t => t.Name)
                .ToListAsync(ct);

            return Results.Ok(names);
        }).WithTags("Tags");

        return app;
    }
}
