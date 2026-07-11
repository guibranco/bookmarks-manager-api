---
title: Docker & Compose
parent: Deployment
nav_order: 1
---

# Docker & Compose

## Image

The [`Dockerfile`](https://github.com/guibranco/bookmarks-manager-api/blob/main/Dockerfile) is a multi-stage build:

1. **build** — `mcr.microsoft.com/dotnet/sdk:10.0` restores and publishes `src/BookmarksManager.Api`, stamping the assembly with the version supplied by CI (see [CI/CD]({{ site.baseurl }}/deployment/ci-cd/)).
2. **runtime** — `mcr.microsoft.com/dotnet/aspnet:10.0`, running as a non-root `bookmarks` user, listening on port `8080`.

Published images live at `ghcr.io/guibranco/bookmarks-manager-api`, tagged with the full SemVer (e.g. `1.2.3`), the major.minor.patch (`1.2`), and `latest`.

## Running it standalone

```bash
docker run -p 8080:8080 \
  -e ApiKey="a-long-random-secret" \
  -e Database__Provider=MariaDb \
  -e Database__MariaDbServerVersion=10.11.6-mariadb \
  -e ConnectionStrings__Default="Server=db;Database=bookmarks_manager;User=bookmarks;Password=change-me;" \
  -e Cors__AllowedOrigins__0="https://bookmarks.straccini.com" \
  ghcr.io/guibranco/bookmarks-manager-api:latest
```

## Production stack

The repository's [`docker-compose.yml`](https://github.com/guibranco/bookmarks-manager-api/blob/main/docker-compose.yml) runs two services on the production host:

- **`api`** — the container above, only exposed on the internal `bookmarksnet` network, with a `/health`-based healthcheck.
- **`nginx`** — terminates TLS and reverse-proxies `api.bookmarks.straccini.com` to the `api` service. Its config lives at [`deploy/nginx/bookmarks-manager-api.conf`](https://github.com/guibranco/bookmarks-manager-api/blob/main/deploy/nginx/bookmarks-manager-api.conf) and expects certificates managed by `certbot`/Let's Encrypt under `/etc/letsencrypt`.

```bash
# on the production host
cp .env.example .env   # fill in BOOKMARKS_MANAGER_API_KEY, connection string, frontend origin
docker compose pull
docker compose up -d
```

Required environment variables (see `docker-compose.yml`):

| Variable | Purpose |
| :------- | :------ |
| `BOOKMARKS_MANAGER_API_KEY` | Value for the `X-Api-Key` header — keep this secret |
| `BOOKMARKS_MANAGER_CONNECTION_STRING` | MariaDB connection string |
| `MARIADB_SERVER_VERSION` | MariaDB version string for Pomelo (defaults to `10.11.6-mariadb`) |
| `FRONTEND_ORIGIN` | Browser origin allowed by CORS (the Bookmarks Manager UI's URL) |

## Database migrations

Migrations are provider-specific (`BookmarksManager.Migrations.Sqlite` / `BookmarksManager.Migrations.MariaDb`) and are applied out-of-band, before rolling out a new image:

```bash
EF_PROVIDER=MariaDb dotnet ef database update \
  --project src/BookmarksManager.Migrations.MariaDb \
  --startup-project src/BookmarksManager.Api \
  --connection "Server=db;Database=bookmarks_manager;User=bookmarks;Password=change-me;"
```

Applying migrations is intentionally decoupled from container startup, so a rollout never races a schema change against multiple starting replicas.
