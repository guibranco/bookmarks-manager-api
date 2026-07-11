---
title: Health
parent: API reference
nav_order: 4
---

# Health

## `GET /health`

Public. Reports whether the API can reach its database.

```json
{ "status": "healthy", "database": "connected" }
```

Returns `200 OK` when the database is reachable, or `503 Service Unavailable` with `"status": "unhealthy"` / `"database": "unreachable"` otherwise. Used by the Docker Compose healthcheck in [Deployment]({{ site.baseurl }}/deployment/).
