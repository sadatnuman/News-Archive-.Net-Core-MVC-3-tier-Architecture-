# SADP Handoff for ChatGPT: The Meridian Newsfolio (AIUB CSC 4273), Revision 2

Written for an independent reviewer who has not seen the terminal session. Every claim gives a path (relative to `NewsArchiveSolution/` unless stated), a test name or a command result. Supporting files are in `sadp_delivery/`.

## A. Branch, commits, safety

| Item | Value |
|---|---|
| Repository | `sadatnuman/News-Archive-.Net-Core-MVC-3-tier-Architecture-` (remote unchanged) |
| Start point | `main` at `484deb3` ("Merge pull request #9 from sadatnuman/sadp/midterm-readiness"); same file tree as `origin/sadp/midterm-readiness` (empty `git diff --stat`) |
| Work branch | `sadp/review-corrections` (local) |
| Code + documents commit | `95851fc2b5fccc803d3569657b5df9f9322a862f` |
| This handoff | Committed on the same branch in the commit right after `95851fc` (`git log -1 -- sadp_delivery/SADP_HANDOFF_FOR_CHATGPT.md` shows it) |
| Pushed / merged | **No / No** |
| Size, code + README/ARCHITECTURE + tools | 29 files, +1,793 / −90 (1,049 of the added lines are the EF-generated migration designer) |
| Size, `sadp_delivery/` | 22 files, +1,051 / −378 (including 2 PNG and 2 SVG diagrams) |

**Safety:**
- No force push, merge or deployment.
- No real email: SMTP host empty, outbox only.
- No destructive migration: the new migration `20261109152432_ArticleEditConcurrencyToken` has empty Up/Down.
- The user's `NewsArchiveDB` was **not touched** (28 articles, checked with `sqlcmd`). The new migration was not applied to it.
- The smoke script and the 5,000 generated stories ran only on the disposable `NewsArchiveSmokeDB`.
- No secrets added. The smoke script reads the existing development demo password at run time; it is not stored in the script.
- No group details were invented: every name and ID is [TBD].

## B. Files changed

**Code (BLL / DAL / UI):**
- `BLL/ChangeResult.cs` (new: `ChangeResult { Done, NotAllowed, Conflict }`, and the two conflict messages).
- `BLL/Interfaces/IArticleArchive.cs` (new, 3 members).
- `BLL/Interfaces/IArticleService.cs` (5 methods now return `ChangeResult`; 3 archive methods moved out; 29 → 26 members).
- `BLL/Services/ArticleService.cs` (implements both interfaces; version check at `:89`).
- `BLL/Rules/ArticleRules.cs:23` (`Version`).
- `BLL/DTOs/ArticleDTO.cs` (`Version`), `BLL/DTOs/ArticleFormDTO.cs` (`Version`).
- `BLL/BusinessLayerServices.cs:17` (registers `IArticleArchive`).
- `DAL/EF/NewsArchiveDbContext.cs:92` (`UpdatedAt` concurrency token), the new empty migration, and the model snapshot.
- `NewsArchive/Controllers/AdminController.cs:105` (`Decided` helper).
- `Api/ArticlesApiController.cs` (409 for conflicts in `Update` and `Decided`).
- `ArticleController.cs` (Edit conflict message).
- `ArchiveController.cs`, `Api/ArchiveApiController.cs`, `SeoController.cs` (use `IArticleArchive`).
- `NewsArchive/Email/OutboxEmailSender.cs:39`.
- `Views/Article/_ArticleForm.cshtml:16` (hidden `Version`).
- `Resources/SharedResource.bn.resx` (2 Bangla messages).

**Tests:**
- `NewsArchive.Tests/Integration/AppTests.cs`: 5 new tests, 1 extended.
- `Patterns/EmailSenderTests.cs`: 1 changed.
- `Services/MemoryAppCacheTests.cs` (new, 2 tests).
- `Security/MappingDepthTests.cs` (new, 1 test).

**Tools and docs:**
- `tools/smoke/smoke.py` (new).
- `.gitignore` (`__pycache__/`).
- `README.md`, `ARCHITECTURE.md`.

**SADP deliverables:**
- `MIDTERM_DRAFT_FOR_REVIEW.md`
- `01`, `02`, `03`, `04`, `05`, `06`, `07`, `08`
- `09_C4_AND_4PLUS1/` (`C4_DIAGRAMS.md`, `4PLUS1_VIEWS.md`, `render_c4.py`, `c4_level1_context.png/.svg`, `c4_level2_containers.png/.svg`)
- `11`, `12`, `13`, `14`, `15`, `WORK_LOG.md`, this file

## C. What the reviewer asked, and what was done

| Review item | Done | Evidence |
|---|---|---|
| P1.1 Draft vs. Midterm §0–10 | Re-checked section by section against P2 p1–5. Cover, §0 and the member names stay [TBD]. | `04` |
| P1.2 M4's weak ISP | New `IArticleArchive` (3 members). Archive page, archive API and sitemap depend only on it. Abstract Factory unchanged. | `AppTests.Archive_pages_depend_only_on_the_narrow_archive_contract` (3-method stub serves `/archive` and `/api/archive`) |
| P1.3 Keep the DI Singleton and explain it | Kept. Side-by-side "classic vs DI-managed" table, viva answer and the honest weakness are in `07` and `13`. | `AppTests.Lockout_and_reset_stores_are_one_instance_for_the_whole_app` |
| P1.4 4+1 process wording | Corrected: requests run on pool threads, no thread is reserved, an `await` releases the thread; our synchronous EF calls block a pool thread for each query. Cited to Microsoft "ASP.NET Core best practices" [10]. | Draft §5, `09/4PLUS1_VIEWS.md` |
| P1.5 C4 review | L1, L2 and L3 checked for boundary, element types, technology and arrow labels. Fixed: browser scripts are delivered by the web app; CDN added; SMTP "when configured"; L3 filters run *before* controllers; every arrow labelled. | `09/C4_DIAGRAMS.md` review table |
| P1.6 Render C4 | `render_c4.py` writes SVG and PNG (local headless Chrome, nothing installed). Both PNGs viewed; overlapping labels fixed and re-rendered. L1 = midterm Figure 1, L2 = proposal §8. | `09_C4_AND_4PLUS1/*.png` |
| P1.7 IEEE in-text citations | Every draft section cites [n]. 12 references. [4], [5], [7] checked against Crossref; [8], [10], [11] opened. 4 books still need a library check. | Draft §10 |
| P1.8 No Week 4–13 material in §0–10 | Boundary check written; `DigestSender` described only as an availability tactic. | `04` |
| P2.1 Stale-edit overwrites | Version (`StatusId-UpdatedAt.Ticks`) in the edit form and the API; `UpdatedAt` is also a concurrency token. A stale save is refused before writing; two saves in flight cannot both win. | `An_edit_from_a_stale_form_never_overwrites_a_newer_edit`, `Two_saves_in_flight_from_the_same_version_cannot_both_win`; smoke over API and MVC |
| P2.2 409 messages | `ChangeResult.Conflict` → "Another editor acted on this story at the same moment…" or "Someone else changed this article after you opened it…" (EN + BN). Ordinary wrong states keep "Only pending articles…". | `A_decision_racing_another_editor_reports_a_conflict_not_a_wrong_state`; extended `Author_submission_stays_hidden_…` |
| P2.3 Production email | Outside Development, without SMTP, `SendAsync` now returns `false` and logs "No email transport is configured". Pages still work; the password-reset answer does not reveal whether the account exists; the Development outbox is unchanged. | `EmailSenderTests.Development_outbox_writes_the_message_and_production_without_smtp_reports_failure` |
| P2.4 Impact review | Visibility, authorization, audit and scheduling reviewed: no change to visibility or authorization; audit only on `Done`; the scheduler uses `ExecuteUpdate` and cannot be blocked, while a form opened before the scheduler acted gets a conflict instead of pushing the story back to Draft. | `12` "Impact review" |
| P2.5 Stale caches vs. withdrawn content | Verified with tests. Invalidation runs after each successful write, and version keys stop late cache writes. Remaining gap: RSS/sitemap `Cache-Control` (15/60 min) lets proxies keep a withdrawn *title*; its link answers 404. | `A_withdrawn_story_disappears_from_every_public_read_at_once`, `MemoryAppCacheTests` (2); `08`, `15` R-10 |
| P3.1 Restore, Release build, tests | Restore exit 0; build 0 warnings / 0 errors; **116 passed, 0 failed, 0 skipped**. | `11` |
| P3.2 E2E smoke on an isolated database | **40/40** on `NewsArchiveSmokeDB`. | `11` |
| P3.3 Reproducible smoke script | `tools/smoke/smoke.py`: refuses database names without `Smoke`/`Test`, builds, recreates the smoke database, loads sample data, runs the app on loopback in Development, runs the checks, then the optional benchmark, then stops the app. | README "Tests" |
| P3.4 Benchmark ≥ 5,000 stories | See section D. | `08`, `11` |
| P3.5 AutoMapper advisory | Not upgraded (15+ is commercially licensed; untested major change). Every map has `MaxDepth(3)`, now guarded by a test. Upgrade paths written down. | `MappingDepthTests.Every_mapping_has_a_depth_limit`; `08` |
| P3.6 Six NFAs classified | VERIFIED / MEASURED / TARGET / NOT VERIFIED for each. | Draft §3, `08` |
| P4 Documentation sync | "Not pushed" statements corrected (revision 1 was pushed and merged as PR #9); line numbers updated after the refactor; matrices, diagrams, patterns, evidence and remaining actions updated. Week-by-week coverage keeps IMPLEMENTED / DOCUMENTED / PLANNED / N/A with reasons; no blockchain, IoT, microservices, AI agents or Saga were forced in. | `01`, `02`, `03`, `06`, `12`, `14`, `WORK_LOG.md` |

## D. Verified evidence

**Commands** (2026-11-09, this machine):

| Command | Exit | Result |
|---|---|---|
| `dotnet restore NewsArchiveSolution.slnx` | 0 | up to date |
| `dotnet build NewsArchiveSolution.slnx -c Release` | 0 | 0 warnings, 0 errors |
| `dotnet test NewsArchive.Tests -c Release --no-build` | 0 | 116 passed, 0 failed, 0 skipped |
| `git diff --check` | 0 | clean |
| `dotnet list … package --vulnerable --include-transitive` | 0 | only AutoMapper 14.0.0 (GHSA-rvv3-g6hj-g44x, High) |
| `python tools/smoke/smoke.py --stories 5000` (repo root) | 0 | `SUMMARY 40/40 checks passed; benchmark errors=0` |

**Smoke checks (40):**
- Pages and barriers: 200 for public pages; 302 / 401 / 403 where expected.
- Editorial flow: author → hidden → admin approve → public; a decision on a decided story → 409 "Only pending articles…"; search finds the story.
- Rejection: the note is visible to the author.
- Stale edits:
  - API: 204, then 409 for the stale copy;
  - MVC: the edit page has a hidden `Version`; first save 302; the same form again shows the conflict.
- Scheduling: published by the background job after 85 s.
- Newsletter: exactly one outbox file.
- Withdrawal: the story is visible before; after the delete it is gone from the page, API, list and RSS.

**Search benchmark:**
- **Dataset:** 5,024 published stories (5,031 in all), 915,558 word-index rows. The 5,000 generated stories were created through the real admin form; all returned 302 in 27 s.
- **Story content:** half English, half Bangla. Zipf-weighted vocabulary: 83 real English + 68 real Bangla words, the rest synthetic. Seed 4273.
- **Configuration:** Windows 11, Ryzen 5 7520U (8 logical CPUs), 13.8 GB RAM, SQL Server 2025 Express on the same machine, Release build, Development, loopback.
- **Workload:** 30 fixed queries (frequent, mid, rare, two-word, 3-letter prefix, no-match, both languages) on `/Article/Index?keyword=`; 20 warm-up requests; 400 sequential requests; 800 requests from 20 threads.

| Mode | n | p50 | p95 | p99 | Throughput | Errors |
|---|---|---|---|---|---|---|
| Sequential | 400 | 47.1 ms | 72.2 ms | 80.3 ms | 20.9 req/s | 0 |
| 20 concurrent | 800 | 446.7 ms | 672.5 ms | 779.5 ms | 43.0 req/s | 0 |

- **Targets:** p95 ≤ 200 ms (1 user) and ≤ 1 s (20 users). Both are met on this machine.
- **Limits:**
  - one run;
  - client and server share the CPU;
  - synthetic text;
  - all generated stories share a publish day;
  - Express edition;
  - the REST search API was not benchmarked, because it is rate-limited to 120 requests per minute per IP.

**Six quality attributes (status):**

| Attribute | Status |
|---|---|
| Reliability | Decisions and edits VERIFIED; scheduled publish MEASURED |
| Security | VERIFIED |
| Availability | Mail isolation VERIFIED; 99.5 % uptime NOT VERIFIED (no deployment) |
| Performance | MEASURED |
| Scalability | 1 instance MEASURED; 2+ instances TARGET |
| Maintainability | VERIFIED |

**Patterns (W3) with GoF roles, unchanged in role:**

| Pattern | Where | Tests |
|---|---|---|
| Builder | `BLL/Builders/ArticleBuilder.cs`, director `ArticleService.Create` (`:60`) | 7 |
| Factory Method | `NewsArchive/Email/EmailSender.cs:20` | 5 |
| Abstract Factory | `IDataAccessFactory`, family test | 1 |
| Singleton, DI-managed | `Program.cs:52`, `:118` | `Assert.Same` |

## E. What could not be tested

- A real deployment, HTTPS on a host, real SMTP delivery, 99.5 % uptime.
- 2+ app instances, which would need a shared state store.
- GitHub Actions on this branch: it has not been pushed. The previous revision's 108 tests passed on `main`, as reported by the reviewer.
- The MVC flash message for a decision race through HTTP. A true race cannot be timed reliably; the service result behind it is tested.
- The two new Bangla messages as rendered in a browser. They are in the resource file, but were not viewed on screen.
- The 4 book references against library copies.

## F. Remaining gaps (from `15`)

| ID | Priority | Gap |
|---|---|---|
| R-T1 | **P0** | Member IDs, names, group, section, date; who owns which module |
| R-T3 | **P0** | Convert the Markdown draft to the submission format (DOCX/PDF) with Figure 1 |
| R-T4 | P1 | Library check of 4 book references |
| R-1 | P1 | Shared state store before 2+ instances |
| R-3b | P1 | Async EF on the search path; re-run the benchmark (tests the 9× concurrency slowdown hypothesis) |
| R-4 | P1 | Production deployment |
| R-11 | P1 | Push this branch so CI runs the 116 tests |
| R-6 | P2 | `IArticleService` still has 26 members; `AdminController` has 12 dependencies |
| R-7 | P2 | `TimeProvider` instead of the static `SiteClock` |
| R-8 | P2 | AutoMapper upgrade or replacement |
| R-9 | P2 | Change the sample password |
| R-10 | P2 | Shorter RSS/news-sitemap cache headers |
| R-12 | P2 | Document the API's required `version` field |

## G. User information still required

1. The group number, section and submission date.
2. Every member's student ID and name, and which member owns which module (M1–M4). The GitHub owner `sadatnuman` must be confirmed by the group.
3. The submission format the lecturer wants (DOCX or PDF).
4. Whether to push `sadp/review-corrections` and open a pull request.
5. For a live demo: a hosting choice and its credentials.
6. For an AutoMapper upgrade: whether the group qualifies for the free Community licence.

## H. Verdicts

**MIDTERM (§0–10): CODE AND EVIDENCE READY; SUBMISSION PARTIAL.**
- Every section has real code, test, rendered-diagram or measurement evidence.
- Every section cites IEEE sources and stays inside Lectures 1–3.
- It cannot be submitted until the group fills in its personal data (R-T1) and converts the draft to the required format (R-T3).

**FULL SEMESTER: PARTIAL.**
- All 12 supplied decks are mapped (Week 7 is the exam week).
- Week 9 ADRs, Week 12 diagrams-as-code with two rendered levels, and a re-runnable benchmark exist.
- Week 4–5 patterns are proposed with triggers, not built, on purpose.
- Weeks 6, 10, 11 and 13 are mostly not applicable to one deployable, with reasons.
- There is no deployment, no multi-instance test, and the final report and presentation are not written yet.
- No mark is guaranteed.
