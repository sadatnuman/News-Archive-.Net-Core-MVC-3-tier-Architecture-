# Architecture

This document describes how NewsArchive is structured today and where it is heading.
Update it whenever a layer, entity, endpoint or cross-cutting concern changes.

## Overview

NewsArchive is an ASP.NET Core MVC application on .NET 10 with SQL Server, split into three projects:

```
NewsArchiveSolution/
├── DAL/          Data Access Layer   (EF Core DbContext, entities, repositories)
├── BLL/          Business Logic Layer (services, DTOs, mapping, validation, auth filters)
└── NewsArchive/  Presentation Layer   (MVC controllers, Razor views, static files, startup)
```

Dependencies flow one way only:

```
NewsArchive  ──►  BLL  ──►  DAL  ──►  SQL Server
```

- Controllers only talk to BLL services and receive DTOs.
- Services talk to repositories and map entities to DTOs with AutoMapper (`BLL/MapperConfig.cs`).
- Repositories own all EF Core queries. Filtering, sorting, paging and counting happen in SQL, not in memory.

## Data layer (DAL)

`DAL/EF/NewsArchiveDbContext.cs` was scaffolded database-first from the `NewsArchiveDB` schema
(a copy ships as `NewsArchiveDB.bacpac` in the repo root).

| Entity | Key fields | Relationships |
|---|---|---|
| `User` | UserId, Name, Email (unique), PasswordHash, RoleId, CreatedAt | Role (many-to-one), Articles, Comments |
| `Role` | Id, RoleName | Users |
| `Article` | ArticleId, Title, Summary, Content, PublishDate, ViewsCount, IsBreakingNews, ImagePath | Author (User), Category, Comments, Tags (many-to-many via `ContentLabels`) |
| `Category` | CategoryId, Name, Description | Articles |
| `Tag` | TagId, Name | Articles |
| `Comment` | CommentId, ArticleId, UserId, Text, CreatedDate | Article, User |

Repositories (`DAL/Repos/*Repo.cs`) expose CRUD plus query methods. Read queries use `AsNoTracking`.
Notable `ArticleRepo` queries:

- `Search` keyword in title/summary/content, date range (the `to` date includes the whole day), paging, total count.
- `GetTrending` score = views + 2 × comments, computed in SQL.
- `GetRecommended` same category, then shared tags, then most viewed, excluding the current article.
- `IncrementViewCount` single `UPDATE` statement (`ExecuteUpdate`).
- `CountPerCategory`, `GetMostViewed`, `Count` used by the admin dashboard.

## Business layer (BLL)

| Folder | Contents |
|---|---|
| `Services/` | `ArticleService`, `CategoryService`, `CommentService`, `TagService`, `UserService`, `RoleService`, `AnalyticsService` |
| `DTOs/` | Request/response models with DataAnnotations validation |
| `Validations/` | `UniqueEmail` attribute (checks the Users table) |
| `AuthFilters/` | `Logged`, `AdminAccess`, `MemberAccess` session-based authorization filters |

All services and repositories are registered as scoped in `NewsArchive/Program.cs`.

## Presentation layer (NewsArchive)

Default route: `{controller=Article}/{action=Index}/{id?}`.

### Endpoints

| Controller | Action | Method | Access |
|---|---|---|---|
| Article | Index (`keyword`, `from`, `to`, `page`, `pageSize`) | GET | Public |
| Article | BreakingNews, TrendingNews, Details/{id}, Privacy, Error | GET | Public |
| Article | Create, Edit/{id}, Delete/{id} | GET, POST | Admin |
| Article | AddComment | POST | Logged in |
| Category | Index, ArticlesByCategory/{id} | GET | Public |
| Category | Create, Edit/{id}, Delete/{id} | GET, POST | Admin |
| Comment | Delete | POST | Comment owner or Admin |
| Auth | Login, Create (sign up), ForgetPassword, VerifyOtp | GET, POST | Public |
| Auth | Logout | GET | Logged in |
| Admin | Index (dashboard), AllUser, Profile, ChangePassword, Create, Edit/{id}, Delete/{id} | GET, POST | Admin |
| Member | Profile, ChangePassword | GET, POST | Member |

### Views

- `Views/Shared/_Layout.cshtml` page shell; `_Navigation.cshtml` header with categories, breaking/trending links and the user menu.
- One folder per controller under `Views/`.
- Static assets in `wwwroot/` (Bootstrap, jQuery, article images under `wwwroot/Article/picture/`).

## Cross-cutting concerns

| Concern | Current implementation |
|---|---|
| Authentication | Session keys `Email` and `RoleId` set at login (30 min idle timeout) |
| Authorization | Role IDs: `1` = Admin, `2` = Member, enforced by BLL auth filters |
| Password storage | MD5 hash (to be replaced, see roadmap) |
| Password reset | 6-digit OTP stored in session, valid 1 minute, printed to the console in development |
| Validation | DataAnnotations on DTOs + `UniqueEmail` |
| Errors | `UseExceptionHandler("/Article/Error")` outside Development |
| Configuration | Connection string `ConnectionStrings:DbConn` in `appsettings.json`, overridable with the `ConnectionStrings__DbConn` environment variable |

## Known limitations

- MD5 password hashing and session-based auth.
- Anti-forgery tokens are missing on several POST actions.
- Article create/edit binds `AuthorId` and `ViewsCount` from the form.
- Images are referenced by typed path; there is no upload.
- Tags exist in the schema but have no management UI.
- No EF Core migrations yet; the schema comes from the bacpac.

## Roadmap

| Phase | Focus | Status |
|---|---|---|
| 1 | Foundation cleanup: `.gitignore`, remove duplicates, SQL-side queries, migrations, error pages | In progress |
| 2 | ASP.NET Core Identity, CSRF everywhere, lockout, rate limiting, security headers, `IEmailSender` abstraction | Planned |
| 3 | Slugs, article status, rich text, image upload, tags UI, SEO URLs, date archive | Planned |
| 4 | Editorial workflow (Reporter/Editor/Admin), revisions, scheduling, admin CMS, soft delete | Planned |
| 5 | International homepage, article page features, full-text search, bookmarks, newsletter, Bangla + English | Planned |
| 6 | Structured data, sitemaps, RSS, caching, accessibility, dark mode, public API | Planned |
| 7 | Tests, CI, health checks, Docker | Planned |
