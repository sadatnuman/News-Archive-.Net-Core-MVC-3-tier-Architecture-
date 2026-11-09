# Midterm Requirement Traceability (P2 §0–10, Lectures 1–3 only)

**Source.** `CSC4273_MidTerm_CaseStudy_Guideline.pdf` (5 pages), re-read on 2026-11-09.

**Scope.** It covers Lectures 1–3 only. Later-week material is out of scope "behavioural/structural patterns, EDA, circuit breakers, sagas, observability, blockchain, IoT, AI agents" (P2 p1). The guideline gives no page limit; the 5–8 pages belong to the Proposal (P1 p3) and are not applied here.

Paths are relative to `NewsArchiveSolution/` on branch `sadp/final-hardening`.

## Section by section

| § | Owner | Requirement (P2) | Evidence in code / tests | Draft section | Readiness |
|---|---|---|---|---|---|
| Cover | Group | Topic, group & section, members (ID — Name), date, CO1 | Topic and outcome filled | Draft cover | **PARTIAL**: group, section, IDs, names, date are [TBD] |
| 0 | Group | Two-sentence recap; table member → feature → NFA → SOLID + pattern | 4 modules with real code (see the member map below) | §0 | **PARTIAL**: names [TBD]; the group must confirm ownership and group size |
| 1 | Group | 2–3 architectural + 2–3 design decisions, and why | A1 layering (`BLL/BLL.csproj:14`), A2 SQL Server + EF migrations, A3 cookie + opaque-token identity (`BLL/Services/AuthService.cs:101`); D1 Builder, D2 Factory Method, D3 tokenizer + cache lifetime | §1 | READY |
| 2 | Group | Style (layered / MVC / client–server), monolith vs SOA, drivers | Compile-time layering; one deployable; layers ≠ tiers | §2 | READY |
| 3 | Individual | ≥ 1 attribute each, measurable target, tactic, name | M1 Reliability (`NewsArchiveDbContext.cs:91-92`, `ArticleService.cs:89`); M2 Security (`LoginThrottle.cs:41-47`); M3 Availability, offline failure isolation (`Program.cs:111-115`, `EmailSender.cs`); M4 Performance (`ArticleRepo.Search`) | §3 | READY (code side); names [TBD] |
| 4 | Group | C4 L1 or L2 + two lines why | `09_C4_AND_4PLUS1/c4_level1_context.png` (rendered, checked by eye; SMTP removed because no mail provider exists) | §4, Figure 1 | READY |
| 5 | Group | Logical, process, development, physical, scenarios | `09_C4_AND_4PLUS1/4PLUS1_VIEWS.md`; process wording corrected (thread pool, no thread per request); S1–S6 traced to tests | §5 | READY |
| 6 | Individual | One SOLID principle per member, own feature, why | SRP (builder / service / rules), DIP (`PasswordChanger` → `IAuthService`), OCP (`EmailSender` subclasses), ISP (`BLL/Interfaces/IArticleArchive.cs`, 3 members; test `AppTests.Archive_pages_depend_only_on_the_narrow_archive_contract`) | §6 | READY |
| 7 | Group | CP or AP for critical data, business reason | Single SQL Server today (no partition, ACID ≠ CAP); planned CP for editorial state, AP reads with "withdrawn must vanish" (test `A_withdrawn_story_disappears…`) | §7 | READY |
| 8 | Group | Monolith → microservices path, first service, why | Newsletter & Notifications first; 5 phases; a plan, not an implementation | §8 | READY |
| 9 | Individual | One creational pattern per member, justified | Builder `BLL/Builders/ArticleBuilder.cs` (7 tests); Singleton, DI-managed (`Program.cs` `AddSingleton<LoginThrottle>`, `AddSingleton<PasswordResetStore>`; `Assert.Same` test); Factory Method `NewsArchive/Email/EmailSender.cs` (5 tests); Abstract Factory `DAL/Interfaces/IDataAccessFactory.cs` (family test) | §9 | READY |
| 10 | Group | IEEE references | 12 entries with in-text [n]; [4], [5], [7] checked on Crossref; [8], [10], [11] opened | §10 | READY (4 books to check against the library copies) |

## Member map (tentative; the group must confirm)

| Member | Feature (code) | NFA (§3) | SOLID (§6) | Pattern (§9) | Demo / test command |
|---|---|---|---|---|---|
| M1 [TBD] | Editorial: `ArticleService`, `ArticleBuilder`, `ArticleRules` | Reliability | SRP | Builder | `dotnet test --filter "FullyQualifiedName~ArticleBuilderTests\|FullyQualifiedName~Two_reviewers\|FullyQualifiedName~stale_form"` |
| M2 [TBD] | Accounts & security: `AuthService`, `LoginThrottle`, `PasswordResetStore`, `PasswordChanger` | Security | DIP | Singleton (DI-managed) | `dotnet test --filter "FullyQualifiedName~SecurityTests\|FullyQualifiedName~Lockout_and_reset_stores\|FullyQualifiedName~Twelve_parallel"` |
| M3 [TBD] | Newsletter & email (offline): `NewsletterService`, `EmailSender` family, `DigestSender` | Availability (failure isolation) | OCP | Factory Method | `dotnet test --filter "FullyQualifiedName~EmailSenderTests\|FullyQualifiedName~mail_is_disabled\|FullyQualifiedName~double_opt_in"` |
| M4 [TBD] | Discovery: search, archive (`IArticleArchive`), recommendations | Performance | ISP | Abstract Factory | `dotnet test --filter "FullyQualifiedName~Archive_pages_depend\|FullyQualifiedName~Repositories_from_one_factory"`; `python tools/smoke/smoke.py --stories 5000` |

All filter commands run from `NewsArchiveSolution/` with `NewsArchive.Tests -c Release`.

## Boundary check

- The draft answers Q1–Q9 with Lecture 1–3 concepts only.
- Microsoft documentation [8], [10], [11] supports facts about our own code; it does not introduce new course concepts.
- The `DigestSender` queue is described as an availability tactic, not as EDA.
- Latency percentiles make the §3 target measurable; the draft does not cite Week 9 for them.
- Not used in the draft: ADRs (Week 9), the ATAM table (Week 8), the later patterns in `07`.

## Verdict

- **MIDTERM_CODE: PASS.** Every section has genuine code, test, figure or measurement evidence within Lectures 1–3.
- **MIDTERM_SUBMISSION: PARTIAL.** It needs members, IDs, group, section and date, plus conversion of the Markdown draft to the submission format with Figure 1.
