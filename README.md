# NewsArchive

A news publishing and archive web application built with ASP.NET Core MVC (.NET 10) and SQL Server, using a 3-tier architecture (Presentation → Business Logic → Data Access).

See [ARCHITECTURE.md](ARCHITECTURE.md) for the design, endpoint list and roadmap.

## Features

**Readers**
- Latest news with keyword and date-range search and pagination
- Breaking news, trending news (by views and comments) and category pages
- Article page with author, category, comments and recommended articles

**Members**
- Sign up, log in, log out
- Comment on articles and delete their own comments
- Edit profile and change password
- Password reset with a one-time code

**Admins**
- Create, edit and delete articles and categories
- Manage users and roles
- Dashboard with total articles and users, most-viewed articles and articles per category
- Delete any comment

## Tech stack

| Layer | Technology |
|---|---|
| Framework | ASP.NET Core MVC, .NET 10 |
| Data | Entity Framework Core 10, SQL Server |
| Mapping | AutoMapper |
| UI | Razor views, Bootstrap 5, Bootstrap Icons |

## Project structure

```
NewsArchiveSolution/
├── DAL/          EF Core DbContext, entities, repositories
├── BLL/          Services, DTOs, validation, auth filters
└── NewsArchive/  MVC controllers, views, wwwroot, Program.cs
NewsArchiveDB.bacpac   Database export with sample data
```

## Getting started

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- SQL Server (Express or Developer edition) and SQL Server Management Studio or Azure Data Studio

### 1. Restore the database

Import `NewsArchiveDB.bacpac` into your SQL Server instance:

- SSMS: right-click **Databases** → **Import Data-tier Application** → select the bacpac → name it `NewsArchiveDB`.
- Or with SqlPackage:
  ```bash
  sqlpackage /Action:Import /SourceFile:NewsArchiveDB.bacpac /TargetServerName:"localhost\SQLEXPRESS" /TargetDatabaseName:NewsArchiveDB /TargetTrustServerCertificate:True
  ```

### 2. Configure the connection string

Edit `NewsArchiveSolution/NewsArchive/appsettings.json` and set `ConnectionStrings:DbConn` to your server, for example:

```json
"ConnectionStrings": {
  "DbConn": "Data Source=localhost\\SQLEXPRESS;Initial Catalog=NewsArchiveDB;Integrated Security=True;TrustServerCertificate=True;"
}
```

Or leave the file unchanged and set an environment variable instead:

```bash
ConnectionStrings__DbConn="Data Source=...;Initial Catalog=NewsArchiveDB;..."
```

### 3. Run

```bash
cd NewsArchiveSolution
dotnet build NewsArchiveSolution.slnx
dotnet run --project NewsArchive
```

Open the URL printed in the console. You can also open `NewsArchiveSolution/NewsArchiveSolution.slnx` in Visual Studio 2022+ and press F5.

### Roles

| RoleId | Role | Access |
|---|---|---|
| 1 | Admin | Full content and user management, dashboard |
| 2 | Member | Comments, profile; default for new sign-ups |

In development the password-reset code is printed to the application console.

## Branching

- `develop` all ongoing work.
- `main` stable code, updated by reviewed merges from `develop`.

## License

No license has been chosen yet. All rights reserved by the author.
