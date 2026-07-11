---
title: Security
parent: Architecture
nav_order: 3
---

# Security

## Reads are public, writes require a key

The API deliberately splits every resource into a public read surface and a protected write surface:

| Method | Route | Auth required? |
| :----- | :---- | :-------------- |
| `GET` | `/api/bookmarks`, `/api/bookmarks/{id}` | No |
| `POST` | `/api/bookmarks` | **Yes** |
| `PUT` | `/api/bookmarks/{id}` | **Yes** |
| `DELETE` | `/api/bookmarks/{id}` | **Yes** |
| `GET` | `/api/folders`, `/api/folders/{id}` | No |
| `POST` | `/api/folders` | **Yes** |
| `PUT` | `/api/folders/{id}` | **Yes** |
| `DELETE` | `/api/folders/{id}` | **Yes** |
| `GET` | `/api/tags` | No |
| `GET` | `/health` | No |

This is enforced per-route in the endpoint definitions, not globally: each mutating route chains `.RequireAuthorization()`, while `GET` routes and `/health` do not.

```csharp
group.MapPost("/", async (BookmarkRequest request, BookmarkService service, CancellationToken ct) =>
{
    var created = await service.CreateAsync(request, ct);
    return Results.Created($"/api/bookmarks/{created.Id}", created);
}).RequireAuthorization();
```

## How the API key is validated

`ApiKeyAuthenticationHandler` (scheme name `ApiKey`, header `X-Api-Key`) runs on every request, but only rejects the request if the endpoint actually requires authorization:

1. The configured key (`ApiKey` in configuration) must be non-empty, otherwise authentication always fails — write operations are unusable until an API key is configured.
2. The caller must send the `X-Api-Key` header.
3. Both the configured key and the supplied key are SHA-256 hashed, then compared with `CryptographicOperations.FixedTimeEquals` — a constant-time comparison that avoids leaking key length or content through response-timing side channels.

There is a single shared key (no per-client keys, no scopes) — anyone with the key can perform any write operation. Treat it as an administrative secret.

## Error responses

Errors are mapped centrally by `GlobalExceptionHandler` into [RFC 7807](https://www.rfc-editor.org/rfc/rfc7807) `ProblemDetails` responses:

| Exception | Status | Body |
| :-------- | :----- | :--- |
| `NotFoundException` | `404` | `ProblemDetails` |
| `ConflictException` | `409` | `ProblemDetails` (e.g. deleting a non-empty folder without `cascade=true`) |
| `AppValidationException` | `400` | `ValidationProblemDetails` with per-field errors |
| Anything else | `500` | Generic `ProblemDetails` (details logged server-side, not exposed to the caller) |

## CORS

Allowed browser origins are configured via `Cors:AllowedOrigins` (empty by default — no browser origin is allowed until explicitly configured). See [Configuration]({{ site.baseurl }}/getting-started/configuration/).
