# NewsArchive

A news publishing and archive web application built with ASP.NET Core MVC (.NET 10) and SQL Server, using a 3-tier architecture (Presentation → Business Logic → Data Access).

See [ARCHITECTURE.md](ARCHITECTURE.md) for the design, database schema, URL map and roadmap.

## Features

**Readers**
- Home page with a lead story, latest news, trending stories and a breaking-news ticker
- Section pages, trending and breaking news
- Search by keyword and date range, with pagination
- Article pages with byline, reading time, share buttons (LinkedIn, X, Facebook, WhatsApp), comments and related stories
- Sign up, sign in, profile, password change and password reset with a one-time code

**Authors**
- Write articles, save drafts and submit them for approval
- "My articles" page with the status of every article and the editor's notes on rejected ones

**Admins**
- Approve or reject submitted articles; nothing written by an author is published without approval
- Publish articles directly, manage sections, users and comments
- Dashboard with totals, the review queue, most-viewed articles and articles per section

**SEO and sharing**
- Clean URLs such as `/news/12/headline-of-the-story` with 301 redirects from old URLs
- Title, meta description, canonical URL and robots rules on every page
- Open Graph and Twitter Card tags, so links shared on LinkedIn, X and Facebook show a title, summary and thumbnail
- JSON-LD structured data (`NewsArticle`, `NewsMediaOrganization`, `WebSite`, `BreadcrumbList`)
- `robots.txt`, XML sitemap, Google News sitemap and RSS feed

## Tech stack

| Layer | Technology |
|---|---|
| Framework | ASP.NET Core MVC, .NET 10 |
| Data | Entity Framework Core 10 (migrations), SQL Server |
| Mapping | AutoMapper |
| UI | Razor views, Bootstrap 5, Bootstrap Icons, Source Serif 4 / Inter / Noto Bengali |

## Project structure

```
NewsArchiveSolution/
├── DAL/          EF Core DbContext, entities, migrations, repositories
├── BLL/          Services, DTOs, business rules, validation
└── NewsArchive/  MVC controllers, filters, views, SEO, wwwroot, Program.cs
NewsArchiveDB.bacpac   Original database export with sample data
```

## Getting started

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- SQL Server (Express or Developer edition)
- EF Core tools: `dotnet tool install --global dotnet-ef`

### 1. Create the database

**Option A: fresh database from migrations**

```bash
cd NewsArchiveSolution
dotnet ef database update --project DAL --startup-project NewsArchive
```

This creates every table, key, index and the seed data for roles and article statuses.

**Option B: start from the sample data**

1. Import `NewsArchiveDB.bacpac` (SSMS: right-click **Databases** → **Import Data-tier Application**).
2. Mark the baseline migration as applied, because the tables already exist:
   ```sql
   CREATE TABLE [__EFMigrationsHistory] ([MigrationId] nvarchar(150) NOT NULL PRIMARY KEY, [ProductVersion] nvarchar(32) NOT NULL);
   INSERT INTO [__EFMigrationsHistory] VALUES ('20261025140008_InitialSchema', '10.0.7');
   ```
3. Apply the remaining migrations:
   ```bash
   dotnet ef database update --project DAL --startup-project NewsArchive
   ```

### 2. Configure

In `NewsArchiveSolution/NewsArchive/appsettings.json`:

```json
"ConnectionStrings": {
  "DbConn": "Data Source=localhost\\SQLEXPRESS;Initial Catalog=NewsArchiveDB;Integrated Security=True;TrustServerCertificate=True;"
},
"Site": {
  "Name": "NewsArchive",
  "BaseUrl": "https://your-domain.com",
  "TwitterHandle": "@yourhandle",
  "ContactEmail": "newsroom@your-domain.com",
  "SocialProfiles": [ "https://www.linkedin.com/company/your-page" ]
}
```

- `ConnectionStrings__DbConn` can also be set as an environment variable.
- Set `Site:BaseUrl` to the public address after deployment. Canonical URLs, the sitemap and social share images use it.

### 3. Run

```bash
cd NewsArchiveSolution
dotnet run --project NewsArchive
```

Or open `NewsArchiveSolution/NewsArchiveSolution.slnx` in Visual Studio 2022+ and press F5.

### Roles

| RoleId | Role | Access |
|---|---|---|
| 1 | Admin | Full access; approves articles |
| 2 | Reader | Read and comment; default for new sign-ups |
| 3 | Author | Write and submit articles for approval |

An admin can change a user's role from **Account → Users**. In development the password-reset code is printed to the application console.

## Sharing on LinkedIn and X

Social networks fetch the page from the public internet, so previews only work after the site is deployed (not on `localhost`).

1. Deploy the site and set `Site:BaseUrl`.
2. Check a URL with the [LinkedIn Post Inspector](https://www.linkedin.com/post-inspector/). It also refreshes LinkedIn's cache after you change an image.
3. On X, paste the link into a new post to see the card preview.

Article pages use the article image as the thumbnail; other pages use `wwwroot/images/brand/og-default.png` (1200×630). For the best result use article images that are at least 1200×630.

## Branching

- `develop` all ongoing work.
- `main` stable code, updated by reviewed merges from `develop`.

## License

No license has been chosen yet. All rights reserved by the author.
