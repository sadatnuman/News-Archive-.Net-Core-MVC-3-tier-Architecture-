# SADP Handoff for ChatGPT: The Meridian Newsfolio (final hardening)

Written for an independent reviewer. The block below is identical to `CLAUDE_FINAL_COPY_TO_CHATGPT.txt`; detailed evidence is in `SECURITY_REVIEW_FINDINGS_AND_FIXES.md`, `TEST_MATRIX_AND_COMMANDS.md`, `EMAIL_OFFLINE_MODE_AND_TESTS.md`, `MIDTERM_REQUIREMENT_TRACEABILITY.md`, `FULL_COURSE_APPLICABILITY_MATRIX.md` and `FINAL_CHANGELOG.md`.

```text
THE MERIDIAN NEWSFOLIO - FINAL SADP HANDOFF FOR INDEPENDENT REVIEW (AIUB CSC 4273)
Plain-text copy for direct pasting. Same facts as sadp_delivery/SADP_HANDOFF_FOR_CHATGPT.md.
Paths are relative to NewsArchiveSolution/ unless stated. Line numbers are final on this branch.

=====================================================================
A. REPOSITORY, BRANCH, SAFETY
=====================================================================
Repository : github.com/sadatnuman/News-Archive-.Net-Core-MVC-3-tier-Architecture-
Base       : main @ 12d1893913787c12ff429f4b73809426c9d6d73f (PR #10; 116/116 tests, build 0 warnings / 0 errors)
Branch     : sadp/final-hardening (local only)
Commit 1   : c1dfb82ef1985b89732f57cd5c8d32cf89b22b0a  (code, tests, docs)
Commit 2   : the next commit on the same branch          (this handoff, changelog)
Pushed / merged / deployed : NO / NO / NO
GitHub CI on the new commits : NOT RUN (not pushed). Local tests do not replace it.

Size:
- Code + README/ARCHITECTURE + tools: 47 files, +1,147 / -149.
- No EF migration added: "dotnet ef migrations has-pending-model-changes" reports no changes.

User database NewsArchiveDB: NOT TOUCHED.
- Checked with sqlcmd: 28 articles.
- Last applied migration: 20261109091901_AddArticleWords.
- Only NewsArchiveTestDB (integration tests) and NewsArchiveSmokeDB (smoke/benchmark) were used.

NO REAL EMAIL:
- Real email delivery is not implemented/configured or tested.
- Email verification uses a local outbox/fake transport only.
- No SMTP connection, no provider, no credentials, no DNS/MX, no real recipients (synthetic @example.test only).
- Proof is in section E.

Course sources read: C:\Users\User\Downloads\SADP
- CSC4273_MidTerm_CaseStudy_Guideline.pdf (5 pages).
- CSC4273_Semester_Project_Proposal_Template.pdf (6 pages).
- 12 decks: Weeks 01-06 and 08-13.
- Week 07 was not supplied (W6 s24: exam week); nothing was invented for it.

=====================================================================
B. ARCHITECTURE AND FEATURES (verified in code)
=====================================================================
Layers:
- One ASP.NET Core 10 deployable: NewsArchive (MVC + REST API + Razor) -> BLL -> DAL (EF Core 10, SQL Server).
- BLL references DAL with PrivateAssets=compile (BLL/BLL.csproj:14), so the UI cannot compile against DAL types.
- Layers are not physical tiers: one process plus the database.

Roles: Admin, Author, Reader.

Features:
- Author drafts/submits; admin approves, schedules or rejects with a note; a background job publishes due stories.
- Bilingual EN/BN word search, date archive, sections, topics, authors, recommendations, bookmarks, history.
- Comments with reports and auto-hide at 3, admin moderation, audit log.
- Newsletter with double opt-in (offline only).
- Token-protected REST API with Swagger.

=====================================================================
C. P0 / P1 FINDINGS - BEFORE -> AFTER -> TEST
=====================================================================
S1 (P0) An author could delete other users' pictures. FIXED.
  Before:
    MediaService.DeleteIfUploaded deleted any "/uploads/..." path without "..".
    Typed ImagePath values reached it, so an author could put another user's avatar or story picture
    path into their own story, then clear it or delete the story.
  After:
    BLL/Services/MediaService.cs:41 DeleteArticleImageIfUnused accepts only
      ^/uploads/articles/[0-9]{4}/[0-9]{2}/[0-9a-f]{32}\.(jpg|png|gif|webp)$  (:24).
    BLL/Services/MediaService.cs:48 DeleteAvatarIfUnused accepts only
      ^/uploads/avatars/[0-9a-f]{32}\.(...)$  (:27).
    Both refuse when Articles.ImagePath (DAL/Repos/ArticleRepo.cs:109) or Users.AvatarPath
    (DAL/Repos/UserRepo.cs:64) still uses the path.
    Callers: ArticleController.cs:233,270,285,314; Api/ArticlesApiController.cs:183;
    Api/AuthApiController.cs:103,107,118; AvatarController.cs:47,52,67.
  Tests (real files in an isolated temporary web root):
    HardeningTests.Deleting_one_story_never_removes_a_picture_another_story_still_uses
    HardeningTests.A_story_pointing_at_someone_elses_avatar_never_deletes_it
    HardeningTests.Only_canonical_unused_upload_paths_can_ever_be_deleted (13 unsafe shapes refused)
  Mutation check: with the reference check removed, the first test fails.
  Residual: a millisecond check-then-delete gap at one server (R-13).

S2 (P0) Parallel sign-in requests bypassed the 5-per-15-minutes lockout. FIXED.
  Before:
    IsLocked() was checked first; RecordFailure() then read the count and wrote count+1 without a lock.
    20 parallel guesses all passed.
  After:
    NewsArchive/Auth/LoginThrottle.cs:41 Attempt(email, verify).
    Under a lock it refuses when failures + inFlight >= 5 (:47) and reserves a slot.
    The password is checked outside the lock; the result is recorded under the lock.
    Used by AuthController.cs:53 (website), Api/AuthApiController.cs:38 (API),
    Auth/PasswordChanger.cs:48 (current-password check).
  Tests:
    SecurityTests.Twenty_simultaneous_wrong_passwords_get_at_most_five_checks (exactly 5 of 20)
    SecurityTests.A_correct_password_clears_earlier_failures_and_a_crash_does_not_count
    HardeningTests.Twelve_parallel_wrong_passwords_get_five_checks_and_then_both_sign_in_paths_are_locked
      (5 x 401, 7 x 429; the right password then gets 429; the MVC form shows the lock)
  Mutation check: with inFlight removed from the gate, both concurrency tests fail.
  Residual: the counters are per process; a shared store is planned (ADR-005).

C1 (P1) The MVC story form with a missing section gave a 500 and lost the text. FIXED.
  - ArticleController.cs:65 RequireSection runs before any upload (create :213, edit :262).
  - Race: ArticleRepo.Create/Update (DAL/Repos/ArticleRepo.cs:14,27) catch only SQL 547 (DAL/Repos/DbErrors.cs)
    and return false; the form returns with the text kept and only its own new upload removed.
  - Tests: HardeningTests.A_form_for_a_missing_section_keeps_the_text_and_removes_only_its_own_upload,
    Saving_into_a_section_that_vanished_returns_false_instead_of_crashing.

C2 (P1) User deletion ran in separate steps. FIXED.
  - DAL/Repos/UserRepo.cs:32-55: one transaction (:34). A foreign-key conflict -> rollback, false (:49).
  - Other failures roll back (dispose) and surface.
  - Tests: Deleting_a_user_removes_their_comments_reports_and_moderator_links_together;
    A_user_delete_that_fails_at_the_last_step_rolls_everything_back
    (a temporary trigger on the test database injects the failure; the user, comment, report and
    moderator link all remain).

C3 (P1) Two registrations for one email at once gave a 500. FIXED.
  - UserRepo.Create (:19) catches only SQL 2601/2627 and returns false -> 409 "Email already exists".
  - An outage is not hidden.
  - Tests: Two_registrations_for_one_email_at_once_create_exactly_one_account
    (1 x 201, 1 x 409, 1 row, 1 audit entry); A_duplicate_insert_returns_false_and_the_repository_keeps_working.

C4 (P1) An API edit without "version" was told "someone else changed this article". FIXED.
  - Api/ArticlesApiController.cs:146-150: missing or malformed version (ArticleRules.IsWellFormedVersion,
    BLL/Rules/ArticleRules.cs:28) -> 400 ValidationProblem on the key "version":
    "Send the version returned by GET /api/articles/{id} in the JSON property "version"."
  - Stale but well-formed -> 409. No article -> 404. Anonymous -> 401. Not allowed -> 403. Current -> 204.
  - Swagger [EndpointDescription] (:136); README curl example and answer table. The MVC hidden field is unchanged.
  - Tests: HardeningTests.The_edit_api_explains_a_missing_version_and_keeps_409_for_real_conflicts;
    AppTests.An_edit_from_a_stale_form_never_overwrites_a_newer_edit (now expects 400 for a missing version).

P2 items fixed:
  D1 Removed unused IArticleService.Get() (loads all rows), ICommentService.Get(), ICommentService.Update().
  D2 MaintenanceJob every 24 h (NewsArchive/Publishing/MaintenanceJob.cs; BLL/BusinessLayerServices.cs:51);
     test Daily_maintenance_removes_expired_tokens_and_old_audit_entries.
  D3 News sitemap is filtered and limited in SQL: GetPublishedForSitemap(since, max) (DAL/Repos/ArticleRepo.cs:308,
     SeoController.cs:131); main sitemap capped at 45,000 (:67).
  D4 Auto-hide is a conditional UPDATE (CommentRepo.cs:77, CommentService.cs:92): audited once.
  D5 A delete racing an edit returns false, not a 500 (Repository.cs:64).
  D6 Knowing an email no longer changes a confirmed subscriber's language (NewsletterService.cs:49).
  D7 RSS / news sitemap Cache-Control 900 -> 300 s (SeoController.cs:124,147).
  Each has a regression test (see TEST_MATRIX_AND_COMMANDS.md).

Deferred with reason:
  D8 AutoMapper 14 advisory GHSA-rvv3-g6hj-g44x (High).
     Every map has MaxDepth(3), guarded by MappingDepthTests.
     The fixed versions (15.1.1 / 16.1.1) are commercially licensed (free Community tier with conditions).
     Not upgraded without an owner decision.
  D9 Sitemap index beyond 45,000 stories: not reachable at the expected size.

Wider scan - no new exploitable issue found:
  - JSON-LD Html.Raw uses System.Text.Json default escaping.
  - The service worker does not cache HTML or private paths.
  - Suggestions, bookmarks and author pages filter on Published.
  - Not examined deeply: site.js beyond the service worker, analytics queries, CI file, proxy/HTTPS on a real host.

=====================================================================
D. BUILD, TESTS, SMOKE, BENCHMARK (2026-11-09, this machine)
=====================================================================
Machine: Windows 11, Ryzen 5 7520U (8 logical CPUs), 13.8 GB RAM, .NET SDK 10.0.403, SQL Server 2025 Express.

dotnet build NewsArchiveSolution.slnx -c Release     -> exit 0, 0 warnings, 0 errors
dotnet test NewsArchive.Tests -c Release --no-build  -> 137 passed, 0 failed, 0 skipped
   (116 + 17 HardeningTests + 2 SecurityTests + 1 CommentServiceTests + 1 NewsletterServiceTests)
dotnet ef migrations has-pending-model-changes       -> no changes
dotnet list ... package --vulnerable --include-transitive -> only AutoMapper 14.0.0
git diff --check                                     -> clean
python tools/smoke/smoke.py --stories 5000 (repo root) -> 45/45 checks passed, benchmark errors 0, exit 0

Smoke guards:
- Only NewsArchiveSmokeDB* / NewsArchiveTestDB* names on localhost, ".", 127.0.0.1 or (localdb) are accepted.
  9 guard cases were checked, including refusing NewsArchiveDB, look-alike names and remote servers.
- Email__*/SMTP* environment variables are removed.
- The app runs in Development on 127.0.0.1:5199.

Smoke checks:
- EN/BN pages, health, RSS, sitemap, Swagger.
- 302/401/403 barriers.
- author -> admin approve/reject/schedule; 409 for a decided story.
- API stale edit 409 and missing version 400; MVC stale form conflict.
- Offline email:
    one confirmation file in the private outbox;
    the confirm link works;
    the admin digest reaches the outbox;
    the reset code is accepted (code not printed);
    an unknown email gets the same answer and no mail.
- Withdrawal from page, API, list and RSS.
- Scheduler published a story set 75 s ahead after 85 s.

Search benchmark:
- Dataset: 5,024 published stories (5,000 generated through the real admin form; seed 4273; half EN, half BN;
  synthetic Zipf vocabulary); 915,558 word-index rows.
- Workload: 30 fixed queries on /Article/Index?keyword=; 20 warm-up requests; 400 sequential requests;
  800 requests from 20 threads; 50 requests per sitemap.

                       p50       p95       p99        throughput  errors
  search, 1 user       46.7 ms   66.9 ms   71.0 ms    21.0 req/s  0
  search, 20 users     476.8 ms  784.2 ms  1028.0 ms  38.9 req/s  0
  news sitemap         56.2 ms   80.5 ms   81.2 ms    17.6 req/s  0
  main sitemap         128.2 ms  150.4 ms  297.2 ms   7.6 req/s   0

Repeatability: three 5,000-story runs gave 20-client throughput 43.0 / 40.8 / 38.9 req/s and p95 672.5 / 727.8 / 784.2 ms.

Targets:
- search p95 <= 200 ms (1 user) and <= 1 s (20 users): MET on this machine.
- >= 40 req/s with 20 users: NOT MET in the final run (38.9).

Limits:
- client and server on one machine; synthetic text; SQL Server Express;
- no query plans captured;
- headless HTTP checks only (no visual browser test).

=====================================================================
E. OFFLINE EMAIL - CONFIGURATION AND PROOF
=====================================================================
NewsArchive/Program.cs:111-115:
- Development registers OutboxEmailSender: private App_Data/outbox under the content root, not wwwroot, gitignored.
- Every other environment registers DisabledEmailSender (NewsArchive/Email/DisabledEmailSender.cs):
  IsEnabled = false, SendAsync returns false, no transport, no per-mail log.
- One start-up warning (Program.cs:212).
- SmtpEmailSender/SmtpTransport stay as a dormant Factory Method product and are never registered,
  even with Email:Smtp:Host set. The SMTP block was removed from appsettings.json.
- The admin "Send digest" button checks DigestSender.CanSend (AdminController.cs:161; DigestSender.cs:25)
  and says email is turned off. No subscriber is marked as sent.

Proof:
- HardeningTests.Outside_development_mail_is_disabled_even_with_an_smtp_host_and_sends_nothing:
    Production + Email:Smtp:Host=smtp.example.invalid -> DisabledEmailSender is the only registration;
    2,000 sends to reader{i}@example.test all return false;
    0 new warning/error logs; exactly 1 start-up warning;
    a System.Net.Sockets EventListener sees no ConnectStart to ports 25/465/587/2525 or any "smtp" host.
- HardeningTests.Newsletter_double_opt_in_works_offline_through_the_private_outbox:
    subscribe -> one outbox file -> confirm -> Bangla re-subscribe does not change the language -> unsubscribe;
    the outbox is outside wwwroot; the test removes its files and the subscriber.
- The smoke script performs the same flows plus digest and password reset; tokens and codes are never printed.

Limit: the socket listener covers .NET-managed sockets only. The app has no other mail code.

Consequence (documented in the README): on a public host, readers cannot confirm a newsletter sign-up
or reset a password by email. This is scope, not a defect.

=====================================================================
F. SADP - MIDTERM (P2, Lectures 1-3 only)
=====================================================================
Cover / 0 Work split: PARTIAL - member IDs/names, group, section, date are [TBD] (never invented).
1 Arch vs design:  READY - A1 layering, A2 SQL Server + EF migrations, A3 cookie + opaque-token identity
                   (BLL/Services/AuthService.cs:101); D1 Builder, D2 Factory Method, D3 tokenizer + cache TTL.
2 Style:           READY - layered + MVC + client-server; modular monolith vs SOA with drivers (W2 s29-30).
3 NFA (per member):
   M1 Reliability: StatusId + UpdatedAt tokens, form version - VERIFIED; scheduler - MEASURED (85 s).
   M2 Security: parallel-safe lockout, picture-deletion safety, CSRF, PBKDF2 - VERIFIED.
   M3 Availability as offline failure isolation - VERIFIED; deliverability not measured (no real email);
      uptime NOT VERIFIED.
   M4 Performance: 1-user p95 66.9 ms, 20-user p95 784.2 ms - MEASURED.
4 C4:              READY - rendered sadp_delivery/09_C4_AND_4PLUS1/c4_level1_context.png (Figure 1; checked by eye;
                   SMTP removed because no provider exists). L2 c4_level2_containers.png is for the proposal.
5 4+1:             READY - process view corrected (thread pool, no thread per request, await releases the thread;
                   synchronous EF blocks a pool thread); scenarios S1-S6 tested.
6 SOLID:           READY - M1 SRP (ArticleBuilder/ArticleService/ArticleRules); M2 DIP (PasswordChanger -> IAuthService);
                   M3 OCP (EmailSender subclasses); M4 ISP (BLL/Interfaces/IArticleArchive.cs, 3 members;
                   test Archive_pages_depend_only_on_the_narrow_archive_contract).
7 CAP:             READY - one SQL Server today (no partition; ACID != CAP); planned CP for editorial state,
                   AP reads; "withdrawn must vanish" is tested.
8 Migration:       READY - Newsletter & Notifications first; 5 phases; a plan only.
9 Creational:      READY - Builder (BLL/Builders/ArticleBuilder.cs, 7 tests);
                   DI-managed Singleton (LoginThrottle, PasswordResetStore; Assert.Same; classic-vs-DI table in 07);
                   Factory Method (EmailSender.CreateTransport: OutboxEmailSender active, SmtpEmailSender dormant);
                   Abstract Factory (DAL/Interfaces/IDataAccessFactory.cs; family test).
10 IEEE refs:      READY - 12 entries cited in the text; [4][5][7] checked on Crossref; [8][10][11] opened;
                   4 books to check in the library.
Week 4-13 material is kept out of sections 0-10.

=====================================================================
G. SADP - PROPOSAL (P1, sections 1-12, PDF/DOCX, 5-8 pages, via Teams)
=====================================================================
- Sections 1-10 and 12: content exists (sadp_delivery/05).
- Section 8 diagram: L2 rendered.
- Section 5: all six NFAs with status; the Scalability throughput goal was missed in the final run (38.9 < 40 req/s)
  and is reported honestly.
- Section 11 Team & milestones: PARTIAL (names/roles [TBD]).
- Indicative self-rating 21/24 - NOT a grade prediction.

=====================================================================
H. WEEKS 4-13 (later milestones; nothing forced into the code)
=====================================================================
Implemented:
- cache-aside with TTL and version keys (W9);
- p50/p95/p99 measurement (W9);
- ADR-001..008 (W9; ADR-005 still Proposed);
- diagrams as code (W12).
Documented / planned:
- Facade (article page assembly), Strategy (second ranking), State (more editorial states);
- shared state store for scale-out; blue-green (not deployed);
- research proposal outline; mini-ATAM.
Partial:
- resilience (fallback via disabled mail; no retry, to avoid double digests);
- observability (structured logs only).
Not applicable, with reasons:
- Composite, Proxy/Decorator claims;
- API Gateway / EDA / Circuit Breaker / Saga (one deployable; an in-process Channel is not EDA,
  one SQL transaction is not a Saga);
- sharding, load balancing (one instance);
- blockchain / IoT / MQTT (an audit log is not a blockchain);
- agentic AI / RAG (recommendations are deterministic, not AI);
- serverless.
Auth naming: API tokens are opaque random values stored as SHA-256 hashes - NOT JWT, no OAuth2.

=====================================================================
I. STILL OPEN (no known open P0 security or P1 correctness defect)
=====================================================================
ID     Priority            Item
R-T1   P0 (submission)     Member IDs/names, group, section, date; confirm module owners and group size
R-T3   P0 (submission)     Convert the Markdown draft to DOCX/PDF with Figure 1
R-T4   P1                  Library check of 4 book references
R-11   P1                  Push sadp/final-hardening and open a PR so CI runs 137 tests (needs user approval)
R-1    P1                  Shared store for lockout/reset codes/cache before 2+ instances (lockout is per process)
R-3b   P1                  Async EF on the search path; re-measure (throughput 38.9-43.0 req/s around the 40 goal)
R-4    P1                  Production deployment (needs host and credentials)
R-E1   P2 (scope)          Real email: not implemented by decision - NOT a security/correctness defect
R-6..R-9, R-13, R-14  P2   Split IArticleService/AdminController; TimeProvider; AutoMapper licence decision;
                           demo password; orphan sweep; sitemap index

Reproduce everything:
  cd NewsArchiveSolution && dotnet build NewsArchiveSolution.slnx -c Release && dotnet test NewsArchive.Tests -c Release --no-build
  (from repo root) python tools/smoke/smoke.py --stories 5000

=====================================================================
J. USER INPUT STILL REQUIRED
=====================================================================
1. Group number, section, submission date.
2. Every member's student ID and name, and who owns M1-M4 (the GitHub owner sadatnuman must be confirmed).
3. The lecturer's required submission format.
4. Approval to push sadp/final-hardening and open a PR.
5. Later and optional: hosting and credentials; AutoMapper licence decision; whether real email is ever wanted.

=====================================================================
K. FINAL GATES
=====================================================================
EMAIL_OFFLINE      PASS     Disabled outside Development, even with an SMTP host; 2,000 sends -> false, 0 log noise,
                            no SMTP socket; Development outbox only; synthetic recipients; no SMTP registration
                            anywhere (Program.cs:111-115).
P0_SECURITY        PASS     S1 and S2 fixed, tested with real files and parallel requests, mutation-checked.
P1_CORRECTNESS     PASS     C1-C4 fixed, each with a test; missing API version -> 400; 137/137 tests pass.
MIDTERM_CODE       PASS     Sections 0-10 backed by code/tests/figure/measurement, Lectures 1-3 only;
                            member ownership honest but tentative.
MIDTERM_SUBMISSION PARTIAL  Needs member names/IDs, group, section, date and the converted DOCX/PDF.
FULL_SEMESTER      PARTIAL  Applicability matrix and milestone plan exist; final report, presentation,
                            deployment and multi-instance work do not.
Database targets: only NewsArchiveTestDB and NewsArchiveSmokeDB were used; NewsArchiveDB is unchanged.
Mail transport: no SMTP registration exists on this branch.
No mark is guaranteed.
```
