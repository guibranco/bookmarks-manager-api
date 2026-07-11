using BookmarksManager.Api.Dtos;
using BookmarksManager.Api.Services;

namespace BookmarksManager.Api.Endpoints;

public static class FolderEndpoints
{
    public static IEndpointRouteBuilder MapFolderEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/folders").WithTags("Folders");

        group.MapGet("/", async (FolderService service, CancellationToken ct) =>
            Results.Ok(await service.GetAllAsync(ct)));

        group.MapGet("/{id}", async (string id, FolderService service, CancellationToken ct) =>
            Results.Ok(await service.GetByIdAsync(id, ct)));

        group.MapPost("/", async (FolderRequest request, FolderService service, CancellationToken ct) =>
        {
            var created = await service.CreateAsync(request, ct);
            return Results.Created($"/api/folders/{created.Id}", created);
        }).RequireAuthorization();

        group.MapPut("/{id}", async (string id, FolderRequest request, FolderService service, CancellationToken ct) =>
            Results.Ok(await service.UpdateAsync(id, request, ct))).RequireAuthorization();

        group.MapDelete("/{id}", async (string id, FolderService service, CancellationToken ct, bool cascade = false) =>
        {
            await service.DeleteAsync(id, cascade, ct);
            return Results.NoContent();
        }).RequireAuthorization();

        return app;
    }
}
