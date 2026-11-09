# 08 · Quality Attributes, CAP, Mini-ATAM

Lecture anchors:
- W1 s27–39: the six attributes.
- W1 s40–43 and W8 s9–10: CAP.
- W8 s21–23: ATAM.
- W9 s5: latency percentiles.

Status labels used below:

| Label | Meaning |
|---|---|
| MEASURED | A number produced in this task |
| TESTED | An automated test enforces it |
| TARGET / NOT MEASURED | A goal that has not been measured yet |

## Six quality attributes

| Attribute | Measurable target | Tactics in the design | How it is verified | Status |
|---|---|---|---|---|
| Scalability | Today: 1 app instance handles 20 concurrent readers at ≥ 40 req/s on the search page. Later: 200 concurrent readers by adding a second instance, once in-memory state moves to a shared store. | Stateless pages except: session, `IMemoryCache`, lockouts, reset codes, digest queue. List queries skip the article body; word index. | Benchmark: 44.7 req/s at 20 concurrent clients, 0 errors (`11`). | First part MEASURED. Scale-out TARGET / NOT MEASURED, blocked by in-memory state (`15` P1). |
| Availability | 99.5 % monthly during the semester (≤ 3.6 h downtime). A failure of the mail provider never fails a page. | `/health` with a database check; errors in mail delivery contained (`EmailSender.SendAsync`); digest in the background; migrations applied at start. | `EmailSenderTests` (failing transport returns `false`); smoke `/health` 200. | Mail isolation TESTED. Uptime TARGET / NOT MEASURED: there is no deployment, and one instance is a single point of failure (W1 s30). |
| Security | 0 unpublished stories visible to the public. Admin-only actions answer 403 to other roles. 5 failed sign-ins per email+IP lock that pair for 15 min. Passwords use PBKDF2 with 100k iterations. Every form POST requires an antiforgery token. | Role filters, `ArticleRules.CanView`, `LoginThrottle`, `PasswordResetStore`, CSP, rate limits, HttpOnly cookies, opaque API tokens stored as SHA-256 hashes. | `SecurityTests` (8), `AppTests` (roles, lockout, antiforgery, headers, hidden pending story); smoke 302/401/403/404. | TESTED |
| Maintainability | A new email channel or a new article creation rule changes ≤ 2 files. 0 reverse layer dependencies. CI runs 100 % of the tests on every push. | Layers enforced by the compiler; Builder; factory method; BLL-owned abstractions; 108 tests. | grep checks in `03`; the build itself. | TESTED (0 reverse dependencies), MEASURED (108 tests in CI) |
| Performance | Search page p95 ≤ 200 ms for one user and ≤ 1 s with 20 concurrent users, on one server. | `ArticleWords` index (prefix `LIKE` gives an index seek); `Cards` projection; caches (sections 30 min, listings 2 min, recommendation lists 2 min). | Local benchmark (`11`). | MEASURED: one user p95 = 51.2 ms; 20 users p95 = 621.9 ms, p99 = 762.0 ms. Both targets met on this machine with 28 stories. Not measured at production data volume. |
| Reliability | A review decision is stored exactly once. A scheduled story goes live within 60 s of its time, exactly once. No subscriber gets two digests in 6 days. A failed email never loses the request. | Concurrency token on `StatusId`; atomic `PublishDue`; `DigestGap` filter plus a per-edition queue guard. | `AppTests.Two_reviewers…`; smoke (second decision 409; scheduled story live after 85 s); `NewsletterServiceTests.Digest_skips…`. | TESTED and MEASURED |

**Benchmark conditions.**
- Machine: Windows 11 laptop, AMD Ryzen 5 7520U (8 logical CPUs), 13.8 GB RAM, SQL Server 2025 Express on the same machine.
- App: Release build, Development environment, loopback port 5199, database `NewsArchiveSmokeDB` with the 28 sample stories plus 3 smoke stories.
- Load: 10 warm-up requests, then 200 sequential requests per page, then 400 requests from 20 threads.
- Each measurement is one run. There is no repeat or variance analysis, so treat the numbers as indicative only.

## CAP trade-off (midterm §7)

**Most critical data.** The editorial state of an article (`Articles.StatusId`, review fields): Draft, Pending, Published, Scheduled or Rejected. This is what decides whether an unreviewed story can reach the public.

**Today.**
- There is one SQL Server instance.
- CAP is about a *distributed* system during a *network partition*, and one node has no partition between replicas (W8 s9 lists a single-node RDBMS as "CA").
- What protects the data today is **ACID**: single-row atomic updates plus the optimistic concurrency token.
- ACID consistency is not CAP consistency (W1 s41).

**Decision for a future replicated deployment: CP for editorial state.**
- If the app server cannot reach the primary database during a partition, approve, reject, schedule and edit must **fail** (409 or 503). They must not be accepted on a replica that might be stale.
- Business reason: publishing a story that was rejected or not yet reviewed is a legal and reputation risk for a news organisation. It also breaks the core rule that admin approval comes before publication. A delayed approval only costs a few minutes.

**AP for the reading side.**
- Article pages, search, recommendations, view counts and reading history may be served from a read replica or a cache that is a few seconds stale.
- Showing a story 2 minutes late, or a view count that lags, harms no one, while "site unavailable" loses readers.
- The current design already accepts this staleness inside one node: the 2-minute listing cache.

**Limits of this answer.** It is a design decision. No replica, partition test or failover exists in this repository.

## Mini-ATAM (W8 s21–23)

| Step | Result |
|---|---|
| 1 Present | Modular monolith (layered + MVC), one SQL Server, in-memory caches and stores, background jobs inside the web process. |
| 2 Approaches | Layering enforced at compile time; optimistic concurrency; atomic scheduled publish; word index; cache-aside with TTL; background queue; rate limits and lockout. |
| 3 Utility tree | Security (unreviewed news must never leak) > Reliability (decisions recorded exactly once) > Performance (search) > Availability > Scalability. |
| 4 Sensitivity points | Listing-cache TTL (2 min) controls freshness after a publish; on publish the cache region is cleared, which reduces the risk. Lockout window (15 min) affects both security and the inconvenience to real users. |
| 4 Trade-off points | **In-memory state:** raises performance and keeps things simple, but blocks horizontal scaling and loses lockouts and reset codes on restart. **Background job inside the web process:** simple, but a crash stops scheduled publishing until restart. |
| 5 Risks | R1 one app instance is a single point of failure. R2 in-memory stores prevent scale-out. R3 concurrent latency is about 12× the single-user latency (sync EF calls), still within target. R4 the edit form has no staleness check (`02` F3). |
| 5 Non-risks | Layer direction (enforced by the compiler); double publication (atomic UPDATE); SQL injection (parameterised EF). |
| 6 Results | Before any production launch: move session, lockouts and reset codes to a shared store (SQL or Redis) and run 2 instances (ADR-005, proposed); add a staleness check to the edit form; repeat the benchmark with 5,000 stories. |
