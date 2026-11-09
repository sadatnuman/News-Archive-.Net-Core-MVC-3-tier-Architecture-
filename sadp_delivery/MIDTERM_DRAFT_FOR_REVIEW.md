# CSC 4273 — Mid-Term Case Study Report (DRAFT FOR REVIEW)

This draft follows the official template in P2 p3–5. It uses only Lectures 1–3.

**Legend:**
- **[CONFIRMED]** = backed by the code or a test (paths relative to `NewsArchiveSolution/`).
- **[TBD]** = personal or group data still missing.

| Field | Entry |
|---|---|
| Project topic (system) | The Meridian Newsfolio: a bilingual (English/Bangla) news archive with editorial review |
| Group no. & section | [TBD] |
| Team members (ID — Name) | [TBD] (the repository is owned by GitHub account `sadatnuman`; the group must confirm every name and ID) |
| Submission date | [TBD] |
| Outcome | CO1 — Analyze quality attributes and trade-offs (with CO3) |

## 0 · Project Topic & Work-Split (GROUP)

The Meridian Newsfolio lets authors write news in English or Bangla, admins review, schedule and moderate it, and readers search, read and get recommendations. It is one ASP.NET Core application over SQL Server [CONFIRMED].

| Member (ID — Name) | Owns feature / module | NFA they achieve (§3) | SOLID (§6) + creational pattern (§9) |
|---|---|---|---|
| [TBD] — [TBD] (M1) | Editorial workflow (write, submit, review, schedule) | Reliability: one review decision stored exactly once | SRP · Builder (`ArticleBuilder`) |
| [TBD] — [TBD] (M2) | Accounts & security (sign-in, lockout, reset, roles, API tokens) | Security: lockout, CSRF, role checks | DIP · Singleton (DI-managed single instance) |
| [TBD] — [TBD] (M3) | Newsletter & email delivery | Availability: mail failures never break a page | OCP · Factory Method (`EmailSender.CreateTransport`) |
| [TBD] — [TBD] (M4) | Discovery (search, archive, recommendations) | Performance: search p95 | ISP · Abstract Factory (`IDataAccessFactory`) |

For 3 or 5 members, see `13_MEMBER_WORK_SPLIT_AND_VIVA.md`.

## 1 · Architecture vs. Design (GROUP, Q1)

Rule used, from W1 s5–6: a decision is architectural when reversing it is costly and the change reaches many modules or the deployment. It is design when it is local and cheap.

| Architectural decisions (hard to reverse) | Why architectural |
|---|---|
| A1: One deployable modular monolith in three layers (UI → BLL → DAL), enforced at compile time (`BLL/BLL.csproj:14`) | Changing it changes the deployment, the CI and how every feature talks to data |
| A2: SQL Server with EF Core and migrations as the single system of record | Changing it needs a data migration and touches every repository |
| A3: Identity: cookie sign-in for the website, plus opaque random API tokens stored as hashes for apps (`BLL/Services/AuthService.cs:101`) | Every endpoint and every client depends on it |

| Design decisions (cheap to revisit) | Why design |
|---|---|
| D1: `ArticleBuilder` builds new articles (`BLL/Builders/ArticleBuilder.cs`) | Local to the editorial module; callers would not notice a refactor |
| D2: `EmailSender` with the factory method `CreateTransport()` | Local to email delivery |
| D3: Search tokenizer and the 2-minute listing cache TTL | One class or one constant to change |

## 2 · Architectural Style & Why (GROUP, Q2)

**Primary style:** Layered (N-tier, W2 s6–7), plus MVC in the presentation layer (W2 s11), in a client–server setting. Browsers and API clients talk to one server over HTTPS; the server talks to SQL Server [CONFIRMED].

**Dependencies flow downward only:** Controller → Service → `IDataAccessFactory`/Repository → DbContext. Checks found:
- 0 UI→DAL references.
- 0 DAL→BLL references.
- 0 HTTP types in the BLL.

**Monolith vs SOA: a modular monolith.** The drivers match the "monolith best for" row of W2 s29:
- a team of 2–5 students;
- under 10,000 expected users;
- heavily shared data (articles, users, comments, history);
- strong consistency needed for review decisions (simple ACID in one database);
- one host.

SOA or microservices were rejected because separate pipelines, data ownership and network calls would cost more than the team can run, before any module has a real independent scaling need (W2 s30).

**Layers are not physical tiers.** UI, BLL and DAL run in one process; only the database is separate.

## 3 · Non-Functional Attribute & How (INDIVIDUAL, Q3)

| Member | Quality attribute | Measurable target | How the design achieves it (evidence) |
|---|---|---|---|
| M1 [TBD] | Reliability | When two admins decide the same pending story, exactly 1 decision is stored. A scheduled story goes live once, ≤ 60 s after its time. | Concurrency token on `StatusId` (`DAL/EF/NewsArchiveDbContext.cs:91`; conflict → `false` at `DAL/Repos/Repository.cs:47` → HTTP 409). Atomic `PublishDue` UPDATE (`DAL/Repos/ArticleRepo.cs:259`). Verified by `AppTests.Two_reviewers_cannot_both_decide_the_same_article` and the smoke run (second decision 409; scheduled story live after 85 s with its time set 75 s ahead). |
| M2 [TBD] | Security | 0 unpublished stories readable by the public. 5 failed sign-ins per email+IP lock that pair for 15 min. 100 % of form POSTs need an antiforgery token. Passwords use PBKDF2-SHA256 with 100k iterations. | `ArticleRules.CanView`; `LoginThrottle` (`NewsArchive/Auth/LoginThrottle.cs:7`); `AutoValidateAntiforgeryToken` (`Program.cs:151`); `PasswordHasher` (`BLL/Security/PasswordHasher.cs:9`). Verified by `SecurityTests` and `AppTests` (lockout → 429, missing token → 400, roles → 401/403). |
| M3 [TBD] | Availability | A mail-server outage or a bad address causes 0 failed page requests. *Send now* returns before any email is sent. | `EmailSender.SendAsync` contains every delivery error and returns `false` (`NewsArchive/Email/EmailSender.cs:22`); the digest goes through the background `DigestSender` queue. Verified by `EmailSenderTests.A_failing_transport_returns_false…`. |
| M4 [TBD] | Performance | Search page p95 ≤ 200 ms for 1 user and ≤ 1 s for 20 concurrent users on one server. | `ArticleWords` index (prefix seek), list projection without article text, caches. Measured locally: p95 = 51.2 ms (1 user) and 621.9 ms (20 users, 44.7 req/s). Both targets met on this laptop with 28 stories. |

## 4 · C4 Diagram & Why (GROUP, Q4)

Insert the **Level-1 System Context** diagram from `09_C4_AND_4PLUS1/C4_DIAGRAMS.md`: Reader, Author, Admin and API client around the Newsfolio box; external SMTP provider, search engines and feed readers, and the font CDN.

**Why this view:**
- It shows everyone who uses the Newsfolio and every outside system it depends on, on one page that a non-technical reader understands in a minute.
- It fixes the system boundary (only email leaves the system) before any technology detail.

## 5 · 4+1 View Model (GROUP, Q5)

| 4+1 view | Our system in this view |
|---|---|
| Logical | Article (with status, schedule and review fields), User/Role, Category, Tag, Comment/CommentReport, Bookmark, ReadingHistory, Subscriber, AuditLog, ArticleWord. Services: ArticleService + ArticleBuilder + ArticleRules, Search/Recommendation, Comment, Auth, Newsletter, Audit. |
| Process | One request per thread-pool thread, each with its own scoped DbContext. Shared singletons: lockout, reset codes, cache. Background job every 30 s publishes due stories and indexes words. A background queue sends digests. Optimistic concurrency on review decisions. |
| Development | `NewsArchiveSolution.slnx`: DAL (EF, repositories, migrations) ← BLL (services, rules, builders) ← NewsArchive (MVC + API + Razor) and NewsArchive.Tests. CI builds and runs 108 tests on every push. |
| Physical | Dev: one laptop running Kestrel plus SQL Server Express, with local disk for uploads and the email outbox. CI: GitHub Ubuntu runner plus a SQL Server 2022 container. Planned production: one app host plus a managed SQL database (not deployed). |
| Scenarios | S1 author submits → admin approves → reader reads. S2 two admins race on one story. S3 scheduled story goes live. S4 bilingual search. S5 subscribe and digest. Each is traced through all four views and tested (`09_C4_AND_4PLUS1/4PLUS1_VIEWS.md`). |

## 6 · SOLID in a Feature (INDIVIDUAL, Q6)

| Member | Feature | SOLID principle | How it is applied & why |
|---|---|---|---|
| M1 [TBD] | Article creation and review | SRP | Construction and invariants live in `ArticleBuilder`; workflow, audit and cache in `ArticleService`; policy in `ArticleRules`. A change to status rules touches one class. |
| M2 [TBD] | Password change | DIP | `PasswordChanger` depends on `IAuthService` (`NewsArchive/Auth/PasswordChanger.cs:15`), so the test injects a fake and needs no database. |
| M3 [TBD] | Email delivery | OCP | A new channel is a new `EmailSender` subclass plus a transport. Its callers (newsletter, auth, digest) stay unchanged. |
| M4 [TBD] | Recommendations | ISP | `RecommendationService` uses only the article and reading-history repository contracts. **Caveat:** the factory could hand it all 11 repositories. |

## 7 · CAP Trade-off (GROUP, Q7)

**Most critical data.** An article's editorial status: whether a story may be public.

**Today.** One SQL Server node with ACID transactions and an optimistic concurrency check. There is no replica, so there is no network partition to choose about. ACID consistency is not CAP consistency (W1 s41).

**Design decision for replication: CP for editorial status.** During a partition, approve, reject and schedule are refused (error) rather than written to a copy that may be stale. A story that was rejected or never reviewed must never reach readers, which matters legally and for trust, while a delayed approval costs minutes.

**AP for reading.** Pages, search, recommendations and view counts may be a few seconds stale. Being unavailable would lose readers, and staleness hurts no one.

## 8 · Monolith → Microservices Migration (GROUP, Q8)

| Phase | Trigger | Step | What it buys | Cost |
|---|---|---|---|---|
| 1 Now | — | Modular monolith on 1 host + SQL Server; modules talk through BLL interfaces | Fast delivery, one transaction | Scales as one unit |
| 2 | > ~1k concurrent readers | Move session, lockouts and reset codes to a shared store; run 2+ identical instances behind a load balancer | Horizontal scaling and redundancy without splitting code | One more round trip per sign-in |
| 3 **First service** | Newsletter sends grow, or mail slows the web host | Extract **Newsletter & Notifications** (subscribers, digest building, `EmailSender`). It owns the `Subscribers` table. It reads published stories through the archive's REST API (or a "story published" message later). | Bursty email work is isolated from reading; it can be deployed and scaled alone; a mail outage cannot slow pages | Duplicated read model, network calls, a second pipeline |
| 4 | Search load dominates | Extract **Search & Recommendations**. It owns `ArticleWords` and the reading history, and is fed by story-published updates. | Scale the CPU- and query-hot part alone | Eventual consistency of the index |
| 5 | Only if needed | Keep **Editorial + Accounts** together as the core (strong consistency, CP) | Simpler correctness | — |

**Why the newsletter goes first.** Its data (subscribers) is already separate. It only *reads* stories. It has the burstiest workload. Splitting it never breaks the CP editorial rule. This follows the W2 s31 rule: extract the module with the clearest independent need first.

## 9 · Creational Design Patterns (INDIVIDUAL, Q9)

| Member | Feature | Creational pattern | Why it fits / what it solves |
|---|---|---|---|
| M1 [TBD] | Creating an article | Builder (`BLL/Builders/ArticleBuilder.cs`) | 12+ fields, optional image, breaking flag and publish time, and role-dependent status rules. Fluent steps plus a validating `Build()` replace a long object initialiser and scattered status logic (W3 s25–28). Tested by 7 unit tests. |
| M2 [TBD] | Lockout and reset-code stores | Singleton, DI-managed | Exactly one store must exist, or a lockout recorded by one instance is missed by another. The DI container guarantees one instance (W3 s11 "DI-scoped singleton"), and it stays testable. Tested with `Assert.Same` across scopes. |
| M3 [TBD] | Sending email | Factory Method (`EmailSender.CreateTransport()`) | The creator keeps the shared send logic; subclasses decide which transport to create (SMTP or outbox). A new channel = new subclass (OCP). Tested by 5 tests. |
| M4 [TBD] | Data access for discovery | Abstract Factory (`IDataAccessFactory`) | Creates a family of repositories that share one unit of work. Swapping to the in-memory test family changes no service code. Tested by `Repositories_from_one_factory_share_one_unit_of_work`. |

## 10 · References (GROUP, IEEE)

[1] L. Bass, P. Clements, and R. Kazman, *Software Architecture in Practice*, 3rd ed. Boston, MA, USA: Addison-Wesley, 2012.
[2] E. Gamma, R. Helm, R. Johnson, and J. Vlissides, *Design Patterns: Elements of Reusable Object-Oriented Software*. Reading, MA, USA: Addison-Wesley, 1994.
[3] R. C. Martin, *Clean Architecture: A Craftsman's Guide to Software Structure and Design*. Boston, MA, USA: Prentice Hall, 2017.
[4] P. Kruchten, "The 4+1 view model of architecture," *IEEE Software*, vol. 12, no. 6, pp. 42–50, Nov. 1995.
[5] E. Brewer, "CAP twelve years later: How the 'rules' have changed," *Computer*, vol. 45, no. 2, pp. 23–29, Feb. 2012.
[6] S. Brown, "The C4 model for visualising software architecture." [Online]. Available: https://c4model.com
[7] M. Kleppmann, *Designing Data-Intensive Applications*. Sebastopol, CA, USA: O'Reilly, 2017.
[8] Microsoft, "Handling concurrency conflicts — EF Core." [Online]. Available: https://learn.microsoft.com/ef/core/saving/concurrency
[9] M. Al-Amin, "CSC 4273 Lectures 1–3," AIUB, Summer 2025-26 (course slides).
