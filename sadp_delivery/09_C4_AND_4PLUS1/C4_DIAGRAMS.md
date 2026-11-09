# C4 Diagrams: The Meridian Newsfolio

These diagrams are kept as code (Mermaid), in the spirit of W12 s14. GitHub renders them. Each one states its level, title and key, and every arrow says *what* and *how* (W2 s24–25).

**For the midterm report (§4) use Level 1.** Level 2 is for the proposal or a viva question. Level 3 is for the Week 12 checkpoint (W12 s11: "three diagrams (C1–C3) is the checkpoint requirement").

**Rendered images:** not produced. No Mermaid renderer is installed, and nothing was installed. Paste the code into mermaid.live or view it on GitHub to export PNG or SVG.

## Level 1: System Context

Why this view (two lines for §4):
- It shows the whole Newsfolio as one box, with every kind of user and every outside system it depends on. Anyone can read it in a minute (W2 s23).
- It fixes the system boundary before any technology talk. That matters here, because three roles share one system and email is the only outside dependency.

```mermaid
flowchart LR
    reader([Reader / visitor<br/>Person: reads, searches, comments,<br/>saves, subscribes])
    author([Author<br/>Person: writes and submits articles])
    admin([Admin<br/>Person: reviews, schedules, moderates,<br/>sends the digest])
    apiclient([API client<br/>External app using the REST API])
    system[["The Meridian Newsfolio<br/>Software system: bilingual EN/BN news archive<br/>with editorial review, search, recommendations,<br/>moderation and newsletter"]]
    smtp[(SMTP mail provider<br/>External system, optional:<br/>delivers confirmation, digest<br/>and reset-code emails)]
    crawlers([Search engines and feed readers<br/>External systems: read sitemaps,<br/>RSS and link previews])
    cdn[(Font and icon CDN<br/>External system: Google Fonts,<br/>jsDelivr Bootstrap Icons)]

    reader -- "Reads and searches news, comments<br/>[HTTPS, web browser]" --> system
    author -- "Writes and submits articles<br/>[HTTPS, web browser]" --> system
    admin -- "Reviews, schedules, moderates<br/>[HTTPS, web browser]" --> system
    apiclient -- "Calls JSON endpoints<br/>[HTTPS/JSON, bearer token]" --> system
    system -- "Sends emails<br/>[SMTP, optional]" --> smtp
    crawlers -- "Fetch sitemap, RSS, pages<br/>[HTTPS/XML]" --> system
    reader -. "Browser loads fonts and icons<br/>[HTTPS]" .-> cdn
```

Key:
- `([...])` = person or outside actor.
- `[[...]]` = the system in scope.
- `[(...)]` = external system.
- A dotted arrow means the browser, not the server, makes the call.

## Level 2: Containers

```mermaid
flowchart TB
    subgraph users[People]
        p1([Reader / Author / Admin])
        p2([API client])
    end
    subgraph newsfolio[The Meridian Newsfolio, software system boundary]
        web["Web application<br/>[Container: ASP.NET Core 10 on Kestrel, one process]<br/>MVC + Razor pages, REST API + Swagger,<br/>BLL and DAL assemblies, background jobs<br/>(ScheduledPublisher, DigestSender)"]
        front["Browser front end<br/>[Container: Razor HTML + site.js + service worker sw.js]<br/>theme, search suggestions, Markdown preview,<br/>offline page"]
        db[("Database<br/>[Container: SQL Server; Express in dev]<br/>articles, users, comments, history,<br/>subscribers, audit log, word index")]
        files[("Uploaded pictures<br/>[Container: file system, wwwroot/uploads]")]
        outbox[("Development outbox<br/>[Container: file system, App_Data/outbox]<br/>Development only")]
    end
    smtp[(SMTP mail provider<br/>External, optional)]

    p1 -- "Uses<br/>[HTTPS]" --> front
    front -- "Page requests, form posts, fetch calls<br/>[HTTPS, HTML/JSON, antiforgery token]" --> web
    p2 -- "JSON calls<br/>[HTTPS, bearer token]" --> web
    web -- "Reads and writes<br/>[EF Core 10 over TDS/TCP 1433]" --> db
    web -- "Saves and deletes pictures<br/>[file I/O]" --> files
    web -- "Writes email copies<br/>[file I/O]" --> outbox
    web -- "Sends email<br/>[SMTP + TLS]" --> smtp
```

Notes that keep this diagram honest:
- **UI, BLL and DAL are not separate containers.** They are three assemblies loaded into one process, so they appear inside "Web application" and in Level 3.
- **The MVC website and the REST API are the same deployable,** so they are one container.
- **SQL Server** is a separate process, and in production normally a separate host.

## Level 3: Components inside the web application (Week 12 checkpoint)

```mermaid
flowchart TB
    subgraph web[Web application container]
        mvc["MVC controllers<br/>[Component: 17 classes]<br/>Article, Admin, Auth, Newsletter…"]
        api["API controllers<br/>[Component: 13 classes, /api]"]
        filters["Auth and filters<br/>[Component]<br/>cookie auth + CookieSessionValidator,<br/>TokenAuthorizationFilter, CustomAuthorize,<br/>rate limiter, antiforgery, CSP headers"]
        svc["Business services<br/>[Component: BLL, 17 services + rules]<br/>ArticleService + ArticleBuilder,<br/>RecommendationService, NewsletterService…"]
        jobs["Background jobs<br/>[Component: hosted services]<br/>ScheduledPublisher (every 30 s),<br/>DigestSender (queue)"]
        mail["Email delivery<br/>[Component]<br/>EmailSender → Smtp / Outbox transport,<br/>MailComposer (EN/BN)"]
        infra["Infrastructure adapters<br/>[Component]<br/>MemoryAppCache, WebRootFileStorage,<br/>HttpActorContext"]
        dal["Data access<br/>[Component: DAL]<br/>IDataAccessFactory → 11 repositories,<br/>NewsArchiveDbContext"]
    end
    db[(SQL Server)]
    mvc --> filters
    api --> filters
    mvc -- "calls" --> svc
    api -- "calls" --> svc
    mvc -- "queues digest" --> jobs
    jobs -- "calls" --> svc
    jobs -- "sends" --> mail
    svc -- "uses abstractions owned by the BLL:<br/>IEmailSender, IAppCache, IFileStorage, IActorContext" --> infra
    svc -- "creates repositories through<br/>IDataAccessFactory" --> dal
    dal -- "EF Core" --> db
```
