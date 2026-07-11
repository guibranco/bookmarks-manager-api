---
title: Configuration
parent: Getting started
nav_order: 2
---

# Configuration

All configuration is standard ASP.NET Core configuration: `appsettings.json`, `appsettings.{Environment}.json`, environment variables (`Section__Key` syntax), or a mounted secrets file.

## Settings reference

| Key | Description | Example |
| :-- | :----------- | :------ |
| `ApiKey` | Shared secret required in the `X-Api-Key` header for write operations. Empty disables writes entirely. | `dev-secret-key-change-me` |
| `Database:Provider` | `Sqlite` or `MariaDb`. | `Sqlite` |
| `Database:MariaDbServerVersion` | MariaDB server version string, used by Pomelo when `Provider` is `MariaDb`. | `10.11.6-mariadb` |
| `ConnectionStrings:Default` | EF Core connection string for the selected provider. | `Data Source=bookmarks-manager.db` |
| `Cors:AllowedOrigins` | Array of origins allowed to call the API from a browser. | `["https://bookmarks.straccini.com"]` |

## Base `appsettings.json`

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "ApiKey": "",
  "Database": {
    "Provider": "Sqlite",
    "MariaDbServerVersion": "10.11.6-mariadb"
  },
  "ConnectionStrings": {
    "Default": "Data Source=bookmarks-manager.db"
  },
  "Cors": {
    "AllowedOrigins": []
  }
}
```

## Environment variables

Any setting can be overridden with an environment variable using double underscores as the section separator:

```bash
export ApiKey="a-long-random-secret"
export Database__Provider="MariaDb"
export Database__MariaDbServerVersion="10.11.6-mariadb"
export ConnectionStrings__Default="Server=db;Database=bookmarks_manager;User=bookmarks;Password=change-me;"
export Cors__AllowedOrigins__0="https://bookmarks.straccini.com"
```

This is the pattern used by `docker-compose.yml` in production — see [Deployment]({{ site.baseurl }}/deployment/).

{: .important }
> Always set a strong, random `ApiKey` outside of local development. The handler compares keys using a constant-time, hash-based comparison, but an empty or predictable key defeats the purpose of authentication entirely.

## The API key

Write operations (`POST`, `PUT`, `DELETE`) are protected by a single shared API key, checked by [`ApiKeyAuthenticationHandler`](https://github.com/guibranco/bookmarks-manager-api/blob/main/src/BookmarksManager.Api/Auth/ApiKeyAuthenticationHandler.cs). Send it as a header on every write request:

```http
X-Api-Key: your-secret-key
```

Read (`GET`) endpoints never require this header — see [API reference]({{ site.baseurl }}/api-reference/) for which routes are public vs. protected.
