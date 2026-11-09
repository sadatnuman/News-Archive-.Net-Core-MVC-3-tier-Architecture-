# SADP Handoff for ChatGPT: The Meridian Newsfolio (AIUB CSC 4273)

Written for an independent reviewer who has not seen the terminal session.

- Every claim gives a path (relative to `NewsArchiveSolution/` unless it says otherwise), a line number, a test or a command result.
- Supporting files are in `sadp_delivery/`.

## A. Sources read

**Location.** All sources were in `C:\Users\User\Downloads\SADP` (outside the repo; nothing from them is committed).

**Read in full:**
- **PDFs:**
  - P1 `CSC4273_Semester_Project_Proposal_Template.pdf` (6 pages).
  - P2 `CSC4273_MidTerm_CaseStudy_Guideline.pdf` (5 pages).
- **Decks, slide counts:** Week 1 (45), 2 (33), 3 (34), 4 (31), 5 (30), 6 (24), 8 (25), 9 (25), 10 (22), 11 (23), 12 (19), 13 (19).

**Method:**
- `.pptx` files were opened with the Python standard library: slide text, text in shapes, tables and notes.
- Every course diagram is drawn with shapes, so its text was captured.
- The only embedded image in every deck is the AIUB logo (checked).
- No speaker notes have content.

**Notes on the source set:**
- The Week 2 deck was missing at first; the user added it mid-task and it was read in full.
- **Week 7 was not supplied.** W6 s24 says Week 7 is the mid-term exam. No Week 7 requirement was invented.

**Priority order:** official PDFs > W1–W3 (midterm) > later decks > code > general knowledge.

**Discrepancies (full list in `00_SOURCE_REGISTER.md`):**
- P2 gives no page count for the case study; only the proposal has 5–8 pages (P1 p3).
- W8 s9 classes a single-node RDBMS as "CA", while W1 s42 calls PostgreSQL "CP". This is handled in the CAP answer.

## B. Repository identity and safety

| Item | Value |
|---|---|
| Repo | `E:\ASP NET CORE\News-Archive-.Net-Core-MVC-3-tier-Architecture-` (remote `sadatnuman/News-Archive-.Net-Core-MVC-3-tier-Architecture-`, unchanged) |
| Baseline | branch `develop`, HEAD `c9f08a3a54b9b300a52298f1f8a6cc5478e7c3a3`, clean tree |
| Work branch | `sadp/midterm-readiness` (local) |
| Code + docs commit | `b33a3d759b1e48c7a0487dfd288b80b1dfa54cc3` |
| This handoff | committed in the next local commit on the same branch |
| Pushed or merged | **NO** |
| Change size (code + ARCHITECTURE/README) | 17 files, +1,655 / −85 lines (about 1,050 lines are the EF-generated migration designer) |

**Safety:**
- No deployment, and no email sent: SMTP host is empty, so mail goes to an outbox file.
- No destructive migration. The only new one, `20261109095936_ArticleStatusConcurrencyToken`, has empty Up/Down.
- The user's database `NewsArchiveDB` was **not touched** (28 articles before and after).
- The sample-data script ran only on the disposable `NewsArchiveSmokeDB`.
- No secrets were added. The sample password is pre-existing in the dev settings and README; it is not repeated here.

## C. Technology and architecture facts

**Stack:**
- ASP.NET Core MVC + Web API on .NET 10 (SDK 10.0.403).
- EF Core 10.0.14 on SQL Server (2025 Express locally; 2022 container in CI).
- AutoMapper 14 (known advisory, suppressed earlier with `MaxDepth(3)`), Markdig, Swashbuckle.
- xUnit tests.

**Solution:** `NewsArchive` (UI) → `BLL` → `DAL`, plus `NewsArchive.Tests`. `BLL/BLL.csproj:14` references the DAL with `PrivateAssets="compile"`, so the UI cannot compile against DAL types.

**Checked dependency rules (grep, each returned no match):**
- UI → DAL.
- DAL → BLL.
- ASP.NET or `HttpContext` types in the BLL.
- `DbContext` in controllers.

**BLL-owned abstractions implemented by the UI (DIP):**

| BLL interface | UI implementation |
|---|---|
| `IEmailSender` (`BLL/Email/IEmailSender.cs:3`) | `EmailSender` subclasses |
| `IFileStorage` | `WebRootFileStorage` |
| `IActorContext` | `HttpActorContext` |
| `IAppCache` | `MemoryAppCache` |

**Runtime:**
- One process: Kestrel serving MVC + API, plus the hosted services `ScheduledPublisher` (every 30 s) and `DigestSender` (queue).
- SQL Server is the only separate process.
- Layered ≠ physical N-tier.

**Diagrams:**
- `sadp_delivery/09_C4_AND_4PLUS1/C4_DIAGRAMS.md`: C1, C2, C3 in Mermaid.
- `sadp_delivery/09_C4_AND_4PLUS1/4PLUS1_VIEWS.md`.
- Not rendered to images: no renderer is installed, and nothing was installed.

## D. Features and roles (all verified in the code; details in `02`)

Roles: Admin = 1, Reader = 2, Author = 3.

| FR | Feature | Key code | Status |
|---|---|---|---|
| FR-1 | Author drafts or submits an article | `Controllers/Api/ArticlesApiController.cs:113`; `BLL/Services/ArticleService.cs:54`; `BLL/Builders/ArticleBuilder.cs:76` | Test + runtime |
| FR-2 | Admin approves, schedules or rejects with a note | `ArticlesApiController.cs:193` and `:223`; `ArticleService.cs:150` and `:208` | Test + runtime |
| FR-3 | Public only after approval; the scheduler publishes due stories | `BLL/Rules/ArticleRules.cs:17`; `DAL/Repos/ArticleRepo.cs:259` | Runtime (published 85 s after a +75 s schedule) |
| FR-4 | Bilingual word search, archive, sections, topics, authors | `DAL/Repos/ArticleRepo.cs:101`; `BLL/Rules/SearchRules.cs:53` | Test + runtime |
| FR-5 | Recommendations with a reason; reading history | `BLL/Services/RecommendationService.cs:89` and `:157` | Unit tests |
| FR-6 | Comments, reports, auto-hide at 3, admin moderation | `BLL/Services/CommentService.cs:85`; `ModerationRules.cs:7` | Unit tests |
| FR-7 | Double opt-in newsletter; background digest | `BLL/Services/NewsletterService.cs:37` and `:161`; `NewsArchive/Email/DigestSender.cs:25` | Tests + runtime (one outbox file) |
| FR-8 | Register, sign in, lockout, password change and reset | `BLL/Services/AuthService.cs:27`; `NewsArchive/Auth/*` | Tests |
| FR-9 | Bookmarks, history page | `BookmarkController.cs:34`, `HistoryController.cs:24` | Code only (no committed test) |
| FR-10 | Audit log | `BLL/Services/AuditService.cs:9` | Unit tests |
| FR-11 | English and Bangla UI and content | `BLL/AppRoles.cs:27` | Runtime (`/?lang=bn` 200) |
| FR-12 | Token-protected REST API + Swagger | `Controllers/Api/*` | Tests + runtime 401/403/200 |

## E. Official requirement matrices

**Midterm (P2), file `04`:**

| § | Status | Note |
|---|---|---|
| §0 | PARTIAL | Names and IDs are TBD |
| §1, §2, §3, §5, §6, §7, §8, §10 | READY | — |
| §4 | PARTIAL | The figure must be rendered |
| §9 | READY | One viva risk: M2's Singleton is DI-managed |

- The draft report is `sadp_delivery/MIDTERM_DRAFT_FOR_REVIEW.md` and follows the official template.
- It uses **only Lectures 1–3** content to answer Q1–Q9.

**Proposal (P1), file `05`:** all 12 sections are mapped.
- Indicative self-rating (not a grade prediction): **20/24**.
- Lowest criterion: team plan, 1/3, because names are TBD.
- Format: PDF or DOCX, 5–8 pages, via Teams, one per group.

## F. Course coverage, Weeks 1–6 and 8–13

Full table in `06`. Week 7 is absent (exam week).

| Week | Status |
|---|---|
| W1 | SRP / OCP / LSP / ISP / DIP all evidenced. One ISP violation recorded: `IArticleService` has about 30 members; `IDataAccessFactory` exposes 11 repositories. 4+1, six attributes and CAP documented. |
| W2 | Layered and MVC verified. Composition root is `Program.cs`. C4 L1–L3 drawn. Monolith decision made. Smells: `AdminController` with 12 dependencies, `ArticleController.Details` at 55 lines. |
| W3 | Builder and Factory Method **implemented in this task**. Abstract Factory **verified + new test**. Singleton **verified as a DI-scoped singleton** (not the classic form). |
| W4 / W5 | Not added to the code on purpose. Adapter-like transports noted. Facade, State and Strategy proposed with triggers. Composite N/A. Decorator not claimed (framework middleware). |
| W6 | Gateway, EDA, Circuit Breaker and Saga are N/A for one deployable. An in-process `Channel` is **not** EDA; one SQL transaction is **not** a Saga. |
| W8 | Scaling analysis and mini-ATAM done. Sharding and load balancing N/A until more than one instance exists. Resilience partial: SMTP timeout; no retry, to avoid double sends. |
| W9 | p50 / p95 / p99 measured. Cache-aside with TTL verified. **API tokens are opaque random values stored as hashes, not JWT; no OAuth2.** Observability partial. ADRs written (`10_ADRS`, Week 9 milestone, not midterm). |
| W10 / W11 / W13 | N/A or future, with need → benefit → cost → risk reasoning: no blockchain, IoT or AI agent added; serverless only as a future option for the digest; blue-green documented, not deployed. |
| W12 | C4 as code, golden-thread table (`14`), research proposal seed (`14`). |

## G. Proposed member split (`13`)

All names and IDs are **TBD**. The GitHub owner `sadatnuman` must be confirmed by the group.

| Member | Module | NFA | SOLID | Creational pattern |
|---|---|---|---|---|
| M1 | Editorial workflow | Reliability | SRP | Builder |
| M2 | Accounts & security | Security | DIP | Singleton (DI-managed) |
| M3 | Newsletter & email | Availability | OCP | Factory Method |
| M4 | Discovery | Performance | ISP | Abstract Factory |

Variants for 2, 3 and 5 members are in `13`.

## H. Patterns and principles with GoF roles (`07`)

**Builder.**
- Builder `ArticleBuilder` (`BLL/Builders/ArticleBuilder.cs:5`) with fluent steps and `Build()` (`:76`), which validates author, section and text, allows "breaking" only when approved, and decides Draft / Pending / Published / Scheduled.
- Product `Article`.
- Director role: `ArticleService.Create` (`ArticleService.cs:60`).
- Tests: `Patterns/ArticleBuilderTests` (7).

**Factory Method.**
- Creator `EmailSender` (`NewsArchive/Email/EmailSender.cs:11`); business logic in `SendAsync` (`:22`); factory method `CreateTransport()` (`:20`).
- Concrete creators `SmtpEmailSender` and `OutboxEmailSender`.
- Product `IMailTransport`; concrete products `SmtpTransport` (new `SmtpClient` per message) and `OutboxTransport`.
- The creator is chosen by configuration in `Program.cs:112–114`.
- Tests: `Patterns/EmailSenderTests` (5), including an LSP contract test across two creators.
- This is **not** a simple `if/else` factory.

**Abstract Factory.**
- `IDataAccessFactory` (`DAL/Interfaces/IDataAccessFactory.cs:5`) with two families: SQL `DataAccessFactory` and the test `FakeFactory` (`NewsArchive.Tests/Support/TestServices.cs:52`).
- Products: 11 repository interfaces.
- Family guarantee: one shared `DbContext`. Proven by `AppTests.Repositories_from_one_factory_share_one_unit_of_work`: the section from the category repository is the same object as `article.Category`; another scope gets a different object.
- The class *name* was not taken as proof.

**Singleton.**
- `LoginThrottle` and `PasswordResetStore` registered with `AddSingleton` (`Program.cs:52`, `:118`).
- Test: `Assert.Same` across two scopes.
- This is a **DI-managed single instance**, which W3 s11 calls the "DI-scoped singleton", and W3 s31 accepts as "Singleton (or a DI-scoped singleton)".
- **No class has the classic private constructor + static instance.** That was deliberate (global state, hard to test).
- `SiteClock` is a static helper, not claimed as a Singleton.

**SOLID evidence:**

| Principle | Evidence |
|---|---|
| SRP | Builder vs service vs rules |
| OCP | A new email channel is a new subclass |
| LSP | Contract test across two senders; fake factory family |
| ISP | `RecommendationService` uses 2 repository contracts (caveat: the factory exposes 11) |
| DIP | `PasswordChanger` → `IAuthService` (`NewsArchive/Auth/PasswordChanger.cs:15`) |

**Layered vs N-tier.** The three layers are in one process; the C4 L2 shows one web container plus the database.

## I. Quality attributes, CAP, migration (`08`, draft §7–8)

| Attribute | Target | Evidence | Status |
|---|---|---|---|
| Reliability | 1 stored decision per race; scheduled story live once, ≤ 60 s late | Race test; smoke 409; live after 85 s with +75 s schedule | Tested / measured |
| Security | 0 unpublished stories public; lockout after 5 failures per email+IP for 15 min; antiforgery on all POSTs; PBKDF2 100k | `SecurityTests`, `AppTests` | Tested |
| Availability | Mail failure → 0 failed pages; 99.5 % uptime | `EmailSenderTests` | Mail part tested; uptime not measured (no deployment, single instance) |
| Performance | Search p95 ≤ 200 ms for 1 user, ≤ 1 s for 20 users | Measured 51.2 ms and 621.9 ms (p99 762.0 ms, 44.7 req/s) | Met on a laptop with 31 stories; indicative only |
| Scalability | 20 users at ≥ 40 req/s on 1 instance | Measured | Scale-out blocked by in-memory state (ADR-005, proposed) |
| Maintainability | 0 reverse dependencies; 108 tests in CI | grep checks, build | Verified |

**CAP.**
- Today there is one SQL Server node: no replicas, so no partition to tolerate. ACID and the optimistic check protect the data, and ACID ≠ CAP consistency.
- **Design decision for replication:** editorial status is **CP** (refuse approve, reject or schedule during a partition rather than risk publishing unreviewed news); reading is **AP** (stale pages, search and counts are acceptable).
- This is a decision, not a deployed distributed system.

**Migration plan (plan only):**
1. Modular monolith.
2. Shared state store + 2 instances.
3. Extract **Newsletter & Notifications** first: it owns `Subscribers`, only reads stories, has the burstiest load, and does not affect the CP core.
4. Search & Recommendations.
5. Editorial + Accounts stay as the core.

**Mini-ATAM.** Key risks: single instance; in-memory state; about 12× concurrent slowdown; edit form without a staleness check.

## J. Command results (`11`)

| Command | Result |
|---|---|
| Baseline restore / build / test | exit 0 / 0 (0 warnings, 0 errors) / 0, **92/92** |
| Mid-task test run 1 | 1 failure → found a real bug: `Normalize("BN")` returned `en` (fixed) |
| Mid-task test run 2 | 1 failure → the test used a no-tracking repository (fixed) |
| Final build | exit 0, 0 warnings, 0 errors |
| Final test | exit 0, **108 passed, 0 failed, 0 skipped** |
| `git diff --check` | exit 0 |
| Vulnerable packages | only AutoMapper 14.0.0 (High, pre-existing, suppressed earlier) |
| Integration database | `NewsArchiveTestDB` on `localhost\SQLEXPRESS` (CI: SQL Server 2022 container) |

**Smoke run:** app on `http://127.0.0.1:5199` with `NewsArchiveSmokeDB`, **32/32 checks**:
- health, pages (EN/BN), RSS, sitemap, Swagger, 3 demo accounts;
- 302 / 401 / 403 barriers;
- author → submit → hidden → admin approve → public → second decision 409 → found by search;
- reject with note visible to the author;
- schedule +75 s → published by the job;
- subscribe → exactly one outbox file.

**Benchmark (sequential p50 / p95 / p99):**

| Page | p50 | p95 | p99 |
|---|---|---|---|
| Home | 37.7 ms | 83.4 ms | 88.1 ms |
| Search | 33.5 ms | 51.2 ms | 67.6 ms |
| Article | 33.2 ms | 50.1 ms | 51.4 ms |
| Archive | 33.4 ms | 50.5 ms | 55.7 ms |
| Search, 20 concurrent | 424.0 ms | 621.9 ms | 762.0 ms (44.7 req/s, 0 errors) |

The server was stopped afterwards. The user's Visual Studio app was not touched.

## K. Security and safety

**Verified:**
- Role filters (401/403).
- Unpublished stories hidden (404).
- Antiforgery (400 without a token).
- CSP and security headers.
- Lockout per email+IP.
- Hashed reset codes.
- Markdown cannot inject script.
- No raw SQL anywhere: `FromSqlRaw` / `ExecuteSqlRaw` grep returns 0.
- Each reader can report a comment only once: primary key `(CommentId, UserId)`.

**Not verified:** HTTPS on a real host, real SMTP, multi-instance behaviour.

**Bugs found and fixed:**
- F1: case-sensitive language code.
- F2: approve / reject race.

**Bugs found and open:**
- F3: an edit form opened long ago can overwrite a newer edit (P2).
- F4: Production without SMTP logs mail but returns success (by design, in the README go-live list).

## L. Change traceability (`12`)

| # | Requirement | Change | Location | Test | Result |
|---|---|---|---|---|---|
| C1 | §9 Builder | New `ArticleBuilder`; `ArticleService.Create` uses it | `BLL/Builders/ArticleBuilder.cs`; `ArticleService.cs:60` | 7 tests | Pass |
| C2 | §9 Factory Method + OCP | `EmailSender` creator + transports | `NewsArchive/Email/EmailSender.cs:20`; `SmtpEmailSender.cs:20`; `OutboxEmailSender.cs:22` | 5 tests | Pass |
| C3 | §3 Reliability | `StatusId` concurrency token; conflict returns `false` | `DAL/EF/NewsArchiveDbContext.cs:91`; `DAL/Repos/Repository.cs:47`; empty migration | Race test | Pass |
| C4 | F1 | Case-insensitive language code | `BLL/AppRoles.cs:27` | Builder test | Pass |
| C5 | Pattern evidence | Abstract Factory and Singleton tests | `Integration/AppTests.cs` | 2 tests | Pass |
| C6 | Rate limits in tests | Test-only client-IP startup filter | `Integration/AppFactory.cs` | — | Production code unaffected |
| C7 | Docs | ARCHITECTURE and README updated | — | — | — |

**Rollback:** `git checkout develop`. The branch is local, and the migration is a no-op.

## M. Open defects, in priority order (`15`)

| ID | Priority | Item | Needs |
|---|---|---|---|
| R-T1 | P0 | Fill in member names, IDs, group, section, date | User |
| R-T2 | P0 | Render C4 L1 (report) and L2 (proposal) to images | Tool choice |
| R-V1 | P0, viva risk | Confirm the lecturer accepts a DI-managed Singleton for M2; otherwise M2 needs a different pattern or a classic Singleton | Lecturer |
| R-1 | P1 | Shared state store before 2+ instances | — |
| R-2 | P1 | Clear "already decided by someone else" message on a race | — |
| R-3 | P1 | Benchmark with 5k–10k stories; try async EF | — |
| R-4 | P1 | Production deployment + go-live list | Host and credentials |
| R-5 – R-9 | P2 | Edit-form staleness check, split `IArticleService` / `AdminController`, inject `TimeProvider`, AutoMapper advisory, sample password | — |

**Ready-to-send follow-up prompt for Claude Code:**

```text
On branch sadp/midterm-readiness (local only, do not push):
1. R-2: when ArticleService.Approve/Reject/PublishNow/Unschedule fails because the status changed
   since load (concurrency conflict), MVC flash and API ProblemDetails must say
   "This story was already decided by someone else", distinct from "not pending". Extend
   AppTests.Two_reviewers_cannot_both_decide_the_same_article through the API.
2. R-5: optimistic check for the article edit form (UpdatedAt hidden field compared in
   ArticleService.Update); add a test with two edits from the same stale form.
3. R-3: generate 5,000 stories in NewsArchiveSmokeDB only, rerun the benchmark, report p50/p95/p99.
Rebuild, run all tests, update sadp_delivery/11, 12, 15 and this handoff. Never touch NewsArchiveDB.
```

## N. Verdict

- **MIDTERM CODE/EVIDENCE: PARTIAL.**
  - Every section has real code, test or measurement evidence, and the draft report exists.
  - It is not READY only because of R-T1 (team names and IDs) and R-T2 (a rendered C4 figure), plus the R-V1 lecturer check.
- **FULL COURSE READINESS: PARTIAL.**
  - The week-by-week matrix is complete for all 12 supplied decks.
  - The Week 9 ADRs and the Week 12 diagrams-as-code exist.
  - Week 4–5 patterns are proposed, not built (on purpose).
  - Weeks 6, 10, 11 and 13 are mostly NOT APPLICABLE BY TOPIC, with reasons.
  - There is no deployment.

**Acceptance checks failed or not attempted:**
- Rendered diagrams.
- Real host, HTTPS and SMTP.
- 2-instance test.
- Large-data benchmark.
- Viva rehearsal.
- No full marks are guaranteed.

## O. What ChatGPT still needs from the user

The group's member IDs and names, group number, section and submission date; which member owns which module; whether the lecturer accepts a DI-managed Singleton; and, if a live demo is wanted, a chosen host with its credentials.
