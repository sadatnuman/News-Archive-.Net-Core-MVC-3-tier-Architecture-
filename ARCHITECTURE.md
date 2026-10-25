# Architecture

This document describes how NewsArchive is structured today and where it is heading.
Update it whenever a layer, entity, endpoint or cross-cutting concern changes.

## Overview

NewsArchive is an ASP.NET Core MVC application on .NET 10 with SQL Server, split into three projects:

```
NewsArchiveSolution/
├── DAL/          Data Access Layer    (EF Core DbContext, entities, migrations, repositories)
├── BLL/          Business Logic Layer (services, DTOs, mapping, validation, business rules)
└── NewsArchive/  Presentation Layer   (MVC controllers, filters, Razor views, SEO, static files)
```

Dependencies flow one way only:

```
NewsArchive  ──►  BLL  ──►  DAL  ──►  SQL Server
```

The boundaries are enforced by the build:

- `BLL.csproj` references DAL with `PrivateAssets="compile"`, so the presentation layer cannot compile against DAL types. `DAL.dll` is still copied for runtime.
- DAL registers its own services in `DAL/DataLayerServices.cs` (`AddDataLayer`); BLL registers its services and calls `AddDataLayer` in `BLL/BusinessLayerServices.cs` (`AddBusinessLayer`). `Program.cs` only calls `AddBusinessLayer`.
- BLL has no ASP.NET MVC or HTTP dependencies. Authorization filters live in `NewsArchive/Filters`.
- Only DAL touches the `DbContext`. Filtering, sorting, paging and counting happen in SQL.

## Roles and editorial workflow

| RoleId | Role | Can do |
|---|---|---|
| 1 | Admin | Everything: publish directly, approve or reject submissions, manage users, sections and comments |
| 2 | Reader | Read, comment, manage own profile (default for new sign-ups) |
| 3 | Author | Write drafts, submit for approval, edit or delete own unpublished articles |

Article status (`ArticleStatuses` lookup table):

```
Draft (1) ──submit──► Pending (2) ──approve──► Published (3)
   ▲                      │
   └──── edit/resubmit ── Rejected (4) ◄──reject (with note)
```

- Nothing written by an Author is public until an Admin approves it.
- Articles created by an Admin are published immediately.
- Public queries (`ArticleRepo.Published()`) only return Published articles. Non-published articles return 404 to everyone except the author and admins.
- Business rules live in `BLL/Services/ArticleService.cs` (`Create`, `Update`, `Submit`, `Approve`, `Reject`, `Delete`, `CanEdit`, `CanView`). Controllers only call them.
- Role and status ids are defined once in `DAL/EF/LookupIds.cs` and exposed to the UI through `BLL/AppRoles.cs` (`AppRoles`, `ArticleStatus`).

## Data layer (DAL)

The schema is managed with EF Core migrations in `DAL/EF/Migrations`:

| Migration | Purpose |
|---|---|
| `InitialSchema` | Baseline of the original database (matches `NewsArchiveDB.bacpac`) |
| `NormalizeSchemaAndApprovalWorkflow` | 3NF clean-up, approval workflow columns, slugs, keys, indexes and seed data |

### Tables (3NF)

| Table | Primary key | Foreign keys | Unique / check constraints |
|---|---|---|---|
| `Roles` | `Id` | | `UX_Roles_RoleName` |
| `Users` | `UserId` | `RoleId → Roles` | `UQ` on `Email` |
| `ArticleStatuses` | `StatusId` | | `UX_ArticleStatuses_Name` |
| `Categories` | `CategoryId` | | `UX_Categories_Name`, `UX_Categories_Slug` (filtered) |
| `Articles` | `ArticleId` | `AuthorId → Users`, `CategoryId → Categories`, `StatusId → ArticleStatuses`, `ReviewedById → Users` | `CK_Articles_ViewsCount` (`>= 0`) |
| `Tags` | `TagId` | | `UX_Tags_Name` |
| `ContentLabels` | `(ArticleId, TagId)` | `ArticleId → Articles` (cascade), `TagId → Tags` (cascade) | |
| `Comments` | `CommentId` | `ArticleId → Articles` (cascade), `UserId → Users` | |

Normalization changes:

- The redundant `Users.ConfPassword` column was removed. Password confirmation is a form concern only.
- Article status moved to the `ArticleStatuses` lookup table instead of free text or flags.
- Review data (`ReviewedById`, `ReviewedAt`, `ReviewNote`) is stored on the article, with a foreign key to the reviewer.

### Indexes

| Index | Serves |
|---|---|
| `IX_Articles_StatusId_PublishDate` (PublishDate desc) | Home page, search, sitemap, RSS |
| `IX_Articles_CategoryId_StatusId_PublishDate` | Section pages |
| `IX_Articles_IsBreakingNews_StatusId_PublishDate` | Breaking news and ticker |
| `IX_Articles_AuthorId_StatusId` | "My articles" for authors |
| `IX_Articles_ViewsCount` | Most viewed |
| `IX_Articles_ReviewedById` | Reviewer foreign key |
| `IX_Comments_ArticleId_CreatedDate` (desc) | Comments under an article |
| `IX_Comments_UserId`, `IX_Users_Role`, `IX_ContentLabels_TagId` | Foreign key lookups |

Duplicate indexes from the original database (`IX_Users_Email`, `IX_ContentLabels_ArticleId`, single-column article indexes) were dropped because a unique constraint, primary key or composite index already covers them.

### Repositories

`DAL/Repos/*Repo.cs` expose CRUD plus query methods; reads use `AsNoTracking`. Notable `ArticleRepo` queries:

- `Search` keyword in title, summary or content and a date range (the `to` date includes the whole day), paged.
- `GetTrending` score = views + 2 × comments, computed in SQL.
- `GetRecommended` same section, then shared tags, then most viewed.
- `GetByAuthor`, `GetByStatus`, `CountByStatus` editorial workflow.
- `GetPublishedForSitemap`, `GetLatest` sitemap and RSS.
- `IncrementViewCount` single `UPDATE` through `ExecuteUpdate`.

## Business layer (BLL)

| Folder / file | Contents |
|---|---|
| `Services/` | `ArticleService`, `CategoryService`, `CommentService`, `TagService`, `UserService`, `RoleService`, `AnalyticsService` |
| `DTOs/` | Read DTOs and form DTOs (`ArticleFormDTO` prevents over-posting of author, views and status) |
| `Validations/UniqueEmail.cs` | Checks email uniqueness through `UserRepo` |
| `SlugHelper.cs` | URL slugs that keep Bangla and other Unicode letters |
| `AppRoles.cs` | Role and article-status constants and display names |
| `MapperConfig.cs` | AutoMapper entity ↔ DTO maps |

On startup `ArticleService.EnsureSlugs()` and `CategoryService.EnsureSlugs()` fill missing slugs for existing rows.

## Presentation layer (NewsArchive)

### Public URLs

| URL | Action | Notes |
|---|---|---|
| `/` , `/?page=n` | `Article/Index` | Lead story, latest, trending sidebar |
| `/search?keyword=&from=&to=&page=` | `Article/Index` | `noindex`; `/?keyword=` redirects here (301) |
| `/news/{id}/{slug}` | `Article/Details` | Canonical article URL; `/Article/Details/{id}` and wrong slugs redirect (301) |
| `/category/{id}/{slug}` | `Category/ArticlesByCategory` | `/Category/ArticlesByCategory/{id}` redirects (301) |
| `/Category` | `Category/Index` | All sections |
| `/trending`, `/breaking-news` | `Article/TrendingNews`, `Article/BreakingNews` | |
| `/about`, `/contact`, `/editorial-policy`, `/privacy`, `/terms` | `Pages/*` | |
| `/error/{code}` | `Pages/Error` | Status code pages (404, 500) |
| `/robots.txt`, `/sitemap.xml`, `/news-sitemap.xml`, `/rss.xml` | `Seo/*` | |

### Authenticated URLs

| Controller | Actions | Access |
|---|---|---|
| Article | `Create`, `Edit/{id}`, `Delete/{id}` | Admin, Author (authors: own unpublished articles only) |
| Article | `MyArticles`, `Submit/{id}` | Author |
| Article | `AddComment` | Any signed-in user, published articles only |
| Comment | `Delete` | Comment owner or Admin |
| Admin | `Index` (dashboard), `PendingArticles`, `Approve/{id}`, `Reject/{id}` | Admin |
| Admin | `AllUser`, `Create`, `Edit/{id}`, `Delete/{id}`, `Profile`, `ChangePassword` | Admin |
| Category | `Create`, `Edit/{id}`, `Delete/{id}` | Admin |
| Member | `Profile`, `ChangePassword` | Reader, Author |
| Auth | `Login`, `Create`, `Logout`, `ForgetPassword`, `VerifyOtp` | Public |

All controllers except `Seo` and `Pages` inherit `Controllers/AppController.cs`, which exposes `CurrentUserId`, `CurrentRoleId`, `Flash` and `SetArticleLookups`.

### Views and design

- `Views/Shared/_Layout.cshtml` page shell with skip link, flash messages, header and footer partials.
- `_Navigation.cshtml` top bar, masthead with search, sticky section navigation, role menus, breaking-news ticker.
- `_Footer.cshtml` brand, sections, explore links, company and policy links, RSS.
- Story partials: `_StoryLead`, `_StoryCard`, `_StoryCompact`, built from `_StoryMedia`, `_StoryKicker`, `_StoryMeta`.
- Design tokens and components in `wwwroot/css/site.css` (Source Serif 4 and Inter, with Noto Bengali fallbacks).
- Brand assets in `wwwroot/images/brand/` (1200×630 share image, icons) and `wwwroot/site.webmanifest`.

## SEO

| Concern | Implementation |
|---|---|
| Title, description, canonical, robots | `Views/Shared/_SeoHead.cshtml`, driven by `ViewData.Meta()` (`Seo/PageMeta.cs`) |
| Social cards (LinkedIn, Facebook, X) | Open Graph and Twitter Card tags; article image or `og-default.png` (1200×630) |
| Structured data | JSON-LD `NewsMediaOrganization`, `WebSite` + `SearchAction` on every page; `NewsArticle`, `BreadcrumbList`, `CollectionPage`, `ItemList` where relevant |
| URLs | Keyword slugs, 301 redirects from legacy URLs, `rel=prev/next` on paged lists |
| Crawling | `robots.txt`, XML sitemap with image entries, Google News sitemap (last 48 hours), RSS 2.0 |
| Headings | Exactly one `h1` per page, sections use `h2`, cards `h3` |
| Absolute URLs | `Seo/SeoUrls.cs`; uses `Site:BaseUrl` when set, otherwise the request host (forwarded headers supported) |

Site identity is configured in the `Site` section of `appsettings.json` (`SiteOptions`).

## Cross-cutting concerns

| Concern | Current implementation |
|---|---|
| Authentication | Session keys `Email`, `UserId`, `RoleId` set at login (30 min idle timeout, HttpOnly, SameSite=Lax) |
| Authorization | `Filters/RoleAccess(params int[] roles)` and `Filters/Logged` |
| CSRF | `AutoValidateAntiforgeryToken` applied globally to every POST |
| XSS | Article content is HTML-encoded and rendered as paragraphs |
| Security headers | `X-Content-Type-Options`, `Referrer-Policy`, `X-Frame-Options`, `Permissions-Policy`; HSTS outside Development |
| Errors | `UseExceptionHandler("/error/500")` and `UseStatusCodePagesWithReExecute("/error/{0}")` |
| Compression | Response compression enabled for HTTPS |
| Password storage | MD5 hash (to be replaced, see roadmap) |
| Password reset | 6-digit OTP in session, valid 1 minute, written to the server console in development |
| Configuration | `ConnectionStrings:DbConn` (override with `ConnectionStrings__DbConn`), `Site:*` |
| Code style | No code comments; names must explain intent |

## Known limitations

- MD5 password hashing and session-based authentication.
- Images are referenced by typed path; there is no upload.
- Tags exist in the schema but have no management UI.
- No public JSON API yet.

## Roadmap

| Phase | Focus | Status |
|---|---|---|
| 1 | Foundation: `.gitignore`, remove duplicates, SQL-side queries, migrations, error pages | Done |
| 1b | 3NF schema, roles (Admin/Author/Reader), approval workflow, SEO, new design | Done |
| 2 | Course-aligned architecture: repository interfaces, generic repository, `DataAccessFactory`, AutoMapper profile via DI, cookie authentication with claims, stronger password hashing | Next |
| 3 | REST API (`/api/...`) with `[ApiController]`, DTOs, status codes, token authentication (HttpOnly cookie and Bearer), Swagger | Planned |
| 4 | Rich text, image upload, tags UI, date archive, revisions, scheduling | Planned |
| 5 | Bangla + English localization, full-text search, bookmarks, newsletter | Planned |
| 6 | Tests, CI, health checks, Docker | Planned |
