# C4 Diagrams: The Meridian Newsfolio

**Where each level is used:**

| Level | Used in | Rendered file |
|---|---|---|
| Level 1 | Midterm §4 (Figure 1) | `c4_level1_context.png` (and `.svg`) |
| Level 2 | Project Proposal §8 | `c4_level2_containers.png` (and `.svg`) |
| Level 3 | Week 12 checkpoint only | Mermaid below (not exported) |

**How the images are made.**
- `render_c4.py` writes the SVG files with the Python standard library.
- It then makes the PNG files with the local Chrome (or Edge) in headless mode, at 2× scale.
- Nothing was installed for this. To rebuild them: `python sadp_delivery/09_C4_AND_4PLUS1/render_c4.py`.
- Both PNG files were opened and checked by eye: every box, arrow, label and the key are readable, and nothing overlaps.

The Mermaid sources below match the images. They are kept as text so the diagrams can be reviewed in a pull request.

**Notation (C4):**
- Every element shows its name, its type in brackets, and a short description.
- Every arrow says *what* happens and, in brackets, *how* (protocol or technology).
- A dotted arrow is a call made by the reader's browser, not by our server.

## Review against C4 rules

| Check | Level 1 | Level 2 |
|---|---|---|
| Scope stated | The whole software system | Containers inside the system boundary |
| System boundary shown | The system is one box | Dashed boundary around 5 containers |
| Element types labelled | Person / Software System / External System | Person / Container / External System |
| Technology shown | Only on arrows (L1 has no technology inside the system) | On every container and on every arrow |
| Arrows labelled with intent and protocol | Yes (6) | Yes (9) |
| Key | Yes | Yes |

**Corrections made in this revision:**
- **Level 2, browser scripts.** This container is now *delivered by* the web application. Before, it was drawn as if it were deployed on its own. It is a real container because its code runs in a different place: the reader's browser, including the service worker.
- **Level 2, CDN.** The font and icon CDN was added, because the browser scripts call it.
- **SMTP removed (final hardening).** The app never connects to a mail provider (ADR-007), so the SMTP external system and its arrow were removed from L1 and L2. A note on each figure says that real email is not implemented and that Development writes to a private outbox.
- **Level 3.** The auth filters are drawn as running *before* the controllers. Before, they were drawn as something the controllers call. Every arrow now has a label.
- **Not containers:** UI, BLL and DAL are three assemblies loaded into one process. The MVC site and the REST API are the same deployable. So they are one container, and the layers appear only at Level 3.

## Level 1: System Context

**Why this view (two lines for midterm §4):**
- It shows the whole Newsfolio as one box, with every kind of user and every outside system it depends on. Anyone can read it in a minute [6], [9, L2].
- It fixes the system boundary before any technology talk. That matters here, because three roles share one system and our server calls no outside service: fonts load in the browser, and real email is not implemented.

```mermaid
flowchart LR
    reader([Reader<br/>Person: reads, searches, comments,<br/>bookmarks, subscribes])
    author([Author<br/>Person: writes stories and submits them for review])
    admin([Admin<br/>Person: reviews, schedules, moderates,<br/>sends the digest])
    apiclient[API client<br/>External system: app or script using the REST API]
    system[["The Meridian Newsfolio<br/>Software system: bilingual EN/BN news archive<br/>with editorial review, word search, recommendations,<br/>comment moderation and newsletter"]]
    crawlers[Search engines and feed readers<br/>External systems]
    cdn[Font and icon CDN<br/>External system: Google Fonts, jsDelivr]

    reader -- "Reads and searches news, comments, subscribes<br/>[HTTPS, web browser]" --> system
    author -- "Writes and submits stories<br/>[HTTPS, web browser]" --> system
    admin -- "Reviews, schedules, moderates<br/>[HTTPS, web browser]" --> system
    apiclient -- "Calls JSON endpoints<br/>[HTTPS/JSON, bearer token]" --> system
    crawlers -- "Fetch pages, sitemap, RSS<br/>[HTTPS, XML]" --> system
    reader -. "Browser loads fonts and icons<br/>[HTTPS]" .-> cdn
```

## Level 2: Containers

**Why this view (for the proposal):** it shows which parts run separately and with what technology. It also makes clear that the three layers are *not* three tiers: they run in one process.

```mermaid
flowchart TB
    people([Reader / Author / Admin<br/>Person])
    apiclient[API client<br/>External system]
    crawlers[Search engines and feed readers<br/>External systems]
    cdn[Font and icon CDN<br/>External system]
    subgraph newsfolio[The Meridian Newsfolio: software system boundary]
        browser["Browser scripts<br/>[Container: JavaScript + service worker, runs in the browser]<br/>theme, search suggestions, Markdown preview, offline page"]
        web["Web application<br/>[Container: ASP.NET Core 10 MVC + Web API on Kestrel]<br/>UI, BLL and DAL in one process; Razor pages, JSON API, Swagger,<br/>background publisher and digest sender"]
        db[("Database<br/>[Container: SQL Server, EF Core migrations]<br/>articles, users, comments, word index, audit log")]
        files[("Uploaded pictures<br/>[Container: file system, wwwroot/uploads]")]
        outbox[("Mail outbox<br/>[Container: private file folder, App_Data/outbox, Development only]")]
    end

    people -- "Visits pages, posts forms<br/>[HTTPS]" --> web
    apiclient -- "Calls REST API<br/>[HTTPS/JSON, bearer token]" --> web
    crawlers -- "Reads sitemap, RSS, pages<br/>[HTTPS/XML]" --> web
    web -- "Delivers HTML, CSS, JS<br/>[HTTPS]" --> browser
    browser -- "JSON fetch calls<br/>[HTTPS, antiforgery token]" --> web
    browser -. "Loads fonts and icons<br/>[HTTPS]" .-> cdn
    web -- "Reads and writes<br/>[EF Core 10, TDS over TCP 1433]" --> db
    web -- "Saves and deletes pictures<br/>[file I/O]" --> files
    web -- "Writes email copies<br/>[file I/O]" --> outbox
```

## Level 3: Components inside the web application (Week 12 checkpoint)

```mermaid
flowchart TB
    subgraph web[Web application container]
        filters["Request pipeline and filters<br/>[Component: ASP.NET Core middleware + MVC filters]<br/>cookie auth + CookieSessionValidator, TokenAuthorizationFilter,<br/>CustomAuthorize, rate limiter, antiforgery, security headers"]
        mvc["MVC controllers<br/>[Component: 16 controllers + AppController base]<br/>Article, Admin, Auth, Archive, Newsletter…"]
        api["API controllers<br/>[Component: 12 controllers + ApiControllerBase, /api]"]
        svc["Business services<br/>[Component: BLL, 17 services + rules]<br/>ArticleService + ArticleBuilder, RecommendationService, NewsletterService…"]
        jobs["Background jobs<br/>[Component: .NET hosted services]<br/>ScheduledPublisher (every 30 s), MaintenanceJob (every 24 h), DigestSender (queue)"]
        mail["Email delivery<br/>[Component: Factory Method]<br/>Development: OutboxEmailSender; elsewhere: DisabledEmailSender; SmtpEmailSender dormant; MailComposer (EN/BN)"]
        infra["Infrastructure adapters<br/>[Component]<br/>MemoryAppCache, WebRootFileStorage, HttpActorContext"]
        dal["Data access<br/>[Component: DAL, EF Core 10]<br/>IDataAccessFactory → 11 repositories, NewsArchiveDbContext"]
    end
    db[(SQL Server)]
    filters -- "Lets authorised requests through to" --> mvc
    filters -- "Lets authorised requests through to" --> api
    mvc -- "Calls service interfaces" --> svc
    api -- "Calls service interfaces" --> svc
    mvc -- "Queues the digest" --> jobs
    mvc -- "Sends confirmation and reset mail" --> mail
    jobs -- "Publishes due stories, indexes words" --> svc
    jobs -- "Sends the digest" --> mail
    svc -- "Uses BLL-owned interfaces:<br/>IAppCache, IFileStorage, IActorContext" --> infra
    svc -- "Creates repositories through<br/>IDataAccessFactory" --> dal
    dal -- "SQL [EF Core 10, TDS]" --> db
```
