# 🗄️🔖 Bookmarks Manager (API)

A **RESTful API** for managing bookmarks, built with **PHP 8.3** and **MySQL**, using the **GuiBranco\Panckage** library for structured and efficient development. Designed to integrate seamlessly with the **Bookmark Manager UI** to store, organize, and retrieve bookmarks.

## 🚀 Features
- 📂 **Bookmark Management** – Create, update, delete, and retrieve bookmarks.
- 🏷 **Folder and Tag Organization** – Categorize bookmarks efficiently.
- 🔍 **Search & Filtering** – Find bookmarks quickly.
- 🛡 **Secure & Scalable** – Built with modern PHP best practices.
- 🧪 **Unit and Integration Tests** – Ensuring reliability.
- 🐳 **Docker Support** – Easily run and test in an isolated environment.
- ⚙️ **GitHub Actions for Migrations** – Automating database schema updates.

## 🛠 Tech Stack
- **Frontend**: [Bookmarks Manager UI](https://github.com/guibranco/bookmarks-manager-ui/)
- **Backend**: PHP 8.3
- **Database**: MySQL
- **Libraries**: [GuiBranco\Pancake](https://github.com/guibranco/pancake)
- **Testing**: Unit and Integration Tests with PHPUnit
- **Containerization**: Docker
- **CI/CD**: GitHub Actions for build, test, database migrations, and deployments 

## 📦 Installation

Clone the repository and install dependencies:

```bash
# Clone the repo
git clone https://github.com/guibranco/bookmarks-manager-api.git
cd bookmarks-manager-api

# Install dependencies
composer install
```

## 🔧 Usage

### Start the API Locally

```bash
php -S localhost:8000 -t public
```

### Run the Test Suite

```bash
docker-compose up -d  # Start the test database
tests/run-tests.sh  # Execute tests
```

## 🔄 Database Migrations

Migrations are automatically applied via **GitHub Actions** during deployment. To run them manually:

```bash
php artisan migrate
```

## 📜 License
This project is licensed under the [MIT License](LICENSE).

---

💡 **Contributions are welcome!** Feel free to submit issues or pull requests to improve the project. 🚀

