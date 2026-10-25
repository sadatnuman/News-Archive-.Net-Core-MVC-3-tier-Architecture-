# The Meridian Newsfolio

*News Preserved. Stories Rediscovered.* · *সংবাদ সংরক্ষিত, গল্পের নতুন আবিষ্কার।*

A bilingual (English and Bangla) news publishing and archive web application built with ASP.NET Core MVC (.NET 10) and SQL Server, using a 3-tier architecture (Presentation → Business Logic → Data Access).

See [ARCHITECTURE.md](ARCHITECTURE.md) for the design, database schema, URL map and roadmap.

## Features

**Readers**
- English and Bangla editions with a language switcher; every page, form and message is translated
- Light, dark and system themes
- Home page with a lead story, latest news, trending stories and a breaking-news ticker
- Sections, trending and breaking news, search by keyword and date range
- Article pages with byline, reading time, share buttons (LinkedIn, X, Facebook, WhatsApp), comments and related stories
- Sign up, sign in, profile, password change and password reset with a one-time code

**Authors**
- Write articles in English or Bangla, save drafts and submit them for approval
- "My articles" page with the status of every article and the editor's notes on rejected ones

**Admins**
- Approve or reject submitted articles; nothing written by an author is published without approval
- Add, rename and delete any number of sections, with English and Bangla names; articles are moved when a section is deleted
- Manage users and roles, and moderate comments
- Dashboard with totals, the review queue, most-viewed articles and articles per section

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
| Framework | ASP.NET Core MVC, .NET 10 |
| Data | Entity Framework Core 10 (migrations), SQL Server |
| Mapping | AutoMapper |
| Localization | ASP.NET Core localization, resource files, culture-specific views |
| UI | Razor views, Bootstrap 5.3, Bootstrap Icons, Cinzel / Source Serif 4 / Inter / Noto Bengali |

## Project structure

```
NewsArchiveSolution/
├── DAL/          EF Core DbContext, entities, migrations, repositories
├── BLL/          Services, DTOs, business rules, validation
└── NewsArchive/  MVC controllers, filters, views, localization, SEO, wwwroot, Program.cs
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

```bash
dotnet run --project NewsArchive
```

Or open `NewsArchiveSolution/NewsArchiveSolution.slnx` in Visual Studio 2022+ and press F5.

## Configuration

| Setting | Purpose |
|---|---|
| `Site:Name`, `Site:Tagline`, `Site:Description` | Brand text used in titles, meta tags, feeds and the footer |
| `Site:BaseUrl` | Public address, used for canonical URLs, the sitemap and share images |
| `Site:TwitterHandle`, `Site:SocialProfiles`, `Site:ContactEmail` | Social links and contact details |
| `Site:ShowDemoNotice` | Shows the sample-content banner |

## Sharing on LinkedIn and X

Social networks fetch the page from the public internet, so previews only work after the site is deployed (not on `localhost`).

1. Deploy the site and set `Site:BaseUrl`.
2. Check a URL with the [LinkedIn Post Inspector](https://www.linkedin.com/post-inspector/), which also refreshes LinkedIn's cache.
3. On X, paste the link into a new post to see the card preview.

Article pages use the article image as the thumbnail; other pages use `wwwroot/images/brand/og-default.png`.

## Branching

- `develop` all ongoing work.
- `main` stable code, updated by reviewed merges from `develop`.

## License

No license has been chosen yet. All rights reserved by the author.
