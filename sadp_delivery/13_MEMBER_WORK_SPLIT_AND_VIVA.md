# 13 · Member Work Split and Viva Preparation

P2 p2: "the group owns the system (§1, 2, 4, 5, 7, 8); each member owns a feature and does §3, 6 and 9 for it. Sections 3, 6 and 9 must name which member did which part."

**Personal data rule.** Member names and student IDs are not in the repository and are not invented here. Every member is `TBD — TBD`. The GitHub account `sadatnuman` owns the repository; the group must confirm who that is and fill in the rest.

## Recommended split for 4 members

| Member (ID — Name) | Owns feature / module | NFA (§3) | SOLID (§6) | Creational pattern (§9) |
|---|---|---|---|---|
| M1: TBD — TBD | Editorial workflow: write, submit, review, schedule (FR-1, 2, 3) | Reliability: a review decision is applied exactly once | SRP: builder vs service vs rules | **Builder**: `ArticleBuilder` |
| M2: TBD — TBD | Accounts and security: register, sign-in, lockout, password reset, roles, API tokens (FR-8, 12) | Security: brute force and CSRF limits | DIP: `PasswordChanger` → `IAuthService` | **Singleton** (DI-managed single instance): `LoginThrottle`, `PasswordResetStore` |
| M3: TBD — TBD | Newsletter and email: subscribe, confirm, digest, email delivery (FR-7) | Availability: mail failures never break a page | OCP: new mail channel = new class | **Factory Method**: `EmailSender.CreateTransport()` |
| M4: TBD — TBD | Discovery: search, archive, recommendations, reading history (FR-4, 5, 9) | Performance: search p95 target | ISP: narrow repository contracts | **Abstract Factory**: `IDataAccessFactory` repository family |

### Other team sizes

- **3 members:** M4's module merges into M1 (Editorial + Discovery). M1 keeps Builder, Reliability and SRP. Performance and ISP then move to the group-level part of the proposal.
- **5 members:** add M5 for Comments and moderation (FR-6).
  - NFA: Reliability (each reader can report a comment once; the third report hides it exactly once).
  - SOLID: SRP (`ModerationRules` vs `CommentService`).
  - Creational: Abstract Factory from the comment side (`CommentService` takes the comment and article repositories from one `IDataAccessFactory`).
  - The guideline allows two members to apply the same pattern to different features.
- **2 members:** M1 takes Editorial + Discovery; M2 takes Accounts + Newsletter. Each keeps one NFA, one principle and one pattern from their original rows.

## Per-member detail (what to show in the viva)

### M1: Editorial workflow

- **§3 Reliability.**
  - Target: when two reviewers act on the same pending story, exactly one decision is stored. A scheduled story goes live exactly once, within 60 s of its time (the job runs every 30 s).
  - Tactics:
    - Optimistic concurrency token on `Articles.StatusId` (`DAL/EF/NewsArchiveDbContext.cs:91`; conflict is caught at `DAL/Repos/Repository.cs:47`).
    - Atomic `UPDATE … WHERE StatusId=Scheduled AND ScheduledAt<=now` (`DAL/Repos/ArticleRepo.cs:259`).
  - Proof:
    - `AppTests.Two_reviewers_cannot_both_decide_the_same_article`.
    - Smoke: a second decision gets 409.
    - Smoke: the scheduled story went live after 85 s, with the publish time set 75 s ahead.
- **§6 SRP.** `ArticleBuilder` (construction and invariants), `ArticleService` (workflow, audit, cache) and `ArticleRules` (policy) each have one reason to change.
- **§9 Builder.** Participants:
  - Builder: `ArticleBuilder`.
  - Product: `Article`.
  - Director role: `ArticleService.Create` (`BLL/Services/ArticleService.cs:60`), which picks the steps from the actor's role.
  - `Build()` (`BLL/Builders/ArticleBuilder.cs:76`) validates and decides the status.
  - Why it fits, using the W3 s31 flowchart (Q2): an article has 12+ fields, several optional ones (image, breaking flag, publish time), and status rules that depend on who builds it.
  - Tests: `ArticleBuilderTests` (7).

### M2: Accounts and security

- **§3 Security.** Targets and tactics:
  - Passwords: PBKDF2-SHA256 with 100,000 iterations (`BLL/Security/PasswordHasher.cs:9`).
  - Lockout: 5 failed sign-ins per email and IP lock that pair for 15 minutes (`NewsArchive/Auth/LoginThrottle.cs:7`).
  - Rate limits: 20 sign-in requests per minute per IP (`Program.cs:105`).
  - Reset codes: 10 minutes, 5 tries, 3 per hour.
  - CSRF: an antiforgery token is required on every form POST (`Program.cs:151`).
  - Roles: admin endpoints return 403 to authors and readers.
  - Proof: `SecurityTests` (8); `AppTests` lockout, antiforgery and roles; smoke 401/403/302.
- **§6 DIP.** `PasswordChanger` depends on the `IAuthService` abstraction (`NewsArchive/Auth/PasswordChanger.cs:15`), and the test injects a fake. `AuthService` depends on `IDataAccessFactory` and `IAuditService`.
- **§9 Singleton, DI-managed.**
  - `LoginThrottle` and `PasswordResetStore` must be exactly one instance per app. With two instances, a lockout counter or a reset code stored in one would be invisible to the other, and the security rule would silently break.
  - The single instance is guaranteed by the DI container lifetime (`Program.cs:52`, `:118`). It is not built with a private constructor and a static `GetInstance()`.
  - W3 s11 calls this the "modern fix: a DI-scoped singleton — singleton behaviour WITH testability"; W3 s31 says "Singleton (or a DI-scoped singleton)".
  - Test: `AppTests.Lockout_and_reset_stores_are_one_instance_for_the_whole_app` (`Assert.Same` across two scopes).
  - **Viva risk:** be ready to explain the three classic parts (W3 s8) and why they were not hand-written: global state, cannot be mocked, violates DIP.

### M3: Newsletter and email

- **§3 Availability.**
  - Target: a mail-server outage or a bad address never turns a page into an error. Every send returns `true` or `false` and never throws.
  - Target: the admin's *Send now* request returns before any email is sent, because sending runs in the background `DigestSender`, whatever the list size.
  - Tactics: error containment in `EmailSender.SendAsync` (`NewsArchive/Email/EmailSender.cs:22`); background queue (`NewsArchive/Email/DigestSender.cs:25`); SMTP timeout capped at 5–120 s.
  - Proof: `EmailSenderTests.A_failing_transport_returns_false_instead_of_crashing_the_page`; the smoke subscribe call returned 200 and wrote exactly one outbox file.
  - Not measured: real SMTP latency, because no mail server was used.
- **§6 OCP.** Adding a channel (for example an HTTP mail API) means one new `EmailSender` subclass plus a transport, and one line in `Program.cs:112–114`. `NewsletterService`, `NewsletterController`, `AuthController` and `DigestSender` stay closed to change.
- **§9 Factory Method.** Participants, matching W3 s14:
  - Creator: `EmailSender`. It holds the business logic (validate, send, log, contain errors) and calls the factory method `CreateTransport()` (`EmailSender.cs:20`).
  - Concrete creators: `SmtpEmailSender` and `OutboxEmailSender`.
  - Product: `IMailTransport`.
  - Concrete products: `SmtpTransport` (a new `SmtpClient` per message, because `SmtpClient` is not thread-safe) and `OutboxTransport`.
  - Tests: `EmailSenderTests` (5), including an LSP contract test across two creators.

### M4: Discovery

- **§3 Performance.**
  - Target: search page p95 ≤ 1 s with 20 concurrent users, and ≤ 200 ms for a single user, on one server. This follows the SmartLib style in P1 p2.
  - Measured on this laptop (Release build, 28 sample stories, loopback):
    - One user: p95 = 51.2 ms.
    - 20 concurrent users: p95 = 621.9 ms, 44.7 requests per second.
    - **Both targets met.**
  - Finding: latency grows about 12× under concurrency. The likely cause is synchronous EF Core calls holding thread-pool threads; that is a hypothesis to test next (`15`).
  - Tactics: word index `ArticleWords` (`DAL/Repos/ArticleRepo.cs:101`); list projection without the article body (`ArticleRepo.cs:20`); 2-minute cache of recommendation lists (`BLL/Services/RecommendationService.cs:89`).
- **§6 ISP.** `RecommendationService` uses only `IArticleRepository` and `IReadingHistoryRepository` (`RecommendationService.cs:17`), not a fat data interface. **Honest caveat:** it receives them through `IDataAccessFactory`, which could hand out all 11 repositories (smell recorded in `03`).
- **§9 Abstract Factory.** Participants, matching W3 s20–22:
  - Abstract factory: `IDataAccessFactory` (`DAL/Interfaces/IDataAccessFactory.cs:5`).
  - Concrete factories:
    - `DataAccessFactory`, the SQL Server family.
    - The DispatchProxy `FakeFactory`, an in-memory test family (`NewsArchive.Tests/Support/TestServices.cs:52`).
  - Abstract products: `IArticleRepository`, `IReadingHistoryRepository`, `ICategoryRepository`, and others.
  - Concrete products: `ArticleRepo`, `ReadingHistoryRepo`, and the others.
  - The family guarantee: every repository from one factory shares one `DbContext`, so they see one identity map and one transaction.
  - Test: `AppTests.Repositories_from_one_factory_share_one_unit_of_work`. Same family: the section loaded by the category repository is the same object as `article.Category` from the article repository. Different scope: a different object.

## Group-owned sections (all members)

§1, 2, 4, 5, 7, 8 and 10: see `MIDTERM_DRAFT_FOR_REVIEW.md`.

## Viva Q1–Q9 cheat sheet

| Q | Short answer with code pointer |
|---|---|
| Q1 Architecture vs design | Architecture: modular monolith, SQL Server with EF Core, cookie + opaque-token auth, three-layer split. Design: Builder for articles, factory method for mail, word-index tokenizer, a 2-minute cache TTL. Test: what does it cost to reverse, and how far does the change reach (W1 s5–6)? |
| Q2 Style | Layered (UI → BLL → DAL, enforced at compile time) plus MVC inside the UI plus client–server (browser and API clients → one ASP.NET Core server → SQL Server). A modular monolith, because of team size, under 10k users and one deployment (W2 s29–30). |
| Q3 NFA | Each member's row above: a number, a tactic and a test. |
| Q4 C4 | The L1 context diagram goes in the report: people and external systems around one box. L2 (in `09`) shows the single web app container plus SQL Server. |
| Q5 4+1 | See §5 of the draft; every view comes from the real code and the real `Program.cs`. |
| Q6 SOLID | Each member's row; LSP is covered by a contract test. |
| Q7 CAP | Today: one SQL Server node, so there is no partition (ACID is not CAP). Planned rule for replicas: review decisions CP, reading AP. |
| Q8 Migration | The newsletter/notification service is extracted first; full plan in §8. |
| Q9 Creational | Builder (M1), Singleton via DI (M2), Factory Method (M3), Abstract Factory (M4). |
