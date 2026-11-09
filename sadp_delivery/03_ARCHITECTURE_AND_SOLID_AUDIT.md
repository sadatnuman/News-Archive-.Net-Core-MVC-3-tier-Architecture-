# 03 · Architecture and SOLID Audit

Lecture anchors: W2 s6–8 (layers and violations), s11–17 (MVC, fat controller, composition root); W1 s10–25 (SOLID).

## 1. Dependency direction: checked, not assumed

| Rule (W2 s7) | Check run | Result |
|---|---|---|
| Presentation must not reach the DAL | `grep -rln "using DAL\|DAL.(EF\|Repos\|Interfaces)" NewsArchive` | No match. The compiler also blocks it: `BLL/BLL.csproj:14` uses `PrivateAssets="compile"`. |
| The DAL must not depend on the BLL | `grep -rln "using BLL" DAL` | No match |
| The business layer must not import HTTP | `grep -rn "Microsoft.AspNetCore\|HttpContext" BLL` | No match |
| No controller uses the DbContext | `grep -rln DbContext NewsArchive` | No match |
| One composition root (W2 s17) | `NewsArchive/Program.cs` plus `BLL/BusinessLayerServices.cs:11` and `DAL/DataLayerServices.cs:12–13` | Present |

The real call chain is Controller → Service (BLL) → `IDataAccessFactory` → Repository → `NewsArchiveDbContext` → SQL Server.

The BLL owns four abstractions that the UI implements:

| Abstraction (BLL) | Implementation (UI) |
|---|---|
| `IEmailSender` (`BLL/Email/IEmailSender.cs:3`) | `EmailSender` subclasses |
| `IFileStorage` (`BLL/Media/IFileStorage.cs:3`) | `WebRootFileStorage` |
| `IActorContext` (`BLL/Audit/IActorContext.cs:3`) | `HttpActorContext` |
| `IAppCache` (`BLL/Caching/IAppCache.cs:3`) | `MemoryAppCache` |

So the business layer never depends on infrastructure. This is DIP across layers.

**Layered is not N physical tiers.** UI, BLL and DAL are three assemblies loaded into **one process**. Only SQL Server is a separate process. The diagrams in `09` show one web application container and one database container.

## 2. MVC (W2 s11–19)

| Part | Where |
|---|---|
| Model | DTOs plus services and rules in the BLL (`BLL/DTOs`, `BLL/Services`, `BLL/Rules`) |
| View | Razor views (`NewsArchive/Views`) for the website; JSON for the API (W2 s19: "the JSON response IS the View") |
| Controller | 17 MVC and 13 API controllers. They parse input, call one or two services, and choose a view or status code. |

The fat-controller check found two weak spots.

- **`AdminController` has 12 constructor dependencies** (`NewsArchive/Controllers/AdminController.cs:33`).
  - It covers dashboard, review, comments, users, audit and newsletter in one class. That is an SRP smell at controller level.
  - It holds no business rules or SQL, so it is not a fat controller in the W2 s15 sense.
  - Recorded as P2 in `15`: split it per admin area.
- **`ArticleController.Details` is 55 lines** (`ArticleController.cs:132`).
  - It assembles view data from several services: comments, recommendations, bookmarks, lookups.
  - This is presentation work, not business logic. It is acceptable, and a later facade candidate (W4 s16).

## 3. SOLID, with real examples and violations

| Principle | Good example in this code | Evidence | Test | Violation or smell found |
|---|---|---|---|---|
| **SRP** | Article construction (`ArticleBuilder`), workflow and audit (`ArticleService`) and policy (`ArticleRules`) are three classes with three reasons to change. Email content (`MailComposer`) is separate from email delivery (`EmailSender` and its transports). | `BLL/Builders/ArticleBuilder.cs:76`, `BLL/Services/ArticleService.cs:60`, `BLL/Rules/ArticleRules.cs:5`, `NewsArchive/Email/EmailSender.cs:22` | `ArticleBuilderTests` (7) | `AdminController` covers six admin areas (P2). |
| **OCP** | A new email channel (for example an HTTP mail API) is a new `EmailSender` subclass and transport. `NewsletterController`, `AuthController`, `DigestSender` and `NewsletterService` keep depending on `IEmailSender` and do not change. | `NewsArchive/Email/EmailSender.cs:20` (factory method), `Program.cs:112–114` (config picks the creator) | `EmailSenderTests` (5) | Recommendation scoring is one hard-coded formula (`RecommendationRules`). A new ranking needs an edit, not an extension. That is acceptable today and a W5 Strategy candidate. |
| **LSP** | Every `IEmailSender` keeps the same contract: `true` when the message was accepted, `false` (never an exception) for a bad address or a failed delivery. `FakeFactory` replaces `DataAccessFactory` in every service test without changing behaviour. | `NewsArchive/Email/EmailSender.cs:22–47`; `NewsArchive.Tests/Support/TestServices.cs:52` | `EmailSenderTests.Every_sender_honours_the_same_contract` (runs the same assertions on two different senders) | None found. No override throws `NotImplementedException`. |
| **ISP** | `RecommendationService` depends only on `IArticleRepository` and `IReadingHistoryRepository`. `IAppCache` has two methods. `IActorContext` has two properties. | `BLL/Services/RecommendationService.cs:17`, `BLL/Caching/IAppCache.cs:3` | `RecommendationServiceTests` | **Violation:** `IArticleService` has about 30 members and is used by many controllers that call 2–3 of them. **Smell:** `IDataAccessFactory` hands any service all 11 repositories. Both are recorded in `15` (P2): split by reader and editor use. |
| **DIP** | Services receive `IDataAccessFactory`, `IAuditService`, `IAppCache` and `IActorContext` through constructors. `PasswordChanger` depends on `IAuthService`, so tests inject a fake. | `BLL/Services/ArticleService.cs:19`, `NewsArchive/Auth/PasswordChanger.cs:15` | `SecurityTests.Password_change_needs_the_current_password` (fake `IAuthService`) | `SiteClock` is a static clock (`BLL/SiteClock.cs:9`). Tests cannot freeze time; they work around it with relative times. Recorded P2: inject `TimeProvider`. |

## 4. Other architecture defects checked

| Check | Result |
|---|---|
| SQL injection | Every query goes through EF Core LINQ or `ExecuteUpdate` with parameters. Search terms reach SQL as `LIKE @p` through `StartsWith`. No string-built SQL was found (`grep -rn "FromSqlRaw\|ExecuteSqlRaw"` → none). |
| N+1 queries | List pages use one projection (`ArticleRepo.Cards`, `DAL/Repos/ArticleRepo.cs:20`) with tags in the same query. Lookups for authors, avatars and sections are batched (`ArticleLookupService`). |
| Race conditions | Approve/reject (fixed: concurrency token). Scheduled publishing is a single atomic `UPDATE … WHERE StatusId=Scheduled AND ScheduledAt<=now` (`ArticleRepo.cs:259`), so two app instances cannot publish twice. A reader can report a comment only once: the primary key is `(CommentId, UserId)` (`DAL/EF/NewsArchiveDbContext.cs:358`), and `CommentRepo.AddReport` catches the duplicate-key error. |
| Unpublished leaks | `ArticleRules.CanView` is used by the website and the API. Smoke and integration tests confirm 404 before approval. |
| Lifetimes | `DbContext` and services are scoped. `LoginThrottle`, `PasswordResetStore`, `IAppCache`, `IEmailSender` and `DigestSender` are singletons that hold no scoped dependency. `ValidateScopes` and `ValidateOnBuild` are on (`Program.cs:36–37`), so a captive dependency fails at startup. |
| Async and cancellation | Mail sending and file writes pass `CancellationToken`. The repositories are synchronous (EF sync API). That is acceptable at this load and recorded P2. |
| Scale-out blockers | In-memory state: session, `IMemoryCache`, lockouts, reset codes, the digest queue. Recorded in `08` (Scalability) and `15`. |
