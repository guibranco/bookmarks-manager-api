# syntax=docker/dockerfile:1.7

# ---- build ---------------------------------------------------------------
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Version info supplied by CI (GitVersion). Defaults keep local builds working.
ARG VERSION=0.0.0
ARG ASSEMBLY_VERSION=0.0.0.0
ARG FILE_VERSION=0.0.0.0
ARG INFORMATIONAL_VERSION=0.0.0-local

# Copy just the things required for restore first for better layer caching.
COPY BookmarksManager.sln ./
COPY src/BookmarksManager.Api/BookmarksManager.Api.csproj src/BookmarksManager.Api/
COPY src/BookmarksManager.Migrations.Sqlite/BookmarksManager.Migrations.Sqlite.csproj src/BookmarksManager.Migrations.Sqlite/
COPY src/BookmarksManager.Migrations.MariaDb/BookmarksManager.Migrations.MariaDb.csproj src/BookmarksManager.Migrations.MariaDb/
COPY tests/BookmarksManager.Api.Tests/BookmarksManager.Api.Tests.csproj tests/BookmarksManager.Api.Tests/
COPY seed/SeedImporter.cs seed/

RUN dotnet restore BookmarksManager.sln

COPY src/ ./src/
COPY tests/ ./tests/
COPY seed/ ./seed/

RUN dotnet publish src/BookmarksManager.Api/BookmarksManager.Api.csproj \
    -c Release \
    -o /app/api \
    --no-restore \
    -p:UseAppHost=false \
    -p:Version=${ASSEMBLY_VERSION} \
    -p:AssemblyVersion=${ASSEMBLY_VERSION} \
    -p:FileVersion=${FILE_VERSION} \
    -p:InformationalVersion=${INFORMATIONAL_VERSION}

# ---- runtime -------------------------------------------------------------
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime

# Re-declare ARGs needed for LABEL values (ARGs don't cross stage boundaries).
ARG VERSION=0.0.0
ARG INFORMATIONAL_VERSION=0.0.0-local
ARG GIT_SHA=unknown
ARG BUILD_DATE

# OCI image labels — discoverable via `docker inspect` and GHCR UI.
LABEL org.opencontainers.image.title="Bookmarks Manager API" \
      org.opencontainers.image.description="RESTful API for managing bookmarks, folders and tags" \
      org.opencontainers.image.version="${VERSION}" \
      org.opencontainers.image.revision="${GIT_SHA}" \
      org.opencontainers.image.created="${BUILD_DATE}" \
      org.opencontainers.image.source="https://github.com/guibranco/bookmarks-manager-api" \
      org.opencontainers.image.licenses="MIT"

ENV BOOKMARKS_MANAGER_VERSION=${INFORMATIONAL_VERSION}

# Create a non-root user and data/log dirs.
RUN groupadd --system --gid 10001 bookmarks \
    && useradd --system --uid 10001 --gid bookmarks --home-dir /home/bookmarks --create-home bookmarks \
    && mkdir -p /var/lib/bookmarks-manager /var/log/bookmarks-manager \
    && chown -R bookmarks:bookmarks /var/lib/bookmarks-manager /var/log/bookmarks-manager

WORKDIR /app
COPY --from=build /app/api ./

USER bookmarks

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "BookmarksManager.Api.dll"]
