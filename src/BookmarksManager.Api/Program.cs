using BookmarksManager.Api.Auth;
using BookmarksManager.Api.Common;
using BookmarksManager.Api.Data;
using BookmarksManager.Api.Endpoints;
using BookmarksManager.Api.Services;
using Microsoft.OpenApi.Models;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, services, loggerConfiguration) => loggerConfiguration
    .ReadFrom.Configuration(context.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File(
        Path.Combine(context.HostingEnvironment.ContentRootPath, "logs", "bookmarks-manager-api-.log"),
        rollingInterval: Serilog.RollingInterval.Day,
        retainedFileCountLimit: 14));

var databaseProvider = builder.Configuration["Database:Provider"] ?? DatabaseProvider.Sqlite;
var connectionString = builder.Configuration.GetConnectionString("Default")
    ?? throw new InvalidOperationException("Missing 'ConnectionStrings:Default' configuration.");
var mariaDbServerVersion = builder.Configuration["Database:MariaDbServerVersion"];

builder.Services.AddDbContext<AppDbContext>(options =>
    DatabaseProvider.Configure(options, databaseProvider, connectionString, mariaDbServerVersion));

builder.Services.AddScoped<BookmarkService>();
builder.Services.AddScoped<FolderService>();

builder.Services
    .AddAuthentication(ApiKeyDefaults.SchemeName)
    .AddScheme<ApiKeyAuthenticationSchemeOptions, ApiKeyAuthenticationHandler>(ApiKeyDefaults.SchemeName, _ => { });
builder.Services.AddAuthorization();

var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
builder.Services.AddCors(options => options.AddDefaultPolicy(policy => policy
    .WithOrigins(allowedOrigins)
    .AllowAnyHeader()
    .AllowAnyMethod()));

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "Bookmarks Manager API", Version = "v1" });

    options.AddSecurityDefinition(ApiKeyDefaults.SchemeName, new OpenApiSecurityScheme
    {
        Name = ApiKeyDefaults.HeaderName,
        Type = SecuritySchemeType.ApiKey,
        In = ParameterLocation.Header,
        Description = "API key required for write operations. Example: \"X-Api-Key: {your key}\"",
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = ApiKeyDefaults.SchemeName },
            },
            Array.Empty<string>()
        },
    });
});

var app = builder.Build();

if (args.Length > 0 && args[0].Equals("seed", StringComparison.OrdinalIgnoreCase))
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var seedLogger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

    var seedPath = args.Length > 1
        ? args[1]
        : Path.Combine(app.Environment.ContentRootPath, "..", "..", "seed", "sample.json");

    await SeedImporter.RunAsync(db, Path.GetFullPath(seedPath), seedLogger);
    return;
}

app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors();
app.UseAuthentication();
app.UseAuthorization();

app.MapBookmarkEndpoints();
app.MapFolderEndpoints();
app.MapTagEndpoints();
app.MapHealthEndpoints();

app.Run();

public partial class Program;
