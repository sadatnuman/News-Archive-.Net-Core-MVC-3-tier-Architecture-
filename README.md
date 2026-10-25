# The Meridian Newsfolio

*News Preserved. Stories Rediscovered.* · *সংবাদ সংরক্ষিত, গল্পের নতুন আবিষ্কার।*

A bilingual (English and Bangla) news publishing and archive web application built with ASP.NET Core MVC (.NET 10) and SQL Server, using a 3-tier architecture (Presentation → Business Logic → Data Access).

See [ARCHITECTURE.md](ARCHITECTURE.md) for the design, database schema, URL map and roadmap.

## Features

**Readers**
- English and Bangla editions with a language switcher; every page, form and message is translated
- Light, dark and system themes
- Home page with a lead story, latest news, trending stories and a breaking-news ticker
- Sections, topics (tags), trending and breaking news, search by keyword and date range
- Article pages with byline, reading time, share buttons (LinkedIn, X, Facebook, WhatsApp), comments and related stories
- Sign up, sign in, profile with a profile picture, password change and password reset with a one-time code; every password field has a show/hide eye button

**Authors**
- Write articles in English or Bangla, upload a picture, tag them with up to 8 topics, save drafts and submit them for approval
- "My articles" page with the status of every article and the editor's notes on rejected ones

**Admins**
- Approve or reject submitted articles; nothing written by an author is published without approval
- Add, rename and delete any number of sections, with English and Bangla names; articles are moved when a section is deleted
- Manage topic tags in English and Bangla; every tag gets its own page at `/tag/{id}/{slug}`
- Manage users and roles, and moderate comments
- Dashboard with totals, the review queue, most-viewed articles and articles per section

**REST API**
- JSON API under `/api` for articles, sections, tags, comments and accounts, with paging, language filters and proper status codes
- Token authentication: `Authorization: Bearer {token}` for apps, or an HttpOnly `AuthToken` cookie for browser clients (`?webClient=true`)
- Role checks on every protected endpoint, token expiry, logout and "log out everywhere"
- Interactive documentation with Swagger UI at `/swagger`

**Security**
- PBKDF2 password hashing (old MD5 hashes are upgraded automatically on the next sign-in)
- Cookie sign-in with claims; a changed password, role or deleted account ends existing sessions
- Account lockout after 5 failed sign-ins, rate limits on sign-in, comments and the API
- CSRF protection, strict Content Security Policy and other security headers, safe redirects

**SEO and sharing**
- Clean URLs such as `/news/12/headline-of-the-story` with 301 redirects from old URLs
- Title, meta description, canonical URL, robots rules and `hreflang` on every page
- Open Graph and Twitter Card tags with a branded 1200×630 share image
- JSON-LD structured data (`NewsArticle`, `NewsMediaOrganization`, `WebSite`, `BreadcrumbList`)
- `robots.txt`, XML sitemap for both languages, Google News sitemap and RSS feeds (`/rss.xml`, `/rss.xml?lang=bn`)

**Responsive design**
- Checked without horizontal scrolling from 320 px phones to 1920 px desktops in Chrome and Edge

## Tech stack

| Layer | Technology |
|---|---|
| Framework | ASP.NET Core MVC, .NET 10 (SDK pinned by `global.json`, any 10.0.x feature band) |
| Data | Entity Framework Core 10.0.14 (migrations), SQL Server |
| Mapping | AutoMapper 14 (profile registered through dependency injection, `MaxDepth` safeguard) |
| Authentication | Cookie authentication with claims (MVC), hashed bearer and cookie tokens (API) |
| API docs | Swashbuckle (Swagger / OpenAPI) |
| Localization | ASP.NET Core localization, resource files, culture-specific views |
| UI | Razor views, Bootstrap 5.3, Bootstrap Icons, Cinzel / Source Serif 4 / Inter / Noto Bengali |

## Project structure

```
NewsArchiveSolution/
├── DAL/          EF Core DbContext, entities, migrations, repository interfaces, DataAccessFactory
├── BLL/          Services, DTOs, AutoMapper profile, password and token security, validation
└── NewsArchive/  MVC and API controllers, filters, views, localization, SEO, wwwroot, Program.cs
database/
└── sample-data.sql    Bilingual sample content (users, sections, articles, comments)
```

## Getting started

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- SQL Server (Express or Developer edition)
- EF Core tools: `dotnet tool install --global dotnet-ef`

### 1. Configure the connection string

In `NewsArchiveSolution/NewsArchive/appsettings.json` set `ConnectionStrings:DbConn`, for example:

```json
"DbConn": "Data Source=localhost\\SQLEXPRESS;Initial Catalog=NewsArchiveDB;Integrated Security=True;TrustServerCertificate=True;"
```

`ConnectionStrings__DbConn` can also be set as an environment variable.

### 2. Create the database

```bash
cd NewsArchiveSolution
dotnet ef database update --project DAL --startup-project NewsArchive
```

This creates every table, key and index, plus the seed data for roles, article statuses and languages.

### 3. Load the sample content (optional)

```bash
sqlcmd -S "localhost\SQLEXPRESS" -E -d NewsArchiveDB -f 65001 -i database/sample-data.sql
```

`-f 65001` is required so Bangla text is read as UTF-8. The script replaces all users, sections, articles, tags and comments.

Sample accounts (all use the password `Meridian@2026`; change them before going live):

| Email | Role |
|---|---|
| `admin@meridiannewsfolio.local` | Admin |
| `farhana@meridiannewsfolio.local`, `tanvir@meridiannewsfolio.local` | Author |
| `nusrat@meridiannewsfolio.local`, `imran@meridiannewsfolio.local`, `sadia@meridiannewsfolio.local` | Reader |

The sample stories are fictional. While `Site:ShowDemoNotice` is `true`, a banner tells visitors that the content is sample content. Set it to `false` once real articles are published.

### 4. Run

In Development the app applies pending migrations by itself on startup (`Database:MigrateOnStartup` in `appsettings.Development.json`), so step 2 is only needed for production or for a manual check.

**Visual Studio Community 2026 (or 2022)**

1. Open `NewsArchiveSolution/NewsArchiveSolution.slnx`.
2. Choose the **http** profile in the toolbar (the **https** profile asks once to trust the local development certificate).
3. Press **F5**. The browser opens at `http://localhost:5096`.

**VS Code**

1. Open the repository folder (not the solution folder). Accept the recommended extensions (C# Dev Kit, C#, SQL Server).
2. Press **F5** and pick **NewsArchive (http)**. The `build` task runs first and the browser opens when the site is ready.
3. **Terminal → Run Task** offers `build`, `watch` (hot reload) and `database update`.

**Command line**

```bash
cd NewsArchiveSolution
dotnet run --project NewsArchive --launch-profile http
```

`/health` returns `Healthy` when the site can reach the database. If SQL Server is not reachable, the log explains which setting to check.

### 5. Try the API

Open `/swagger` while running in Development. Sign in with `POST /api/auth/login`, copy the `token`, press **Authorize** and paste it. From the command line:

```bash
curl -X POST http://localhost:5096/api/auth/login -H "Content-Type: application/json" \
     -d "{\"email\":\"admin@meridiannewsfolio.local\",\"password\":\"Meridian@2026\"}"
curl http://localhost:5096/api/articles/pending -H "Authorization: Bearer <token>"
```

The full endpoint list is in [ARCHITECTURE.md](ARCHITECTURE.md#rest-api).

## Configuration

| Setting | Purpose |
|---|---|
| `Site:Name`, `Site:Tagline`, `Site:Description` | Brand text used in titles, meta tags, feeds and the footer |
| `Site:BaseUrl` | Public address, used for canonical URLs, the sitemap and share images |
| `Site:TwitterHandle`, `Site:SocialProfiles`, `Site:ContactEmail` | Social links and contact details |
| `Site:ShowDemoNotice` | Shows the sample-content banner |
| `Api:SwaggerEnabled` | Serve Swagger UI outside Development (default `false`) |
| `Api:TokenLifetimeHours` | API token lifetime (default 12) |
| `Api:AllowedOrigins` | Other websites allowed to call the API from a browser (CORS) |
| `Hosting:KnownProxies` | IP addresses of a reverse proxy whose `X-Forwarded-*` headers can be trusted |
| `Database:MigrateOnStartup` | Apply pending migrations when the app starts (always on in Development) |

On another PC change `Data Source` in `ConnectionStrings:DbConn` to that machine's SQL Server instance, for example `localhost\SQLEXPRESS` or `(localdb)\MSSQLLocalDB`.

Uploaded pictures are saved in `NewsArchiveSolution/NewsArchive/wwwroot/uploads/` (not tracked by git). Back this folder up together with the database.

Before going live: change the sample passwords, keep `Api:SwaggerEnabled` off unless the API docs should be public, and serve the site over HTTPS (cookies are marked Secure outside Development).

## Sharing on LinkedIn and X

Social networks fetch the page from the public internet, so previews only work after the site is deployed (not on `localhost`).

1. Deploy the site and set `Site:BaseUrl`.
2. Check a URL with the [LinkedIn Post Inspector](https://www.linkedin.com/post-inspector/), which also refreshes LinkedIn's cache.
3. On X, paste the link into a new post to see the card preview.

Article pages use the article image as the thumbnail; other pages use `wwwroot/images/brand/og-default.png`.

## Continuous integration

`.github/workflows/build.yml` restores, builds in Release and lists vulnerable packages on every push and pull request to `develop` and `main`.

## Branching

- `develop` all ongoing work.
- `main` stable code, updated by reviewed merges from `develop`.

## License

No license has been chosen yet. All rights reserved by the author.
