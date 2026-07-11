---
title: Installation
parent: Getting started
nav_order: 1
---

# Installation

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- SQLite (bundled, no install needed) for local development, or a MariaDB/MySQL server for production-like testing
- Optionally, [Docker](https://www.docker.com/) if you'd rather run the API in a container

## Clone the repository

```bash
git clone https://github.com/guibranco/bookmarks-manager-api.git
cd bookmarks-manager-api
```

## Restore & build

```bash
dotnet restore BookmarksManager.sln
dotnet build BookmarksManager.sln
```

## Run the API

```bash
dotnet run --project src/BookmarksManager.Api
```

By default the app uses the `Development` environment when run this way, which:

- Points at a local SQLite database (`bookmarks-manager.dev.db`)
- Enables Swagger UI at `/swagger`
- Sets a development API key (`dev-secret-key-change-me`) — see [Configuration]({{ site.baseurl }}/getting-started/configuration/)

## Run with Docker

```bash
docker build -t bookmarks-manager-api .
docker run -p 8080:8080 \
  -e ApiKey=dev-secret-key-change-me \
  -e ConnectionStrings__Default="Data Source=/data/bookmarks-manager.db" \
  bookmarks-manager-api
```

See [Deployment]({{ site.baseurl }}/deployment/) for the full Docker Compose + nginx setup used in production.

## Run the tests

```bash
dotnet test BookmarksManager.sln
```

This runs both the unit tests (services, validation) and the integration tests (`WebApplicationFactory`-based, hitting real Minimal API endpoints against an in-memory SQLite database).
