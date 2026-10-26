# Architecture

How **The Meridian Newsfolio** is built: the three layers, the data model, every feature, the security model and the tooling. Keep this file up to date whenever a layer, table, endpoint or cross-cutting concern changes.

**Contents**

1. [Overview](#1-overview)
2. [Layers and dependency injection](#2-layers-and-dependency-injection)
3. [Data model](#3-data-model)
4. [Roles and the editorial workflow](#4-roles-and-the-editorial-workflow)
5. [Features](#5-features)
6. [Security](#6-security)
7. [REST API](#7-rest-api)
8. [Presentation layer](#8-presentation-layer)
9. [Languages, theme and design](#9-languages-theme-and-design)
10. [SEO](#10-seo)
11. [Performance and offline support](#11-performance-and-offline-support)
12. [Testing and tooling](#12-testing-and-tooling)
13. [Known limitations](#13-known-limitations)

---

## 1. Overview

An ASP.NET Core MVC news archive on **.NET 10** and **SQL Server**, published in **English and Bangla**. Readers browse, search, save and comment on stories; authors write them; admins review, schedule and moderate. A token-protected REST API exposes the same features to apps.

```
NewsArchiveSolution/
├── DAL/               Data Access Layer     EF Core DbContext, entities, migrations, repositories, DataAccessFactory
├── BLL/               Business Logic Layer  services, rules, DTOs, AutoMapper profile, security, audit, caching contracts
├── NewsArchive/       Presentation Layer    MVC + API controllers, filters, Razor views, email, SEO, wwwroot
└── NewsArchive.Tests/ xUnit tests           rules and services with in-memory fakes
```

Dependencies flow one way:

```
NewsArchive  ──►  BLL  ──►  DAL  ──►  SQL Server
```

## 2. Layers and dependency injection

### Boundaries enforced by the build

- `BLL.csproj` references DAL with `PrivateAssets="compile"`, so the presentation layer cannot compile against DAL types (`DAL.dll` is still copied for runtime).
- Concrete repositories are `internal sealed` in DAL. BLL only sees the interfaces in `DAL/Interfaces`, handed out by `IDataAccessFactory`.
- BLL has no MVC or HTTP dependencies. Anything it needs from the web host is an interface that the presentation layer implements:

| BLL contract | Implemented in the UI by | Purpose |
|---|---|---|
| `Media/IFileStorage` | `Media/WebRootFileStorage` | Save and delete uploaded pictures under `wwwroot/uploads` |
| `Audit/IActorContext` | `Auth/HttpActorContext` | Who is acting (cookie or API token) and from which IP |
| `Email/IEmailSender` | `Email/OutboxEmailSender`, `Email/SmtpEmailSender` | Send email; outbox in Development, SMTP once configured |
| `Caching/IAppCache` | `Caching/MemoryAppCache` | In-memory cache with invalidation by region |

- Only DAL touches the `DbContext`. Filtering, sorting, paging and counting run in SQL; bulk changes use `ExecuteUpdate` / `ExecuteDelete`.

### Registrations

| Layer | Registered in | What |
|---|---|---|
| DAL | `DataLayerServices.AddDataLayer` | `NewsArchiveDbContext`, `IDataAccessFactory` → `DataAccessFactory` (scoped) |
| BLL | `BusinessLayerServices.AddBusinessLayer` | AutoMapper profile and 18 scoped services: `IArticleService`, `ICategoryService`, `ICommentService`, `ITagService`, `IUserService`, `IRoleService`, `IAnalyticsService`, `IAuthService`, `IMediaService`, `ISystemService`, `IAuthorService`, `IBookmarkService`, `IArticleLookupService`, `IRecommendationService`, `ISearchService`, `IAuditService`, `INewsletterService` |
| UI | `Program.cs` | The four contracts above, `LoginThrottle`, `PasswordResetStore`, `PasswordChanger`, `MailComposer`, `CookieSessionValidator`, `SeoUrls`, the `ScheduledPublisher` hosted service, rate limiting, health checks, Swagger |

- `ValidateOnBuild` and `ValidateScopes` are on, so a missing registration or captive dependency stops the app at startup.
- Nothing resolves services by hand except `ScheduledPublisher`, which creates one scope per run through `IServiceScopeFactory` (the supported pattern for background services).
- Pure rules with no dependencies are static classes in `BLL/Rules`: `ArticleRules`, `RecommendationRules`, `ModerationRules`, `NewsletterRules`, `SearchRules`, `MediaLimits`, `TokenClients`; `BLL/Audit/AuditActions.cs` holds `AuditRules`.

### Startup

`BusinessLayerServices.InitializeBusinessLayer` runs once:

1. Applies pending migrations (always in Development, or when `Database:MigrateOnStartup` is `true`).
2. Fills missing slugs for articles, sections and tags.
3. Purges expired tokens (30 days), audit entries older than 365 days and unconfirmed newsletter sign-ups older than 7 days.

If the database is unreachable, the log names the setting to check.

## 3. Data model

All schema changes are EF Core migrations in `DAL/EF/Migrations` (generated comments stripped). The design is in third normal form: lookups for roles, statuses, languages and report reasons, and join tables for many-to-many links.

### Tables

| Table | Key | Foreign keys | Notes |
|---|---|---|---|
| `Roles` | `Id` | | Seeded: Admin 1, Reader 2, Author 3 |
| `Users` | `UserId` | `RoleId → Roles` | Unique email, PBKDF2 hash, avatar path, bio |
| `ArticleStatuses` | `StatusId` | | Seeded: Draft 1, Pending 2, Published 3, Rejected 4, Scheduled 5 |
| `Languages` | `LanguageCode` | | Seeded: `en`, `bn` |
| `Categories` | `CategoryId` | | English and Bangla name and description, unique slug |
| `Articles` | `ArticleId` | `AuthorId`, `ReviewedById → Users`; `CategoryId`, `StatusId`, `LanguageCode` | Slug, summary, Markdown content, views, breaking flag, schedule time, review note |
| `Tags` | `TagId` | | English and Bangla name, unique slug |
| `ContentLabels` | `(ArticleId, TagId)` | both cascade | Article ↔ tag |
| `Comments` | `CommentId` | `ArticleId` (cascade), `UserId`, `ModeratedById → Users` | `IsHidden`, `ModeratedAt` |
| `ReportReasons` | `ReasonId` | | Seeded: Spam, Abuse or harassment, Off topic, Misinformation, Other |
| `CommentReports` | `(CommentId, UserId)` | `CommentId` (cascade), `UserId`, `ReasonId` | One report per reader per comment, optional note |
| `Bookmarks` | `(UserId, ArticleId)` | both cascade | Saved stories |
| `ReadingHistory` | `(UserId, ArticleId)` | both cascade | First and last read time, read count |
| `UserTokens` | `TokenId` | `UserId` (cascade) | SHA-256 of the API token, client type, expiry, revocation |
| `AuditLogs` | `AuditLogId` | `UserId → Users` (set null) | Action, entity, id, details, IP, time |
| `Subscribers` | `SubscriberId` | `LanguageCode → Languages` | Unique email, confirm-token hash, unsubscribe token, confirmed, unsubscribed and last-sent times |

### Migrations

`InitialSchema` → `NormalizeSchemaAndApprovalWorkflow` → `AddLanguages` → `AddUserTokens` → `AddTagTranslationsAndSlugs` → `AddUserAvatars` → `AddUserBio` → `AddBookmarks` → `AddScheduledPublishing` → `AddReadingHistory` → `AddCommentModeration` → `AddAuditLog` → `AddNewsletterSubscribers`.

### Indexes worth knowing

| Index | Serves |
|---|---|
| `IX_Articles_LanguageCode_StatusId_PublishDate` (date desc) | Home page, search, RSS per language |
| `IX_Articles_CategoryId_StatusId_PublishDate` | Section pages |
| `IX_Articles_IsBreakingNews_StatusId_PublishDate` | Breaking news and ticker |
| `IX_Articles_StatusId_ScheduledAt` | Scheduled publishing |
| `IX_Comments_ArticleId_IsHidden` | Visible comments under a story |
| `IX_ReadingHistory_UserId_LastReadAt` (desc) | Recommendations and the history page |
| `IX_AuditLogs_CreatedAt`, `IX_AuditLogs_EntityType_CreatedAt` | Audit log viewer and filters |
| `UX_UserTokens_TokenHash` | Token lookup on every API call |
| `UX_Subscribers_ConfirmTokenHash`, `UX_Subscribers_UnsubscribeToken` | Confirm and unsubscribe links |

### Repositories

| Piece | Role |
|---|---|
| `IRepository<T>` / `Repository<T>` | Generic `Get(id)`, `Get()`, `Create`, `Update`, `Delete`, `Count`, `SaveChanges` |
| `IArticleRepository`, `ICategoryRepository`, `ICommentRepository`, `IUserRepository`, `ITokenRepository`, `ITagRepository` | Entity queries on top of the generic contract |
| `IBookmarkRepository`, `IReadingHistoryRepository`, `IAuditLogRepository`, `ISubscriberRepository` | Join tables and append-only tables |
| `IDataAccessFactory` | The only public entry point: one `GetXRepository()` per repository, plus `CanConnect()` |

Reads use `AsNoTracking`. Public queries go through `ArticleRepo.Published()`, so drafts, pending, rejected and scheduled stories never leak.

## 4. Roles and the editorial workflow

| Role | Can do |
|---|---|
| **Admin** | Everything: publish directly, approve, reject or schedule submissions, manage users, sections, tags, comments and the newsletter, read the audit log |
| **Author** | Write drafts, submit them, edit or delete their own unpublished articles |
| **Reader** | Read, comment, report comments, save stories, keep a reading history (default for sign-ups) |

```
Draft (1) ──submit──► Pending (2) ──approve──────────────► Published (3)
   ▲                     │   └──approve with a time──► Scheduled (5) ──time reached / publish now──┘
   └── edit / resubmit ── Rejected (4) ◄── reject with a note
```

- Nothing an author writes is public until an admin approves it.
- Scheduling accepts times from one minute to one year ahead. `ScheduledPublisher` calls `ArticleService.PublishDue()` every 30 seconds.
- Ownership rules live in `ArticleRules.CanEdit` and `CanView`. Non-public stories return 404 to everyone except the author and admins.

## 5. Features

### Reading

| Feature | Where | Notes |
|---|---|---|
| Home page | `/` | Lead story, latest, trending, popular topics, *On this day*, *Recommended for you*, newsletter sign-up |
| Sections | `/category/{id}/{slug}?page=` | Paged, 13 per page, `rel=prev/next` |
| Topics | `/tags`, `/tag/{id}/{slug}?page=` | Up to 8 topics per article |
| Archive | `/archive`, `/archive/{yyyy}`, `/archive/{yyyy}/{MM}[/{DD}]` | Month tiles and a day calendar |
| Authors | `/authors`, `/author/{id}/{slug}` | Picture, bio, paged stories, `ProfilePage` JSON-LD |
| Search | `/search?keyword=&from=&to=` | Title, summary and text, date range |
| Search suggestions | header and search page | Stories, topics, sections and authors as you type (see below) |
| Saved stories | `/bookmarks` | Bookmark button on every card |
| Reading history | `/history` | See, remove one or clear all |
| Comments | article page | Readers report; admins hide, keep or delete |

### Recommendations

`BLL/Services/RecommendationService` scores candidates in the same language. The weights live in `RecommendationRules`.

| Signal | Weight |
|---|---|
| Each shared topic | 3.0 |
| Similar headline and summary words (Jaccard; English and Bangla stop words removed; Unicode-normalized) | up to 6.0 |
| Same section | 2.0 |
| Same author | 1.0 |
| Freshness (decays over about 30 days) | up to 1.5 |
| Popularity | 0.6 × log10(1 + views) |
| Breaking news | +0.3 |
| Already read by this reader | score × 0.25 |

- **For an article** (`ForArticle`): never the current story or an unpublished one. Each result carries its strongest reason (`SharedTopic` with the tag, `SimilarStory`, `SameSection`, `SameAuthor`, `Popular`), shown as a chip.
- **For a reader** (`ForReader`): a profile from the last 20 reads, newer reads weighing more (`1 / (1 + 0.15 × position)`).
- History: signed-in reads go to `ReadingHistory` (capped at 200 stories per reader). Anonymous visitors' last 20 stories live only in the server session (`Personalization/SessionReadingList`).
- Article page: **Up next** card, **More like this** (four cards), **Recommended for you** in the sidebar after two reads, a reading progress bar, and a **Read next** pop-up after 45% of the story that hides while *Up next* is on screen.

### Search suggestions

`GET /api/search/suggest?q=&lang=` through `ISearchService`: up to 8 stories, 3 topics, 2 sections and 2 authors (authors only if they have published stories). Queries are trimmed to 2-60 characters, and `%` and `_` are matched literally. The dropdown is an accessible combobox: arrow keys, Enter, Escape, matched text highlighted.

### Comment moderation

- Readers report a comment with a reason and an optional note (300 characters). They cannot report their own comment or report one twice.
- The third report hides the comment automatically until an admin reviews it.
- `/Admin/Comments` has two tabs, **Reported** and **Hidden**. **Keep** shows the comment again and clears its reports; **Hide** removes it from readers; **Delete** removes it permanently.
- Hidden comments are excluded from counts, trending and the API. Admins see them on the article with a label.

### Newsletter

- Double opt-in: the footer and home page form sends a confirmation link valid for 48 hours. Only the SHA-256 of the confirm token is stored. The response is the same whether or not the address is known.
- Requests for the same address within 2 minutes don't send a second email.
- Every email has an unsubscribe link and `List-Unsubscribe` headers. The unsubscribe page asks for confirmation, so link scanners can't unsubscribe anyone; one-click `POST` also works.
- `/Admin/Newsletter` shows counts per edition, previews the English and Bangla digests (stories from the past 7 days, up to 6) and sends them. Every send is audited.

### Audit log

`IAuditService.Record` is called by the services after each successful action, so the website and the API log the same way. `/Admin/Audit` filters by type (Articles, Comments, Users, Sections, Topics, Newsletter, Security) and by person; `GET /api/audit` returns the same data.

| Type | Actions |
|---|---|
| Article | Published, approved, scheduled, rejected (with note), published early, unscheduled, published on schedule, edited by an admin, deleted by an admin |
| Comment | Hidden, auto-hidden after reports, kept, removed by someone other than the writer |
| User | Created, updated, role changed (from → to), deleted |
| Section / Tag | Created, updated, deleted (articles moved to …) |
| Newsletter | Digest sent (edition, stories, recipients), subscriber removed |
| Security | Signed in, failed sign-in (unknown emails masked), account created, password changed, password change refused, reset code requested, signed out everywhere |

Passwords and codes are never written to the log. Entries are kept for 365 days.

### Writing

- Markdown with a toolbar and a live preview (`Content/ArticleRenderer`, Markdig). Raw HTML is disabled; only `http`, `https`, `mailto`, `#` and site paths are allowed as links, and only site or `https` images. Paths starting with `//` or `/\` and anything containing a backslash are rejected. External links get `rel="nofollow noopener noreferrer"`; headings are shifted down one level.
- Picture upload: JPEG, PNG, WebP or GIF, detected from the file's first bytes. Limits: 5 MB for articles, 2 MB for profile pictures. Files get random names under `wwwroot/uploads`.

## 6. Security

| Concern | Implementation |
|---|---|
| Passwords | PBKDF2-SHA256, 100,000 iterations, random salt, constant-time compare. Legacy MD5 hashes are accepted once and upgraded. Policy: 8+ characters with a letter and a number |
| Website sign-in | Cookie with claims (`NameIdentifier`, `Name`, `Email`, `Role`, `RoleId`, `SecurityStamp`, avatar), HttpOnly, SameSite=Lax, Secure outside Development, 8 h sliding. `CookieSessionValidator` re-checks every request: a deleted account, changed role or changed password signs the cookie out |
| Sign-out | `POST` with an antiforgery token; `GET /Auth/Logout` only shows a confirmation page |
| API tokens | 256-bit random. Only the SHA-256 is stored, with expiry (`Api:TokenLifetimeHours`) and revocation. Sent as `Bearer` or as the HttpOnly `AuthToken` cookie (SameSite=Strict, path `/api`). Changing a password revokes every token |
| Brute force | 5 failed sign-ins lock the account for 15 minutes (website and API). Unknown emails cost the same time as known ones |
| Rate limits (per IP per minute) | Sign-in, sign-up and reset 20; comments 6; reports 10; newsletter 5; whole API 120 |
| Password change | Requires the current password, which is throttled and audited. Right after an emailed-code reset, the verified code counts instead for 10 minutes |
| Password reset | 6-digit code from a cryptographic RNG, stored as a hash in `PasswordResetStore`. Valid 10 minutes, single use, 5 attempts. At most 3 codes per account per hour, and a new code doesn't reset the attempt count. Same answer whether the account exists or not; sent through `IEmailSender` |
| Authorization | `[CustomAuthorize(roles)]` for MVC (redirect to sign-in or 403 page), `[ApiAuthorize(roles)]` for the API (401/403 `ProblemDetails`), ownership checks in services |
| CSRF | `AutoValidateAntiforgeryToken` on every MVC POST, antiforgery cookie SameSite=Strict. The API accepts JSON only and never reads the MVC cookie |
| XSS | Razor encoding, safe Markdown renderer, JSON-LD serialized with the default encoder, image paths limited to site paths or `https://` |
| Headers | CSP (`script-src 'self'`, no inline scripts), `X-Content-Type-Options`, `Referrer-Policy`, `X-Frame-Options`, `Permissions-Policy`, `Cross-Origin-Opener-Policy`, `no-store` on account and API responses, no `Server` header, HSTS outside Development |
| Redirects | `returnUrl` is followed only when `Url.IsLocalUrl` accepts it |
| Proxies | Forwarded headers trusted only from loopback or `Hosting:KnownProxies` |
| Privacy | Masked emails in logs, privacy page explains reading history and the newsletter in both languages |

## 7. REST API

Controllers in `Controllers/Api` inherit `ApiControllerBase` (`[ApiController]`, JSON only, CORS policy, `ProblemDetails` for every error). Swagger UI is at `/swagger` in Development, or anywhere when `Api:SwaggerEnabled` is `true`.

| Method and route | Access | Result |
|---|---|---|
| `POST /api/auth/login?webClient=` | Public, rate limited | 200 token (or HttpOnly cookie when `webClient=true`) / 401 / 429 |
| `POST /api/auth/register` | Public, rate limited | 201 / 409 |
| `GET /api/auth/me`, `PUT`/`DELETE /api/auth/me/avatar` | Token | 200 / 204 |
| `POST /api/auth/logout`, `/logout-all` | Token | 204 |
| `GET /api/articles?q=&from=&to=&lang=&page=&pageSize=` | Public | Paged list |
| `GET /api/articles/{id}` | Public (unpublished: author and admins) | 200 / 404 |
| `GET /api/articles/breaking`, `/trending`, `/{id}/related` | Public | 200 |
| `GET /api/articles/{id}/recommendations?count=` | Public (a token lowers stories already read) | List with `reason`, `reasonTag`, `score` |
| `GET /api/articles/mine` | Author, Admin | 200 |
| `GET /api/articles/pending`, `/scheduled` | Admin | 200 |
| `POST /api/articles?submit=`, `PUT /api/articles/{id}?submit=`, `DELETE /api/articles/{id}` | Author (own, unpublished), Admin | 201 / 204 / 403 / 404 |
| `POST /api/articles/{id}/submit` | Author | 204 / 409 |
| `POST /api/articles/{id}/approve` (`{ "publishAt": ... }`), `/reject`, `/publish-now` | Admin | 204 / 409 |
| `GET`/`POST /api/articles/{id}/comments` | Public / token, rate limited | 200 / 201 |
| `GET /api/comments/report-reasons` | Public | 200 |
| `POST /api/comments/{id}/report` | Token, rate limited | 202 `{ hidden }` / 400 / 403 / 404 / 409 |
| `DELETE /api/comments/{id}` | Writer or Admin | 204 / 403 |
| `GET /api/comments/reported`, `/hidden`; `POST /api/comments/{id}/hide`, `/keep` | Admin | 200 / 204 |
| `GET /api/categories`, `/{id}`, `/{id}/articles?lang=&page=&pageSize=` | Public | 200 |
| `POST`/`PUT`/`DELETE /api/categories` | Admin | 201 / 204 / 409 |
| `GET /api/tags?lang=`, `/{id}`, `/{id}/articles` ; `POST`/`PUT`/`DELETE` | Public / Admin | 200 / 201 / 204 |
| `GET /api/authors?lang=`, `/{id}`, `/{id}/articles` | Public | 200 / 404 |
| `GET /api/archive?lang=`, `/{year}/{month}?day=`, `/{year}/{month}/days` | Public | 200 / 404 |
| `GET /api/search/suggest?q=&lang=&count=` | Public | `{ query, searchUrl, items[] }` |
| `GET`/`POST`/`DELETE /api/bookmarks[/{articleId}]` | Token, own list | 200 / 204 / 404 |
| `GET /api/recommendations?lang=&count=` | Token | Personal recommendations |
| `GET /api/history`, `POST`/`DELETE /api/history/{articleId}`, `DELETE /api/history` | Token, own history | 200 / 204 / 404 |
| `GET /api/audit?type=&user=&page=&pageSize=` | Admin | Paged audit entries |
| `POST /api/media/images` (multipart `file`) | Author, Admin | 201 `{ path }` |

## 8. Presentation layer

### Website URLs (signed in)

| Controller | Actions | Access |
|---|---|---|
| Article | `Create`, `Edit/{id}`, `Delete/{id}`, `Preview` | Admin, Author (own unpublished) |
| Article | `MyArticles`, `Submit/{id}` | Author |
| Article, Comment | `AddComment`, `Comment/Delete`, `Comment/Report` | Signed in |
| Admin | `Index`, `PendingArticles`, `Approve`, `Reject`, `PublishNow`, `Unschedule` | Admin |
| Admin | `Comments`, `HideComment`, `KeepComment`, `DeleteComment` | Admin |
| Admin | `Newsletter`, `SendDigest`, `RemoveSubscriber`, `Audit` | Admin |
| Admin | `AllUser`, `Create`, `Edit/{id}`, `Delete/{id}`, `Profile`, `ChangePassword` | Admin (cannot change their own role or delete themselves) |
| Category, Tag | `Create`, `Edit/{id}`, `Delete/{id}` | Admin |
| Member | `Profile`, `ChangePassword` | Reader, Author |
| Avatar | `Upload`, `Remove` | Signed in, own picture |
| Bookmark, History | `/bookmarks`, `/bookmarks/toggle`, `/history`, `/history/remove`, `/history/clear` | Signed in, own data |
| Auth | `Login`, `Create`, `Logout`, `ForgetPassword`, `VerifyOtp`, `AccessDenied` | Public |
| Newsletter | `/newsletter/subscribe`, `/newsletter/confirm`, `/newsletter/unsubscribe` | Public |

All MVC controllers except `Seo` and `Pages` inherit `AppController`, which exposes `CurrentUserId`, `CurrentRoleId`, `CurrentEmail`, `CurrentLanguage`, `SignInUserAsync`, `SetArticleLookups` and `Flash`. Flash messages store the English key and the layout translates it.

### Views

- `_Layout`, `_Navigation` (top bar, masthead, search with suggestions, section menu, ticker), `_Footer` (sections, links, newsletter form).
- Story partials: `_StoryLead`, `_StoryCard`, `_StoryCompact`, built from `_StoryMedia`, `_StoryKicker`, `_StoryMeta`.
- Recommendation partials: `_UpNext`, `_RecCard`, `_RecCompact`, `_RecReason`. Other partials: `_NewsletterForm`, `_SaveButton`, `_Avatar`, `_Globe`, `_ChangePasswordForm`.
- `wwwroot/js/site.js` (no inline scripts) provides:
  - Theme switcher and password eye button.
  - Tag limit and upload size check in the article form.
  - Bookmarks, newsletter form and search suggestions.
  - Markdown toolbar and preview.
  - Reading progress and the Read next pop-up.
  - Confirmation dialogs, tilt cards, scroll reveal and service worker registration.

### Email

`MailComposer` builds the confirmation, digest and reset-code emails in English or Bangla, as plain text plus HTML with absolute links. `OutboxEmailSender` logs every email (address masked) and in Development writes it to `App_Data/outbox`. `SmtpEmailSender` is used automatically when `Email:Smtp:Host` is set.

## 9. Languages, theme and design

| Concern | Implementation |
|---|---|
| UI text | `IStringLocalizer<SharedResource>` as `L` in every view; English is the key, Bangla is in `Resources/SharedResource.bn.resx` (580+ entries) |
| Long pages | Culture-specific views (`About.bn.cshtml`, `Privacy.bn.cshtml`, …) |
| Switching | `?lang=bn` / `?lang=en`, remembered in the culture cookie |
| Content | Each article has a language. Listings, recommendations and suggestions show the current language; article pages use the article's own `lang` |
| Theme | Classic newspaper look: white masthead, ink `#121212`, red `#c8102e`; dark mode `#121212` with `#ff4d5e`. Light, dark and system modes, applied before paint by `theme-init.js` |
| 3D layer | Rotating CSS meridian globe, tilt cards with glare, glass sticky navigation, scroll reveal. Everything stops under `prefers-reduced-motion` |
| Pictures | `wwwroot/images/articles`: AI-generated photographic scenes, fictional with no real people, text or logos, 1200×675. `wwwroot/images/avatars`: AI-generated portraits of the fictional sample users, 320×320 |
| Responsive | Checked without horizontal scrolling from 320 to 1920 px; tap targets at least 24 px |

## 10. SEO

| Concern | Implementation |
|---|---|
| Meta | Title, description, canonical, robots via `ViewData.Meta()` and `_SeoHead` |
| Social | Open Graph and Twitter cards, 1200×630 default image |
| Structured data | `NewsMediaOrganization`, `WebSite` + `SearchAction`, `NewsArticle`, `BreadcrumbList`, `CollectionPage`, `ItemList`, `ProfilePage` |
| URLs | Keyword slugs (Bangla kept), 301 from old URLs and wrong slugs, `rel=prev/next` on paged lists |
| Crawling | `robots.txt`, XML sitemap with images for both languages, Google News sitemap (48 h), RSS 2.0 per language, all dates in invariant culture |
| Private pages | `noindex` on account, admin, history, newsletter and offline pages |

## 11. Performance and offline support

- **Data cache** (`IAppCache`, in memory):
  - Sections are cached for 30 minutes. They are read by the menu, the footer and every story list.
  - Trending stories, popular topics and section counts are cached for 2 minutes.
  - Any publish, edit, delete, schedule or moderation clears the affected region at once.
  - Whole pages are deliberately not cached, because each page carries per-visitor antiforgery tokens.
- **Queries**: paging, counting and grouping run in SQL, and the sections page counts in one grouped query.
- **Static files**:
  - Versioned CSS and JS (`?v=`) are `immutable` for a year.
  - Pictures under `/images` and `/uploads` are cached for a day.
  - `sw.js` is `no-cache`.
  - Responses are compressed over HTTPS.
- **PWA**:
  - `site.webmanifest`, plus `wwwroot/sw.js`, which precaches the offline page, icons and core CSS and JS.
  - Static assets are served stale-while-revalidate.
  - A failed page load falls back to the bilingual `offline.html`.
  - The API, admin, account, history, bookmark and newsletter paths are never cached.

## 12. Testing and tooling

| Tool | What |
|---|---|
| `NewsArchive.Tests` (xUnit, 71 tests) | Rules (articles, slugs, passwords, recommendations, moderation, search, newsletter, audit) and services (comments, newsletter, recommendations, audit) with DispatchProxy-based fakes, so no database is needed |
| `.github/workflows/build.yml` | On every push and PR to `develop`/`main`: restore, Release build, unit tests, vulnerable package report |
| `.vscode/` | Launch configuration and `build`, `watch`, `test` and `database update` tasks |
| `global.json` | .NET 10 SDK, latest feature band |
| `/health` | Database reachability check |

Packages: EF Core 10.0.14, Swashbuckle 10.3.1, Markdig 1.5.0. AutoMapper 14.0.0 is kept for the course pattern; advisory GHSA-rvv3-g6hj-g44x is suppressed because every map is flat and capped with `MaxDepth(3)`.

Code style: no code comments; names explain intent; culture-invariant formatting for machine output; compiled logging (`LoggerMessage`); repositories are `sealed`.

## 13. Known limitations

- Uploaded pictures are stored as sent (type and size checked, not resized) on the web server's disk, so `wwwroot/uploads` must be backed up and shared if several servers run the site.
- Sign-in lockouts, reset codes and the data cache live in memory, so they reset when the app restarts and are per server.
- Email goes to the log and the Development outbox until SMTP settings are added.
