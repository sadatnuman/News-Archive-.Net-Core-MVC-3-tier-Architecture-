# 13 · Member Work Split and Viva Preparation

P2 p2: "the group owns the system (§1, 2, 4, 5, 7, 8); each member owns a feature and does §3, 6 and 9 for it. Sections 3, 6 and 9 must name which member did which part."

**Personal data rule.** Member names and student IDs are not in the repository and are not invented here. Every member is `TBD — TBD`. The GitHub account `sadatnuman` owns the repository; the group must confirm who that is and fill in the rest.

## Recommended split for 4 members

| Member (ID — Name) | Owns feature / module | NFA (§3) | SOLID (§6) | Creational pattern (§9) |
|---|---|---|---|---|
| M1: TBD — TBD | Editorial workflow: write, submit, review, schedule, edit (FR-1, 2, 3) | Reliability: no lost review decision, no lost edit | SRP: builder vs service vs rules | **Builder**: `ArticleBuilder` |
| M2: TBD — TBD | Accounts and security: register, sign-in, lockout, password reset, roles, API tokens (FR-8, 12) | Security: brute force and CSRF limits | DIP: `PasswordChanger` → `IAuthService` | **Singleton** (DI-managed single instance): `LoginThrottle`, `PasswordResetStore` |
| M3: TBD — TBD | Newsletter and email: subscribe, confirm, digest, email delivery (FR-7) | Availability: mail failures never break a page | OCP: new mail channel = new class | **Factory Method**: `EmailSender.CreateTransport()` |
| M4: TBD — TBD | Discovery: search, date archive, recommendations, reading history (FR-4, 5, 9) | Performance: search p95 target | ISP: `IArticleArchive` (3 members) for the archive clients | **Abstract Factory**: `IDataAccessFactory` repository family |

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
  - Targets:
    - When two reviewers act on the same pending story, exactly one decision is stored, and the other reviewer is told "Another editor acted on this story".
    - An edit saved from an out-of-date form never overwrites a newer edit.
    - A scheduled story goes live exactly once, within 60 s of its time (the job runs every 30 s).
  - Tactics:
    - Optimistic concurrency tokens on `Articles.StatusId` and `Articles.UpdatedAt` (`DAL/EF/NewsArchiveDbContext.cs:91–92`; a conflict is caught at `DAL/Repos/Repository.cs:47`).
    - A version in the edit form, checked in `ArticleService.Update` (`BLL/Services/ArticleService.cs:89`).
    - `ChangeResult.Conflict`, so a race gets its own message.
    - Atomic `UPDATE … WHERE StatusId=Scheduled AND ScheduledAt<=now` (`DAL/Repos/ArticleRepo.cs:259`).
  - Proof:
    - `AppTests`: `Two_reviewers_cannot_both_decide_…`, `A_decision_racing_another_editor_…`, `An_edit_from_a_stale_form_…`, `Two_saves_in_flight_…`.
    - Smoke: a stale API edit gets 409, and a stale MVC form shows the conflict message.
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
  - Lockout: at most 5 password checks per email and IP per 15 minutes, even when 20 requests arrive together (`LoginThrottle.Attempt`, `NewsArchive/Auth/LoginThrottle.cs:41`).
  - Uploads: picture clean-up deletes only app-generated paths that no article or user still uses (`BLL/Services/MediaService.cs:41`, `:48`).
  - Rate limits: 20 sign-in requests per minute per IP (`Program.cs:105`).
  - Reset codes: 10 minutes, 5 tries, 3 per hour.
  - CSRF: an antiforgery token is required on every form POST (`Program.cs:151`).
  - Roles: admin endpoints return 403 to authors and readers.
  - Proof: `SecurityTests` (10, including 20 parallel attempts → exactly 5 checks); `HardeningTests` (12 parallel API attempts → 5 × 401 + 7 × 429; shared and foreign picture paths kept); `AppTests` antiforgery and roles; smoke 401/403/302.
- **§6 DIP.** `PasswordChanger` depends on the `IAuthService` abstraction (`NewsArchive/Auth/PasswordChanger.cs:15`), and the test injects a fake. `AuthService` depends on `IDataAccessFactory` and `IAuditService`.
- **§9 Singleton, DI-managed.**
  - `LoginThrottle` and `PasswordResetStore` must be exactly one instance per app. With two instances, a lockout counter or a reset code stored in one would be invisible to the other, and the security rule would silently break.
  - The single instance is guaranteed by the DI container lifetime (`Program.cs:52`, `:118`). It is not built with a private constructor and a static `GetInstance()`.
  - W3 s11 calls this the "modern fix: a DI-scoped singleton — singleton behaviour WITH testability"; W3 s31 says "Singleton (or a DI-scoped singleton)".
  - Test: `AppTests.Lockout_and_reset_stores_are_one_instance_for_the_whole_app` (`Assert.Same` across two scopes).
  - **Viva answer (keep this):** "The intent of Singleton is one instance for the whole app. We kept that intent, and a test proves it with `Assert.Same`. We moved the guarantee from the class to the DI container: the classic private constructor, static field and `GetInstance()` (W3 s8) would give a global access point, which cannot be mocked and breaks DIP. The lecture calls our form a DI-scoped singleton (W3 s11, s31). Microsoft's DI guide says the same: singleton services instead of global state." The side-by-side table is in `07`.
  - **Weakness to admit if asked:** the container guarantees one instance only for objects taken from the container. Code could still call `new LoginThrottle(...)`; only the unit tests do that, which is the testability benefit.

### M3: Newsletter and email

- **§3 Availability.**
  - **Scope:** real email is not implemented (owner's decision, ADR-007). Development writes to a private outbox; every other environment uses `DisabledEmailSender`.
  - Target: a failing transport, a bad address or disabled email never turns a page into an error. Every send returns `true` or `false` and never throws, and a disabled send never claims success.
  - Target: the admin's *Send now* request returns before any email is sent, because sending runs in the background `DigestSender`, whatever the list size.
  - Tactics: error containment in `EmailSender.SendAsync` (`NewsArchive/Email/EmailSender.cs:22`); background queue (`NewsArchive/Email/DigestSender.cs:25`); SMTP timeout capped at 5–120 s.
  - Proof: `EmailSenderTests.A_failing_transport_returns_false_instead_of_crashing_the_page`; `HardeningTests.Outside_development_mail_is_disabled…` (Production with an SMTP host configured: 2,000 sends return `false`, no new warning, no SMTP socket); smoke: confirmation, digest and reset code all go to the outbox only.
  - Not measured: real email deliverability. It is out of scope, and this is stated in the report.
- **§6 OCP.** Adding a channel (for example an HTTP mail API) means one new `EmailSender` subclass plus a transport, and one line in `Program.cs:112–114`. `NewsletterService`, `NewsletterController`, `AuthController` and `DigestSender` stay closed to change.
- **§9 Factory Method.** Participants, matching W3 s14:
  - Creator: `EmailSender`. It holds the business logic (validate, send, log, contain errors) and calls the factory method `CreateTransport()` (`EmailSender.cs:20`).
  - Concrete creators: `OutboxEmailSender` (used in Development) and `SmtpEmailSender` (dormant, never registered).
  - Product: `IMailTransport`.
  - Concrete products: `SmtpTransport` (a new `SmtpClient` per message, because `SmtpClient` is not thread-safe) and `OutboxTransport`.
  - Tests: `EmailSenderTests` (5), including an LSP contract test across two creators.

### M4: Discovery

- **§3 Performance.**
  - Target: search page p95 ≤ 1 s with 20 concurrent users, and ≤ 200 ms for a single user, on one server. This follows the SmartLib style in P1 p2.
  - Measured on this laptop (Release build, **5,024 published stories**, 915,558 word-index rows, loopback; details in `08`):
    - One user: p50 = 46.7 ms, p95 = 66.9 ms, p99 = 71.0 ms.
    - 20 concurrent users: p50 = 476.8 ms, p95 = 784.2 ms, p99 = 1,028.0 ms, 38.9 requests per second, 0 errors (final run; earlier runs 40.8–43.0 req/s).
    - **Both targets met on this machine.**
  - Finding: latency grows about 10× under concurrency. The likely cause is synchronous EF Core calls blocking thread-pool threads; that is a hypothesis to test next (`15` R-3).
  - Tactics: word index `ArticleWords` (`DAL/Repos/ArticleRepo.cs:101`); list projection without the article body (`ArticleRepo.cs:20`); 2-minute cache of recommendation lists (`BLL/Services/RecommendationService.cs:89`).
- **§6 ISP.**
  - The archive pages, the archive API and the sitemap need only 3 operations: month list, day counts, stories in a period.
  - They now depend on `IArticleArchive` (`BLL/Interfaces/IArticleArchive.cs`), which has exactly those 3 members. Before, they depended on the 29-member `IArticleService`; it now has 26.
  - Proof: `AppTests.Archive_pages_depend_only_on_the_narrow_archive_contract` registers a 3-method stub, and `/archive` and `/api/archive` are served from it. No method of the editorial interface was needed.
  - Why ISP and not "narrow repositories": the repositories are created by the Abstract Factory, which hands out all 11. The new interface is a client-specific contract in the BLL, which is what ISP asks for (W1 s19–20).
  - Caveat to admit: `IArticleService` is still large for the editorial pages (`15` R-6).
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
| Q4 C4 | The L1 context diagram (`09_C4_AND_4PLUS1/c4_level1_context.png`) goes in the report: people and external systems around one box. L2 (`c4_level2_containers.png`) shows the web application, browser scripts, database and two file stores inside the boundary. |
| Q5 4+1 | See §5 of the draft; every view comes from the real code and the real `Program.cs`. |
| Q6 SOLID | Each member's row: SRP (M1), DIP (M2), OCP (M3), ISP with `IArticleArchive` (M4). LSP is also covered by a contract test. |
| Q7 CAP | Today: one SQL Server node, so there is no partition (ACID is not CAP). Planned rule for replicas: review decisions CP, reading AP. |
| Q8 Migration | The newsletter/notification service is extracted first; full plan in §8. |
| Q9 Creational | Builder (M1), Singleton via DI (M2), Factory Method (M3), Abstract Factory (M4). |
