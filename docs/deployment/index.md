---
title: Deployment
nav_order: 5
has_children: true
---

# Deployment

The production API is served from **[https://api.bookmarks.straccini.com](https://api.bookmarks.straccini.com)**.

1. [Docker & Compose]({{ site.baseurl }}/deployment/docker/) — the container image, `docker-compose.yml`, and the nginx reverse proxy in front of it
2. [CI/CD]({{ site.baseurl }}/deployment/ci-cd/) — how a push to `main` turns into a versioned image and a GitHub release

## At a glance

```mermaid
sequenceDiagram
    participant Dev as Developer
    participant GH as GitHub Actions
    participant GV as GitVersion
    participant GHCR as GHCR
    participant Rel as GitHub Release
    participant Host as Production host

    Dev->>GH: push to main
    GH->>GV: compute SemVer from history
    GH->>GH: dotnet build + dotnet test
    GH->>GHCR: docker build & push<br/>(fullSemVer, majorMinorPatch, latest)
    GH->>Rel: create tag vX.Y.Z + release notes
    Host->>GHCR: docker compose pull && up -d
    Host-->>Dev: api.bookmarks.straccini.com serving vX.Y.Z
```

The last step (pulling the new image on the production host) is a deliberate manual/operator action, not an automated push-to-deploy from CI — see [CI/CD]({{ site.baseurl }}/deployment/ci-cd/) for why.
