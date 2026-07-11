# 🗄️🔖 Bookmarks Manager (API)

A **RESTful API** for managing bookmarks, built with **.NET 10** and **ASP.NET Core Minimal APIs**, backed by **Entity Framework Core** (SQLite or MariaDB). Designed to integrate seamlessly with the **[Bookmarks Manager UI](https://github.com/guibranco/bookmarks-manager-ui)** to store, organize, and retrieve bookmarks.

📖 Full documentation: **[guibranco.github.io/bookmarks-manager-api](https://guibranco.github.io/bookmarks-manager-api/)**
🌐 Production API: **[api.bookmarks.straccini.com](https://api.bookmarks.straccini.com)**

## 🚀 Features

- 📂 **Bookmark Management** – Create, update, delete, and retrieve bookmarks.
- 🏷 **Folder and Tag Organization** – Nested folders and on-demand tags.
- 🌍 **Public reads, protected writes** – Every `GET` endpoint is anonymous; `POST`/`PUT`/`DELETE` require an API key.
- 🔍 **Health check** – `/health` reports API and database connectivity.
- 🧪 **Unit and Integration Tests** – xUnit + `WebApplicationFactory`, ensuring reliability.
- 🐳 **Docker Support** – Multi-stage Dockerfile and Docker Compose stack.
- ⚙️ **GitHub Actions CI/CD** – Build, test, SonarCloud analysis, GitVersion-based releases, and GitHub Pages docs.

## 🛠 Tech Stack

- **Frontend**: [Bookmarks Manager UI](https://github.com/guibranco/bookmarks-manager-ui/)
- **Backend**: .NET 10, ASP.NET Core Minimal APIs
- **Database**: SQLite (dev) or MariaDB (production) via Entity Framework Core / Pomelo
- **Auth**: Shared `X-Api-Key` header for write operations
- **Logging**: Serilog (console + rolling file)
- **API docs**: Swashbuckle / Swagger UI, plus a [Just the Docs](https://just-the-docs.com/) site under [`docs/`](docs/)
- **Testing**: xUnit, `Microsoft.AspNetCore.Mvc.Testing`, EF Core SQLite
- **Containerization**: Docker / Docker Compose
- **CI/CD**: GitHub Actions — build & test with SonarCloud on every PR, GitVersion-based release & GHCR image on `main`, docs deploy to GitHub Pages

## 📦 Installation

```bash
git clone https://github.com/guibranco/bookmarks-manager-api.git
cd bookmarks-manager-api

dotnet restore BookmarksManager.sln
dotnet build BookmarksManager.sln
```

## 🔧 Usage

### Start the API locally

```bash
dotnet run --project src/BookmarksManager.Api
```

This uses the `Development` environment by default: a local SQLite database, Swagger UI at `/swagger`, and a development API key (`dev-secret-key-change-me`).

### Seed sample data

```bash
dotnet run --project src/BookmarksManager.Api -- seed
```

### Run the test suite

```bash
dotnet test BookmarksManager.sln
```

### Run with Docker

```bash
docker build -t bookmarks-manager-api .
docker run -p 8080:8080 -e ApiKey=dev-secret-key-change-me bookmarks-manager-api
```

See the [deployment docs](https://guibranco.github.io/bookmarks-manager-api/deployment/) for the full Docker Compose + nginx production setup.

## 🔐 Authentication model

| Method | Auth required? |
| :----- | :-------------- |
| `GET` (all endpoints, including `/health`) | No — public |
| `POST` / `PUT` / `DELETE` | Yes — `X-Api-Key` header |

See [Security](https://guibranco.github.io/bookmarks-manager-api/architecture/security/) for details.

## 🔄 Database migrations

EF Core migrations are provider-specific and applied out-of-band (not at container startup):

```bash
EF_PROVIDER=Sqlite dotnet ef database update \
  --project src/BookmarksManager.Migrations.Sqlite \
  --startup-project src/BookmarksManager.Api
```

## 📚 Documentation

Full documentation — getting started, architecture diagrams, API reference, and deployment guide — lives under [`docs/`](docs/) and is published at **[guibranco.github.io/bookmarks-manager-api](https://guibranco.github.io/bookmarks-manager-api/)**.

## 📜 License

This project is licensed under the [MIT License](LICENSE).

---

💡 **Contributions are welcome!** Feel free to submit issues or pull requests to improve the project. 🚀
