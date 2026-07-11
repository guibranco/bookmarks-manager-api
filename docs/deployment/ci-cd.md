---
title: CI/CD
parent: Deployment
nav_order: 2
---

# CI/CD

Three GitHub Actions workflows cover the full lifecycle, from every pull request to a tagged production image.

## `build.yml` — build, test & SonarCloud

Runs on every push to `main` and every pull request targeting `main`.

- Restores, builds (`Release`), and runs the full test suite (unit + integration) with code coverage (`XPlat Code Coverage`, OpenCover format).
- Wraps the build in a SonarCloud analysis (`guibranco_bookmarks-manager-api` project, `guibranco` org), so every pull request gets a quality gate and coverage report before merge.
- Uploads TRX test results as a build artifact.

Requires a `SONAR_TOKEN` repository secret (from [sonarcloud.io](https://sonarcloud.io)).

## `release.yml` — release & publish

Runs only on push to `main` (i.e. after a pull request merges).

1. **build-and-test** — computes the next SemVer with [GitVersion](https://gitversion.net/) (`GitHubFlow`/`ContinuousDeployment` mode, configured in [`GitVersion.yml`](https://github.com/guibranco/bookmarks-manager-api/blob/main/GitVersion.yml)), then builds and tests the solution stamped with that version.
2. **docker** — builds the image from the [`Dockerfile`](https://github.com/guibranco/bookmarks-manager-api/blob/main/Dockerfile) and pushes it to `ghcr.io/guibranco/bookmarks-manager-api`, tagged with the full SemVer, `majorMinorPatch`, and `latest`.
3. **release** — creates a Git tag (`vX.Y.Z`) and a GitHub release with auto-generated release notes, linking back to the published image.

This is why the API isn't automatically deployed to the production host from CI: GitHub Actions doesn't hold credentials for that host. Instead, CI's job ends at "a versioned image exists on GHCR and a release exists in GitHub" — pulling that image onto `api.bookmarks.straccini.com` is a deliberate, separate operator action (`docker compose pull && up -d`), described in [Docker & Compose]({{ site.baseurl }}/deployment/docker/).

## `pages.yml` — docs

Runs on push to `main` when anything under `docs/` (or the workflow itself) changes, plus manual dispatch. Builds this Jekyll + Just the Docs site and publishes it to GitHub Pages.

## Versioning scheme

`GitVersion.yml` uses `GitHubFlow` with `ContinuousDeployment` mode: every commit on `main` bumps the version, and pre-release/branch metadata are folded into a single, ever-increasing SemVer. Tags are prefixed `v` (`v1.4.2`), matching the tags created by `release.yml`.
