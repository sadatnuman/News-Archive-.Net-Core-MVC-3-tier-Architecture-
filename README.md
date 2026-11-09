# The Meridian Newsfolio

**News Preserved. Stories Rediscovered.** · **সংবাদ সংরক্ষিত, গল্পের নতুন আবিষ্কার।**

A bilingual (English and Bangla) news archive built with ASP.NET Core MVC on .NET 10, Entity Framework Core and SQL Server. It uses a strict three-layer design (UI → BLL → DAL). Readers get recommendations as they read, authors write in Markdown, admins review, schedule and moderate, and a token-protected REST API exposes it all.

> How it is built (layers, tables, every endpoint, the security model): **[ARCHITECTURE.md](ARCHITECTURE.md)**

---

## Features

### For readers

- English and Bangla editions; every page, form, message and email is translated
- Light, dark and system themes, with a light 3D layer (rotating globe, tilt cards) that switches off for reduced motion
- Home page with lead story, latest news, trending, popular topics, *On this day* and a breaking-news ticker
- Sections, topics, date archive with a calendar, author pages, fast word search in English and Bangla, and search by date
- **Search suggestions** while typing: stories, topics, sections and authors
- **Recommendations while reading**:
  - an *Up next* story and *More like this*, each with the reason (same topic, similar story, same section or author)
  - a *Read next* pop-up and a reading progress bar
  - *Recommended for you* on the home page and in the article sidebar
- **Reading history** (`/history`) with remove and clear, and **saved stories** (`/bookmarks`)
- Comments, with a **Report** button for spam, abuse, off-topic or misinformation
- **Weekly email digest** with double opt-in, one-click unsubscribe and a bot trap (written to a local outbox only; see "Email" below)
- Works **offline**: a bilingual offline page, and repeat visits load faster
- Sign up, profile picture, bio, password change, and password reset by a code (delivered to the local outbox only); eye button on every password field

### For authors

- Markdown editor with toolbar and live preview, picture upload, up to 8 topics per story
- Drafts and submission for review, with the editor's note on returned stories

### For admins

- Approve, reject or **schedule** stories; nothing an author writes goes live without approval
- **Comment moderation**: reported and hidden queues; a comment is hidden automatically after 3 reports
- **Newsletter**: subscriber list, digest preview and *Send now* for each edition, sent in the background and never twice in one week
- **Audit log**: every editorial, moderation, account and sign-in action, filterable by type and person
- Sections, topics and users (with roles); dashboard with totals, review queue, reported comments and most-viewed stories

### REST API

- JSON API under `/api` for articles, sections, topics, authors, archive, comments, bookmarks, history, recommendations, search suggestions, moderation and the audit log
- Bearer tokens for apps, or an HttpOnly cookie for browser clients; role checks, expiry, logout and *log out everywhere*
- Interactive docs at `/swagger`

### Security

- **Passwords and lockout:**
  - PBKDF2 password hashing; old MD5 hashes are upgraded automatically.
  - 5 failed sign-ins from one IP address lock that email there for 15 minutes.
  - Every endpoint that accepts input is rate limited.
- **Account protection:**
  - Changing your password requires the current one.
  - Reset codes are hashed, expire, allow limited attempts and are limited per hour.
  - Sign-out needs a POST with an antiforgery token.
  - A changed password, role or deleted account ends existing sessions.
- **Browser protection:**
  - CSRF protection.
  - Strict Content Security Policy (no inline scripts).
  - Safe Markdown: no raw HTML, and no off-site tricks such as `//` or `/\` links.
  - Security headers and safe redirects.

### SEO

- Clean bilingual URLs with 301 redirects
- Canonical, `hreflang`, Open Graph and Twitter cards
- JSON-LD (`NewsArticle`, `BreadcrumbList`, `ProfilePage` and more)
- Sitemaps (including Google News) and RSS for each language

---

## Tech stack

| Layer | Technology |
|---|---|
| Framework | ASP.NET Core MVC, .NET 10 (`global.json`) |
| Data | Entity Framework Core 10.0.14 with migrations, SQL Server |
| Mapping | AutoMapper 14 through dependency injection (`MaxDepth` safeguard) |
| Content | Markdig (safe Markdown) |
| API docs | Swashbuckle / Swagger |
| UI | Razor, Bootstrap 5.3, Bootstrap Icons, Source Serif 4, Inter, Cinzel, Noto Bengali |
| Tests | xUnit (137 unit and integration tests), a smoke-test and benchmark script, GitHub Actions |

## Project structure

```
NewsArchiveSolution/
├── DAL/                EF Core DbContext, entities, migrations, repository interfaces, DataAccessFactory
├── BLL/                Services, rules, DTOs, AutoMapper profile, security, audit, email and cache contracts
├── NewsArchive/        MVC and API controllers, filters, views, email, SEO, localization, wwwroot, Program.cs
└── NewsArchive.Tests/  Unit tests for rules and services
database/
└── sample-data.sql     Bilingual sample content that fills every table
```

---

## Getting started

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- SQL Server (Express, Developer or LocalDB)
- EF Core tools: `dotnet tool install --global dotnet-ef`

### 1. Connection string

In `NewsArchiveSolution/NewsArchive/appsettings.json` set `ConnectionStrings:DbConn`, for example:

```json
"DbConn": "Data Source=localhost\\SQLEXPRESS;Initial Catalog=NewsArchiveDB;Integrated Security=True;TrustServerCertificate=True;"
```

The environment variable `ConnectionStrings__DbConn` works too.

### 2. Database

In Development the app applies migrations by itself on startup. To do it by hand:

```bash
cd NewsArchiveSolution
dotnet ef database update --project DAL --startup-project NewsArchive
```

### 3. Sample content (optional)

```bash
sqlcmd -S "localhost\SQLEXPRESS" -E -d NewsArchiveDB -f 65001 -i database/sample-data.sql
```

`-f 65001` keeps Bangla text intact. In SSMS, open the file, choose **NewsArchiveDB** and press **Execute**. The script replaces all content:

| Data | Rows |
|---|---|
| Users with profile pictures and bios | 6 |
| Sections and topics in English and Bangla | 11 sections, 20 topics |
| Articles (15 English, 13 Bangla) with pictures and review history | 28: 22 published, 2 scheduled three days ahead, 2 pending, 1 draft, 1 rejected |
| Comments, comment reports, topic links | 12, 3, 46 |
| Bookmarks, reading history | 14, 30 |
| Newsletter subscribers | 6 (5 active, 1 unsubscribed) |
| Audit log entries | 14 |
| Expired and revoked API tokens (cannot be used) | 6 |

Sample accounts all use the password `Meridian@2026`. In Development, the **Demo Admin**, **Demo Author** and **Demo Reader** buttons on the sign-in page fill them in with one click. They come from `appsettings.Development.json`, so a production deployment never shows them. Change the passwords before going live.

| Email | Role |
|---|---|
| `admin@meridiannewsfolio.local` | Admin |
| `farhana@meridiannewsfolio.local`, `tanvir@meridiannewsfolio.local` | Author |
| `nusrat@meridiannewsfolio.local`, `imran@meridiannewsfolio.local`, `sadia@meridiannewsfolio.local` | Reader |

The stories, people and pictures are fictional. While `Site:ShowDemoNotice` is `true`, a banner says so.

### 4. Run

**Visual Studio 2026 / 2022:** open `NewsArchiveSolution/NewsArchiveSolution.slnx`, pick the **http** profile and press **F5**. The site opens at `http://localhost:5096`.

**VS Code:** open the repository folder and accept the recommended extensions. Press **F5** and pick **NewsArchive (http)**. The `build`, `watch`, `test` and `database update` tasks are under **Terminal → Run Task**.

**Command line**

```bash
cd NewsArchiveSolution
dotnet run --project NewsArchive --launch-profile http
```

`/health` returns `Healthy` when the database is reachable.

### 5. Tests

```bash
cd NewsArchiveSolution
dotnet test NewsArchive.Tests
```

The integration tests start the app against a separate database, `NewsArchiveTestDB`, which they create on `localhost\SQLEXPRESS`. Your own database is never touched. To use another server, set the environment variable `NEWSARCHIVE_TEST_DB` to a connection string. CI runs them against a SQL Server container.

**End-to-end smoke test and search benchmark** (Python 3 standard library, `sqlcmd`):

```bash
python tools/smoke/smoke.py                  # recreate NewsArchiveSmokeDB, load the sample data, run every check
python tools/smoke/smoke.py --stories 5000   # also add 5,000 generated stories and measure search p50/p95/p99
```

The script only accepts a database named `NewsArchiveSmokeDB…` or `NewsArchiveTestDB…` on a local SQL Server (`localhost`, `.`, `127.0.0.1` or `(localdb)`), so it can never reset `NewsArchiveDB`. It reads the demo passwords from `appsettings.Development.json`, removes any `Email__…`/`SMTP…` environment variables, runs the app on `127.0.0.1:5199` in Development (emails only go to the private local outbox, which it cleans afterwards) and stops it at the end.

### 6. API

Open `/swagger` in Development. Sign in with `POST /api/auth/login`, copy the token, press **Authorize** and paste it.

```bash
curl -X POST http://localhost:5096/api/auth/login -H "Content-Type: application/json" \
     -d "{\"email\":\"admin@meridiannewsfolio.local\",\"password\":\"Meridian@2026\"}"
curl http://localhost:5096/api/articles/pending -H "Authorization: Bearer <token>"
curl "http://localhost:5096/api/search/suggest?q=rail&lang=en"
```

**Editing an article (`PUT /api/articles/{id}`) needs a version.** First `GET /api/articles/{id}`; the response has a `version` field (for example `"3-639005184000000000"`). Send the full article plus that `version`:

```bash
curl -X PUT http://localhost:5096/api/articles/42 -H "Authorization: Bearer <token>" -H "Content-Type: application/json" \
     -d "{\"title\":\"...\",\"summary\":\"...\",\"content\":\"...\",\"categoryId\":1,\"languageCode\":\"en\",\"version\":\"3-639005184000000000\"}"
```

| Answer | Meaning |
|---|---|
| 204 | Saved |
| 400 | `version` missing or malformed: send the value from `GET /api/articles/{id}` |
| 409 | Someone saved or decided the story after you loaded it: load it again |
| 401 / 403 / 404 | Not signed in / not allowed to edit / no such article |

Clients written before this rule existed must add the `version` field; without it they get 400, never a false "someone else changed it".

---

## Configuration

| Setting | Purpose |
|---|---|
| `Site:Name`, `Site:Tagline`, `Site:Description` | Brand text for titles, meta tags, feeds and emails |
| `Site:BaseUrl` | Public address for canonical URLs, sitemaps, share images and email links |
| `Site:SocialProfiles`, `Site:TwitterHandle`, `Site:ContactEmail` | Social links (shown as footer icons) and contact details |
| `Site:TimeZone` | Time zone for every stored and shown time (default `Asia/Dhaka`), independent of the server clock |
| `Site:ShowDemoNotice` | Sample-content banner (on only in Development) |
| `Site:DemoAccounts` | Demo Admin, Author and Reader buttons on the sign-in page (set only in `appsettings.Development.json`) |
| `Api:SwaggerEnabled`, `Api:TokenLifetimeHours`, `Api:AllowedOrigins` | Swagger outside Development, token lifetime (default 12 h), CORS origins |
| `Email:FromAddress`, `Email:FromName` | Sender of all emails |
| `Email:OutboxFolder` | Where Development writes emails as text files (default `App_Data/outbox` under the content root, not under `wwwroot`, not in git) |
| `Hosting:KnownProxies` | Reverse proxies whose `X-Forwarded-*` headers are trusted |
| `Hosting:TrustAllProxies` | Set `true` on Azure App Service and other hosts that put a proxy in front of the app, so every visitor gets their own rate limit |
| `Database:MigrateOnStartup` | Apply migrations on startup outside Development |

### Email

**Real email delivery is not implemented/configured or tested. Email verification uses a local outbox/fake transport only.**

| Environment | Sender | What happens |
|---|---|---|
| Development | `OutboxEmailSender` | Each email is written as a text file to `App_Data/outbox` (private, not served, not in git). Use it to confirm a newsletter sign-up or read a reset code while testing. |
| Anything else | `DisabledEmailSender` | Nothing is sent and no connection is opened. `SendAsync` returns `false`. One warning is logged at start-up; no log line per email. The admin "Send digest" button says that email delivery is turned off. |

`SmtpEmailSender` stays in the code as the second product of the Factory Method example, but the app never registers it, even if an `Email:Smtp:Host` is configured. Because of this, on a public deployment readers cannot confirm a newsletter sign-up or receive a password-reset code. Turning on real email later needs a code change, a provider, and its own tests.

### Before going live

- Change the sample passwords. Demo buttons and the sample banner are already off outside Development.
- Set `Site:BaseUrl`.
- Behind a host proxy (Azure App Service, most free hosts), set `Hosting:TrustAllProxies` to `true`.
- Serve the site over HTTPS (cookies are Secure outside Development).
- Real email is off by design (see "Email"); newsletter confirmation and password reset by email do not work on a public host until it is added.
- Keep `Api:SwaggerEnabled` off unless the API docs should be public.
- Back up the database together with `wwwroot/uploads`.

## Sharing on LinkedIn and X

Previews need the public site, not `localhost`. Deploy the site and set `Site:BaseUrl`, then check a link with the [LinkedIn Post Inspector](https://www.linkedin.com/post-inspector/). Article pages use the story picture; other pages use `wwwroot/images/brand/og-default.png`.

## Continuous integration

`.github/workflows/build.yml` restores, builds in Release, runs the unit tests and reports vulnerable packages on every push and pull request to `develop` and `main`.

## Branching

- `develop`: all ongoing work
- `main`: stable code, updated by reviewed merges from `develop`

## License

No license has been chosen yet. All rights reserved by the author.
