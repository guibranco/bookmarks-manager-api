---
title: Quick start
parent: Getting started
nav_order: 3
---

# Quick start

## Seed sample data

The repository ships a `seed/sample.json` file — a real bookmarks export with folders, bookmarks and tags — plus a `SeedImporter` that loads it idempotently (already-imported folders/bookmarks are skipped by id).

```bash
dotnet run --project src/BookmarksManager.Api -- seed
```

Pass a custom path as a second argument to import a different export:

```bash
dotnet run --project src/BookmarksManager.Api -- seed /path/to/export.json
```

## Read data (no authentication needed)

```bash
curl http://localhost:5000/api/folders
curl http://localhost:5000/api/bookmarks
curl http://localhost:5000/api/bookmarks/{id}
curl http://localhost:5000/api/tags
curl http://localhost:5000/health
```

## Create a folder (authentication required)

```bash
curl -X POST http://localhost:5000/api/folders \
  -H "Content-Type: application/json" \
  -H "X-Api-Key: dev-secret-key-change-me" \
  -d '{"name": "Reading list"}'
```

## Create a bookmark

```bash
curl -X POST http://localhost:5000/api/bookmarks \
  -H "Content-Type: application/json" \
  -H "X-Api-Key: dev-secret-key-change-me" \
  -d '{
        "title": "Just the Docs",
        "url": "https://just-the-docs.com",
        "tags": ["docs", "jekyll"]
      }'
```

Omitting or getting the `X-Api-Key` header wrong returns `401 Unauthorized`; a missing/invalid `folderId` returns `400 Bad Request` with a `ValidationProblemDetails` body.

## Explore interactively

In development, Swagger UI is available at `http://localhost:5000/swagger` with the `X-Api-Key` security scheme wired in — click **Authorize** and paste your key to try write operations from the browser.
