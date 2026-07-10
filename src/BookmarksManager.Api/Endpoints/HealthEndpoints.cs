using BookmarksManager.Api.Data;

namespace BookmarksManager.Api.Endpoints;

public static class HealthEndpoints
{
    public static IEndpointRouteBuilder MapHealthEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/health", async (AppDbContext db, CancellationToken ct) =>
        {
            bool databaseReachable;
            try
            {
                databaseReachable = await db.Database.CanConnectAsync(ct);
            }
            catch
            {
                databaseReachable = false;
            }

            var status = new
            {
                status = databaseReachable ? "healthy" : "unhealthy",
                database = databaseReachable ? "connected" : "unreachable",
            };

            return databaseReachable ? Results.Ok(status) : Results.Json(status, statusCode: StatusCodes.Status503ServiceUnavailable);
        }).WithTags("Health");

        return app;
    }
}
