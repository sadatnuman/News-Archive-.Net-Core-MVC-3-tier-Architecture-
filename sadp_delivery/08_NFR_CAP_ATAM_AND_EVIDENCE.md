# 08 · Quality Attributes, CAP, Mini-ATAM, Dependency Risk

**Lecture anchors:**
- W1 s27–39: the six quality attributes.
- W1 s40–43: CAP (midterm material).
- W8 s9–10 (CAP in practice), W8 s21–23 (ATAM), W9 s5 (latency percentiles): later-week material, used here only for the full-course record. They are not used to answer the midterm questions.

## Status words

| Status | Meaning |
|---|---|
| VERIFIED | An automated test (or the compiler) enforces it on every run |
| MEASURED | A number produced in this work, on the machine described below |
| TARGET | A goal stated but not yet measured or tested |
| NOT VERIFIED | Cannot be checked without something this project does not have (a deployment, a second server, real SMTP) |

## Six quality attributes

| Attribute | Measurable target | Tactics in the design | Evidence | Status |
|---|---|---|---|---|
| Reliability | (a) A review decision is stored exactly once, and the losing editor is told so. (b) An edit from an out-of-date form never overwrites a newer edit. (c) A scheduled story goes live within 60 s of its time, exactly once. (d) No subscriber gets two digests within 6 days. | `StatusId` and `UpdatedAt` concurrency tokens; form version check; `ChangeResult.Conflict` → 409 with its own message; atomic `PublishDue`; digest gap filter plus a per-edition queue guard. | Tests: `Two_reviewers_cannot_both_decide_…`, `A_decision_racing_another_editor_…`, `An_edit_from_a_stale_form_…`, `Two_saves_in_flight_…`, `NewsletterServiceTests.Digest_skips…`. Smoke: API 409 and MVC conflict page; scheduled story live after 85 s. | (a), (b), (d) VERIFIED. (c) MEASURED (one run). |
| Security | 0 unpublished stories visible to the public. Admin-only actions answer 403 to other roles. 5 failed sign-ins per email+IP lock that pair for 15 min. PBKDF2 with 100k iterations. Every form POST needs an antiforgery token. Every AutoMapper map has a depth limit. | Role filters, `ArticleRules.CanView`, `LoginThrottle`, `PasswordResetStore`, CSP, rate limits, HttpOnly cookies, API tokens stored as SHA-256 hashes, `MaxDepth(3)` on every map. | `SecurityTests` (8), `MappingDepthTests` (1), `AppTests` (roles, lockout, antiforgery, headers, hidden pending story); smoke 302/401/403/404. | VERIFIED |
| Availability | (a) A mail outage, a bad address or missing mail settings never fail a page, and never report a mail as sent when it was not. (b) 99.5 % monthly uptime. | Error containment in `EmailSender.SendAsync`; Production without SMTP returns `false`; digest in the background; `/health` with a database check. | `EmailSenderTests` (5); smoke `/health` 200. | (a) VERIFIED. (b) NOT VERIFIED: no deployment, and one instance is a single point of failure. |
| Performance | Search page p95 ≤ 200 ms for one user and ≤ 1 s with 20 concurrent users, on one server, with ≥ 5,000 stories. | `ArticleWords` index (prefix `LIKE` uses an index seek); list projection without the article body; caches (sections 30 min, listings 2 min). | Benchmark below. | MEASURED: one user p95 = 72.2 ms; 20 users p95 = 672.5 ms. Both targets met on this machine. |
| Scalability | (a) 1 instance serves 20 concurrent readers with 0 errors at ≥ 40 req/s on the search page. (b) 2+ instances after in-memory state moves to a shared store. | Pages hold no state except: session, cache, lockouts, reset codes, digest queue. | Benchmark: 43.0 req/s, 0 errors, 20 clients, 5,024 published stories. | (a) MEASURED. (b) TARGET, blocked by in-memory state (`15` R-1, ADR-005). |
| Maintainability | 0 reverse layer dependencies. A new email channel or a new article-creation rule changes ≤ 2 files. All tests run on every push. | Compiler-enforced layers; Builder; Factory Method; BLL-owned interfaces; `IArticleArchive` for the archive clients. | Build; grep checks in `03`; 116 tests pass locally; CI runs on push and pull request to `develop`/`main`. | VERIFIED |

## Search benchmark (5,000+ stories)

**Dataset.**
- Database `NewsArchiveSmokeDB`, recreated by `tools/smoke/smoke.py`.
- Contents: the bundled sample data, the smoke stories, plus **5,000 generated stories**.
- Totals: **5,024 published, 5,031 in all, 915,558 rows in the `ArticleWords` index.**
- How the stories were made: through the real admin form (`POST /Article/Create`), so every story was validated, built by `ArticleBuilder` and indexed by the app itself. All 5,000 returned 302.
- Language: half English, half Bangla. Section: picked at random.
- Story shape: title 6–10 words, summary 20–35 words, body 3–6 paragraphs of 40–90 words.
- Words: drawn with Zipf-like weights from a 3,000-word vocabulary per language. 83 real English and 68 real Bangla news words; the rest are synthetic words built from syllables. Random seed 4273.

**Configuration.**
- Windows 11 laptop: AMD Ryzen 5 7520U (8 logical CPUs), 13.8 GB RAM.
- SQL Server 2025 Express on the same machine.
- Release build, Development environment, Kestrel on `127.0.0.1:5199`, anonymous requests.
- The test client runs on the same machine.

**Workload.**
- 30 fixed search terms, built from the seed:
  - per language: 3 frequent words, 3 mid-frequency words, 3 rare words, 3 two-word queries, 2 three-letter prefixes;
  - plus 2 queries that match nothing.
- Page measured: `/Article/Index?keyword=…` (the full HTML search page).
- Warm-up: 20 requests.
- Then 400 sequential requests, then 800 requests from 20 threads.

**Results (one run):**

| Mode | n | p50 | p95 | p99 | Throughput | Errors |
|---|---|---|---|---|---|---|
| Sequential | 400 | 47.1 ms | 72.2 ms | 80.3 ms | 20.9 req/s | 0 |
| 20 concurrent | 800 | 446.7 ms | 672.5 ms | 779.5 ms | 43.0 req/s | 0 |

**Compared with the earlier 31-story run (`11`):**
- Sequential p95 rose from 51.2 ms to 72.2 ms.
- Concurrent p95 rose from 621.9 ms to 672.5 ms.
- With about 160 times more stories, latency grew by a third or less. The word index is doing its job.

**Limits of these numbers.**
- One run; no variance analysis.
- Client and server share the CPU.
- The text is synthetic; real Bangla text may produce a different word distribution.
- All generated stories carry the same publish day, which may make date ordering cheaper than in a real archive.
- SQL Server Express limits memory and CPU.
- The REST search endpoint was **not** benchmarked: it allows 120 requests per minute per IP by design, so a load test from one IP measures the rate limiter, not search.

## CAP trade-off (midterm §7)

**Most critical data.** The editorial state of an article (`Articles.StatusId` and the review fields). It decides whether an unreviewed story can reach the public.

**Today.**
- There is one SQL Server instance.
- CAP is about a *distributed* system during a *network partition*. One node has no partition between replicas.
- What protects the data today is **ACID**: single-row atomic updates plus the optimistic concurrency tokens.
- ACID consistency is not CAP consistency (W1 s41).

**Decision for a future replicated deployment: CP for editorial state.**
- If the app server cannot reach the primary database during a partition, approve, reject, schedule and edit must **fail** (409 or 503). They must not be accepted on a replica that might be stale.
- Business reason: publishing a story that was rejected or never reviewed is a legal and reputation risk for a news organisation, and it breaks the rule that admin approval comes before publication. A delayed approval costs only a few minutes.

**AP for the reading side, with one limit.**
- Article pages, search, recommendations, view counts and reading history may come from a replica or a cache that is a little stale.
- **The limit:** staleness may delay a *new* story, but it must not keep a *withdrawn* story readable.

**How the code keeps that limit today (checked in this revision):**

| Read path | How fresh | Evidence |
|---|---|---|
| Article page, API by id, search, latest list | Read from the database on every request; status checked | `AppTests.A_withdrawn_story_disappears_from_every_public_read_at_once`; smoke "withdrawal" checks |
| Cached lists (trending, section counts, popular tags, recommendations; 2 min) | Cleared after every publish, edit, delete and schedule change. A list built while the cache was being cleared is stored under the old key and never served. | `MemoryAppCacheTests` (2) |
| RSS (`Cache-Control` 900 s) and sitemaps (3,600 s / 900 s) | The server builds them fresh each time. A browser or proxy may keep its own copy for up to 15 or 60 minutes. That copy can show a withdrawn *title and link*; the link itself then answers 404. | `SeoController` `[ResponseCache]`; accepted and documented (`15` R-10) |
| Never-approved stories | Never enter any list, feed or cache: every list filters on Published | `AppTests.Author_submission_stays_hidden_…` |

**Limits of this answer.** It is a design decision. No replica, partition test or failover exists in this repository. With 2+ app instances, the in-process cache would no longer be cleared on every instance (`15` R-1).

## Dependency risk: AutoMapper advisory

| Item | Fact |
|---|---|
| Advisory | GHSA-rvv3-g6hj-g44x / CVE-2026-32933, High (CVSS 7.5): uncontrolled recursion. Mapping a very deeply nested object graph (the proof of concept uses 30,000 levels) overflows the stack and ends the process. |
| Affected | AutoMapper < 15.1.1, and 16.0.0 – 16.1.0. The project uses **14.0.0** (`BLL/BLL.csproj:10`). |
| Fixed in | 15.1.1 and 16.1.1. The fix applies a default `MaxDepth` to every map. |
| Exposure here | Every map in `BLL/Mapping/MappingProfile.cs` already sets `MaxDepth(3)`, which is the same protection the fix adds by default. The input DTOs that users can send are flat. ASP.NET Core's JSON reader also stops at 64 levels by default. |
| Test added | `Security/MappingDepthTests.Every_mapping_has_a_depth_limit` fails if any future map is added without a depth limit. |
| Why not upgraded now | AutoMapper 15+ is sold under a commercial licence (Lucky Penny Software). It has a free Community licence for organisations under US$5M revenue, but not for universities using it for institutional software. A major-version change is also a behaviour change that needs its own testing. |
| Upgrade paths | (1) AutoMapper 16.1.1 with a Community licence key, after the group confirms it qualifies. (2) Replace AutoMapper with hand-written mapping methods; 13 type maps (9 `CreateMap` calls, 4 of them with `ReverseMap`), covered by the existing service tests. Either one should be done in its own branch with the full test suite. |
| Status | Mitigated and guarded by a test; advisory still reported by `dotnet list package --vulnerable`. |

## Mini-ATAM (W8 s21–23, full-course record only)

| Step | Result |
|---|---|
| 1 Present | Modular monolith (layered + MVC), one SQL Server, in-memory caches and stores, background jobs inside the web process. |
| 2 Approaches | Compile-time layering; optimistic concurrency on status and edits; atomic scheduled publish; word index; cache-aside with TTL and version keys; background queue; rate limits and lockout. |
| 3 Utility tree | Security (unreviewed news must never leak) > Reliability (no lost decision or edit) > Performance (search) > Availability > Scalability. |
| 4 Sensitivity points | Listing-cache lifetime (2 min) against freshness; the cache is cleared on every change, so only multi-instance runs would feel it. Lockout window (15 min) against convenience. |
| 4 Trade-off points | **In-memory state:** fast and simple, but blocks horizontal scaling and is lost on restart. **Background job inside the web process:** simple, but a crash stops scheduled publishing until restart. |
| 5 Risks | R1 one app instance is a single point of failure. R2 in-memory stores prevent scale-out. R3 concurrent latency is about 9× the single-user latency (sync EF calls are the likely cause; within target). R4 RSS and sitemaps may be cached by others for up to 1 h. |
| 5 Non-risks | Layer direction (compiler); double publication (atomic UPDATE); lost edits (version + token); SQL injection (parameterised EF). |
| 6 Results | Before any production launch: move session, lockouts, reset codes and cache invalidation to a shared store (ADR-005); measure async EF against the 20-client benchmark. |
