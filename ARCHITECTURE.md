# Architecture

This document describes how NewsArchive is structured today and where it is heading.
Update it whenever a layer, entity, endpoint or cross-cutting concern changes.

## Overview

**The Meridian Newsfolio** (*News Preserved. Stories Rediscovered.*) is an ASP.NET Core MVC application on .NET 10 with SQL Server. The solution keeps its original project names and is split into three projects:

```
NewsArchiveSolution/
├── DAL/          Data Access Layer    (EF Core DbContext, entities, migrations, repository interfaces, DataAccessFactory)
├── BLL/          Business Logic Layer (services, DTOs, AutoMapper profile, password and token security, validation)
└── NewsArchive/  Presentation Layer   (MVC controllers, REST API controllers, filters, Razor views, SEO, static files)
```

Dependencies flow one way only:

```
NewsArchive  ──►  BLL  ──►  DAL  ──►  SQL Server
```

The boundaries are enforced by the build:

- `BLL.csproj` references DAL with `PrivateAssets="compile"`, so the presentation layer cannot compile against DAL types. `DAL.dll` is still copied for runtime.
- DAL registers the `DbContext` and `DataAccessFactory` in `DAL/DataLayerServices.cs` (`AddDataLayer`); BLL registers AutoMapper, its services and calls `AddDataLayer` in `BLL/BusinessLayerServices.cs` (`AddBusinessLayer`). `Program.cs` only calls `AddBusinessLayer`.
- Concrete repositories are `internal` to DAL. BLL only sees the repository interfaces returned by `DataAccessFactory`.
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
| `InitialSchema` | Baseline of the original database schema |
| `NormalizeSchemaAndApprovalWorkflow` | 3NF clean-up, approval workflow columns, slugs, keys, indexes and seed data |
| `AddLanguages` | `Languages` lookup table, `Articles.LanguageCode` foreign key, Bangla names for sections |
| `AddUserTokens` | `UserTokens` table for API tokens (hash only, expiry, revocation) |

### Tables (3NF)

| Table | Primary key | Foreign keys | Unique / check constraints |
|---|---|---|---|
| `Roles` | `Id` | | `UX_Roles_RoleName` |
| `Users` | `UserId` | `RoleId → Roles` | `UQ` on `Email` |
| `ArticleStatuses` | `StatusId` | | `UX_ArticleStatuses_Name` |
| `Languages` | `LanguageCode` (`en`, `bn`) | | |
| `Categories` | `CategoryId` | | `UX_Categories_Name`, `UX_Categories_NameBn` (filtered), `UX_Categories_Slug` (filtered) |
| `Articles` | `ArticleId` | `AuthorId → Users`, `CategoryId → Categories`, `StatusId → ArticleStatuses`, `LanguageCode → Languages`, `ReviewedById → Users` | `CK_Articles_ViewsCount` (`>= 0`) |
| `Tags` | `TagId` | | `UX_Tags_Name` |
| `ContentLabels` | `(ArticleId, TagId)` | `ArticleId → Articles` (cascade), `TagId → Tags` (cascade) | |
| `Comments` | `CommentId` | `ArticleId → Articles` (cascade), `UserId → Users` | |
| `UserTokens` | `TokenId` | `UserId → Users` (cascade) | `UX_UserTokens_TokenHash` |

Normalization changes:

- The redundant `Users.ConfPassword` column was removed. Password confirmation is a form concern only.
- Article status moved to the `ArticleStatuses` lookup table instead of free text or flags.
- Review data (`ReviewedById`, `ReviewedAt`, `ReviewNote`) is stored on the article, with a foreign key to the reviewer.

### Indexes

| Index | Serves |
|---|---|
| `IX_Articles_LanguageCode_StatusId_PublishDate` (PublishDate desc) | Home page, search and RSS for each language |
| `IX_Articles_StatusId_PublishDate` (PublishDate desc) | Sitemap and cross-language listings |
| `IX_Articles_CategoryId_StatusId_PublishDate` | Section pages |
| `IX_Articles_IsBreakingNews_StatusId_PublishDate` | Breaking news and ticker |
| `IX_Articles_AuthorId_StatusId` | "My articles" for authors |
| `IX_Articles_ViewsCount` | Most viewed |
| `IX_Articles_ReviewedById` | Reviewer foreign key |
| `IX_Comments_ArticleId_CreatedDate` (desc) | Comments under an article |
| `IX_Comments_UserId`, `IX_Users_Role`, `IX_ContentLabels_TagId` | Foreign key lookups |
| `UX_UserTokens_TokenHash`, `IX_UserTokens_UserId_ExpiresAt` | Token lookup on every API call, revoke-all per user |

Duplicate indexes from the original database (`IX_Users_Email`, `IX_ContentLabels_ArticleId`, single-column article indexes) were dropped because a unique constraint, primary key or composite index already covers them.

### Repositories and factory

| Piece | Role |
|---|---|
| `Interfaces/IRepository<T>` | Generic contract: `Get(id)`, `Get()`, `Create`, `Update`, `Delete`, `Count`, `SaveChanges` |
| `Repos/Repository<T>` | Generic EF implementation; `Update` finds the row by its primary key and copies the new values |
| `Interfaces/IArticleRepository`, `ICategoryRepository`, `ICommentRepository`, `IUserRepository`, `ITokenRepository` | Entity-specific queries on top of the generic contract |
| `Repos/ArticleRepo`, `CategoryRepo`, `CommentRepo`, `UserRepo`, `TokenRepo` | `internal` implementations deriving from `Repository<T>` |
| `DataAccessFactory` | The only public entry point: `GetArticleRepository()`, `GetCategoryRepository()`, `GetCommentRepository()`, `GetUserRepository()`, `GetTokenRepository()`, `GetRoleRepository()`, `GetTagRepository()` |

Reads use `AsNoTracking`. Notable `ArticleRepo` queries:

- `Search` keyword in title, summary or content and a date range (the `to` date includes the whole day), paged.
- `GetTrending` score = views + 2 × comments, computed in SQL.
- `GetRecommended` same section, then shared tags, then most viewed.
- `GetByAuthor`, `GetByStatus`, `CountByStatus` editorial workflow.
- `GetPublishedForSitemap`, `GetLatest` sitemap and RSS.
- `IncrementViewCount` single `UPDATE` through `ExecuteUpdate`.

## Business layer (BLL)

| Folder / file | Contents |
|---|---|
| `Services/` | `ArticleService`, `CategoryService`, `CommentService`, `TagService`, `UserService`, `RoleService`, `AnalyticsService`, `AuthService`; each takes `DataAccessFactory` and `IMapper` |
| `DTOs/` | Read DTOs, form DTOs (`ArticleFormDTO` prevents over-posting of author, views and status), `LoginDTO`, `RegisterDTO`, `AuthUserDTO`, `TokenDTO`, `PagedResultDTO<T>`, `ReviewDTO`, `CommentCreateDTO` |
| `Mapping/MappingProfile.cs` | AutoMapper `Profile`, registered with `AddAutoMapper` and injected as `IMapper` |
| `Security/PasswordHasher.cs` | PBKDF2-SHA256 (100,000 iterations, 16-byte salt), legacy MD5 verification with upgrade, token generation and SHA-256 token hashing, security stamp |
| `Validations/UniqueEmail.cs` | Checks email uniqueness through `DataAccessFactory` |
| `SlugHelper.cs` | URL slugs that keep Bangla and other Unicode letters |
| `AppRoles.cs` | Role and article-status constants and display names |

`AuthService` handles sign-in (`Authenticate`, with automatic hash upgrade), `Register`, `ChangePassword`, `IssueToken`, `ValidateTokenAsync`, `LogoutAsync`, `RevokeAllAsync` and `PurgeExpiredTokens`.

On startup `ArticleService.EnsureSlugs()` and `CategoryService.EnsureSlugs()` fill missing slugs for existing rows, and `AuthService.PurgeExpiredTokens()` deletes tokens that expired or were revoked more than 30 days ago.

## Sections (categories)

Sections are not fixed. Admins can add, rename and delete any number of sections from **Account → Sections**:

- Each section has an English name and an optional Bangla name and description; the Bangla site falls back to the English name.
- Names must be unique; slugs are generated automatically and made unique with a numeric suffix.
- Deleting a section that still has articles requires choosing a target section; the articles are moved in the same transaction (`CategoryRepo.DeleteMovingArticles`).
- The navigation shows the first six sections and puts the rest under **More**.

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
| Auth | `Login`, `Create`, `Logout`, `ForgetPassword`, `VerifyOtp`, `AccessDenied` | Public |

Access is declared with `[CustomAuthorize(roles...)]` (`Filters/CustomAuthorize.cs`, an `IAuthorizationFilter`). Anonymous users are sent to `/Auth/Login?returnUrl=...`; signed-in users without the role are sent to `/Auth/AccessDenied` (HTTP 403).

All MVC controllers except `Seo` and `Pages` inherit `Controllers/AppController.cs`, which reads `CurrentUserId`, `CurrentRoleId` and `CurrentEmail` from the authentication claims and exposes `SignInUserAsync`, `Flash` and `SetArticleLookups`.

### REST API

API controllers live in `Controllers/Api`, inherit `ApiControllerBase` (`[ApiController]`, `ControllerBase`, JSON only, CORS policy, antiforgery not required because the API never reads the MVC cookie) and return `ProblemDetails` for every error.

| Method and route | Access | Success |
|---|---|---|
| `POST /api/auth/login?webClient=false` | Public, rate limited | 200 with `token`, `expiresAt`, `user` |
| `POST /api/auth/login?webClient=true` | Public, rate limited | 200; token is set in the `AuthToken` cookie (HttpOnly, SameSite=Strict, Secure on HTTPS, path `/api`) and not returned in the body |
| `POST /api/auth/register` | Public, rate limited | 201 (new Reader) |
| `GET /api/auth/me` | Any token | 200 |
| `POST /api/auth/logout` | Any token | 204, token revoked |
| `POST /api/auth/logout-all` | Any token | 204, every token of the user revoked |
| `GET /api/articles?q=&from=&to=&lang=&page=&pageSize=` | Public | 200 `PagedResultDTO<ArticleDTO>` (page size 1-50) |
| `GET /api/articles/{id}` | Public; unpublished only for its author and admins | 200 / 404 |
| `GET /api/articles/breaking`, `/trending`, `/{id}/related` | Public | 200 |
| `GET /api/articles/mine` | Author, Admin | 200 |
| `GET /api/articles/pending` | Admin | 200 |
| `POST /api/articles?submit=` | Admin, Author | 201 with `Location` |
| `PUT /api/articles/{id}?submit=` | Owner (unpublished) or Admin | 204 / 403 / 404 |
| `DELETE /api/articles/{id}` | Owner (unpublished) or Admin | 204 / 403 / 404 |
| `POST /api/articles/{id}/submit` | Author (owner) | 204 / 409 |
| `POST /api/articles/{id}/approve`, `/reject` | Admin | 204 / 409 |
| `GET /api/articles/{id}/comments` | Public | 200 |
| `POST /api/articles/{id}/comments` | Any token, rate limited | 201 |
| `DELETE /api/comments/{id}` | Comment owner or Admin | 204 / 403 |
| `GET /api/categories`, `/{id}`, `/{id}/articles?lang=` | Public | 200 |
| `POST /api/categories`, `PUT /api/categories/{id}`, `DELETE /api/categories/{id}?moveArticlesTo=` | Admin | 201 / 204 / 409 |

Token authentication is declared with `[ApiAuthorize(roles...)]` (`Filters/ApiAuthorize.cs`, a `TypeFilterAttribute`) which runs `Filters/TokenAuthorizationFilter.cs` (`IAsyncAuthorizationFilter`). The filter reads `Authorization: Bearer {token}` or the `AuthToken` cookie, validates it through `AuthService`, and returns 401 (missing, unknown, expired or revoked token) or 403 (wrong role).

Swagger UI is served at `/swagger` in Development, or anywhere when `Api:SwaggerEnabled` is `true`; use **Authorize** with the token from `/api/auth/login`.

### Views and design

- `Views/Shared/_Layout.cshtml` page shell with skip link, flash messages, header and footer partials.
- `_Navigation.cshtml` top bar, masthead with search, sticky section navigation, role menus, breaking-news ticker.
- `_Footer.cshtml` brand, sections, explore links, company and policy links, RSS.
- Story partials: `_StoryLead`, `_StoryCard`, `_StoryCompact`, built from `_StoryMedia`, `_StoryKicker`, `_StoryMeta`.
- Design tokens and components in `wwwroot/css/site.css` (Source Serif 4 and Inter, with Noto Bengali fallbacks).
- Brand assets in `wwwroot/images/brand/` (1200×630 share image, icons) and `wwwroot/site.webmanifest`.

## Languages (English and Bangla)

| Concern | Implementation |
|---|---|
| UI text | `IStringLocalizer<SharedResource>` injected as `L` in every view; English text is the key, Bangla lives in `Resources/SharedResource.bn.resx` |
| Validation and labels | DataAnnotations messages and `[Display]` names are localized through the same resource |
| Long pages | Culture-specific views (`About.bn.cshtml`, `Privacy.bn.cshtml`, ...) through view localization |
| Choosing a language | `?lang=bn` / `?lang=en` (query string provider), remembered in the culture cookie; header switcher keeps the current page |
| Content | Every article has a `LanguageCode`; listings show the current language, article pages always render in the article's own language with a matching `lang` attribute |
| SEO | `hreflang` alternates (en, bn, x-default) on listing pages, `og:locale` per language, sitemap entries for both languages, `/rss.xml?lang=bn` |
| Encoding | `WebEncoderOptions` allow all Unicode ranges so Bangla text is output directly instead of as numeric entities |

## Theme

Light, dark and system themes use Bootstrap 5.3 `data-bs-theme`. `wwwroot/js/theme-init.js`, loaded first in `<head>`, applies the saved choice before the page paints (an external file so the Content Security Policy can forbid inline scripts), `wwwroot/js/site.js` handles the switcher in the top bar and follows the operating system when **System** is selected. Colour tokens for both themes are defined in `wwwroot/css/site.css`.

## Brand

| Asset | Path |
|---|---|
| Logo mark (SVG) | `wwwroot/images/brand/logo-mark.svg` |
| Share image 1200×630 | `wwwroot/images/brand/og-default.png` |
| Icons | `wwwroot/favicon.ico`, `wwwroot/images/brand/{favicon-32,apple-touch-icon,icon-192,icon-512}.png` |
| Palette | Navy `#0b1a33`, gold `#d6b064` / text gold `#8a6516`, cream `#f3ead7`, breaking red `#c8102e` |

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
| MVC authentication | Cookie authentication (`Meridian.Auth`, HttpOnly, SameSite=Lax, Secure outside Development, 8 h sliding) with claims `NameIdentifier`, `Name`, `Email`, `Role`, `RoleId`, `SecurityStamp`; the session also stores `UserId`, `RoleId`, `Email`, `Name`. Logout signs out and clears the session |
| Session validation | Every request re-checks the cookie against the database: a deleted user, a changed role or a changed password (security stamp) signs the cookie out immediately |
| API authentication | Random 256-bit tokens; only the SHA-256 hash is stored in `UserTokens`, with client type, expiry (`Api:TokenLifetimeHours`, default 12) and revocation. Changing a password revokes every token of that user |
| Authorization | `[CustomAuthorize]` for MVC, `[ApiAuthorize]` for the API, plus ownership checks in `ArticleService.CanEdit` / `CanView` and comment ownership checks |
| Password storage | PBKDF2-SHA256, 100,000 iterations, random salt, constant-time comparison. Legacy MD5 hashes are verified once and upgraded on the next successful sign-in |
| Password policy | At least 8 characters with at least one letter and one number |
| Brute force | Per-account lockout after 5 failed sign-ins for 15 minutes (`Auth/LoginThrottle.cs`, MVC and API); per-IP rate limits: sign-in, sign-up and reset 20/min, comments 6/min, whole API 120/min; unknown emails cost the same time as known ones |
| Password reset | 6-digit OTP from a cryptographic RNG, valid 5 minutes, single use, 5 attempts, same response whether or not the email exists; logged only in Development |
| CSRF | `AutoValidateAntiforgeryToken` on every MVC POST (antiforgery cookie SameSite=Strict); API cookie is SameSite=Strict and the API only accepts JSON bodies |
| XSS | Razor encoding everywhere, article content rendered as encoded paragraphs, JSON-LD serialized with the default JavaScript encoder, image paths limited to site paths or `https://` |
| Open redirect | `returnUrl` is only followed when `Url.IsLocalUrl` accepts it |
| Security headers | `Content-Security-Policy` (scripts from self only, no objects, frame-ancestors self), `X-Content-Type-Options`, `Referrer-Policy`, `X-Frame-Options`, `Permissions-Policy`, `Cross-Origin-Opener-Policy`, `Cache-Control: no-store` on account and API responses, no `Server` header; HSTS outside Development |
| Proxies | Forwarded headers are only trusted from loopback or from addresses listed in `Hosting:KnownProxies`, so clients cannot fake their IP to dodge rate limits |
| CORS | Only origins listed in `Api:AllowedOrigins` |
| Errors | MVC: `/error/500` and `/error/{code}` pages. API: JSON `ProblemDetails` for exceptions and status codes |
| Compression | Response compression enabled for HTTPS |
| Configuration | `ConnectionStrings:DbConn` (override with `ConnectionStrings__DbConn`), `Site:*`, `Api:*`, `Hosting:KnownProxies` |
| Code style | No code comments; names must explain intent |
| Responsive layout | Verified without horizontal overflow at 320, 375, 414, 768, 1024, 1366 and 1920 px in Chrome and Edge |

## Known limitations

- Images are referenced by typed path; there is no upload.
- Tags exist in the schema but have no management UI.
- The password reset code is not emailed yet; an email sender will be plugged in later.
- Login lockout counters live in memory, so they reset when the app restarts and are per server.

## Roadmap

| Phase | Focus | Status |
|---|---|---|
| 1 | Foundation: `.gitignore`, remove duplicates, SQL-side queries, migrations, error pages | Done |
| 1b | 3NF schema, roles (Admin/Author/Reader), approval workflow, SEO, new design | Done |
| 1c | English + Bangla site, light/dark/system theme, flexible sections, brand, bilingual sample data | Done |
| 2 | Course-aligned architecture: repository interfaces, generic repository, `DataAccessFactory`, AutoMapper profile via DI, cookie authentication with claims, PBKDF2 password hashing | Done |
| 3 | REST API (`/api/...`) with `[ApiController]`, DTOs, status codes, token authentication (HttpOnly cookie and Bearer), Swagger, security hardening | Done |
| 4 | Rich text, image upload, tags UI, date archive, revisions, scheduling | Planned |
| 5 | Bangla + English localization, full-text search, bookmarks, newsletter | Planned |
| 6 | Tests, CI, health checks, Docker | Planned |
