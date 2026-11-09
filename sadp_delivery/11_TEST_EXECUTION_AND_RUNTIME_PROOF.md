# 11 · Test Execution and Runtime Proof

Every command below was run on this machine on 2026-11-09, on branch `sadp/review-corrections`. Nothing is estimated. Commands run from `NewsArchiveSolution/` unless they say otherwise.

## Build and tests (this revision)

| Step | Command | Exit | Result |
|---|---|---|---|
| Start point | `git checkout -b sadp/review-corrections origin/main` | 0 | `main` = `484deb3`, the same file tree as `origin/sadp/midterm-readiness` (`git diff --stat` empty) |
| Build after the code changes | `dotnet build NewsArchiveSolution.slnx -c Release` | 0 | 0 warnings, 0 errors |
| Tests, first run | `dotnet test NewsArchiveSolution.slnx -c Release` | 0 | 115 passed, 0 failed |
| Tests, final run (after `MappingDepthTests` was added) | `dotnet test NewsArchive.Tests -c Release --no-build` | 0 | **116 passed, 0 failed, 0 skipped** (7 s) |
| Restore | `dotnet restore NewsArchiveSolution.slnx` | 0 | All projects up to date |
| Whitespace | `git diff --check` | 0 | No errors (only Git's LF→CRLF notices) |
| Vulnerable packages | `dotnet list NewsArchiveSolution.slnx package --vulnerable --include-transitive` | 0 | Only AutoMapper 14.0.0 (GHSA-rvv3-g6hj-g44x, High), already present; mitigation and upgrade path in `08`. DAL is clean. |
| Migration | `dotnet ef migrations add ArticleEditConcurrencyToken` | 0 | Empty `Up`/`Down`. The snapshot gains `.IsConcurrencyToken()` on `UpdatedAt`. |

**New tests in this revision (8):**

| Test | What it proves |
|---|---|
| `AppTests.A_decision_racing_another_editor_reports_a_conflict_not_a_wrong_state` | When a second admin's request had already loaded the story before the first decision, the second decision returns `ChangeResult.Conflict`, and the stored status stays Published with no review note. |
| `AppTests.An_edit_from_a_stale_form_never_overwrites_a_newer_edit` | Over the REST API: the first editor's save → 204. The second editor's save with the same old version → 409 "Someone else changed this article…". A save with no version → 409. The stored title is the first editor's. After reloading, the second editor's save → 204. |
| `AppTests.Two_saves_in_flight_from_the_same_version_cannot_both_win` | Two scopes load the same story and both save; the database token lets only the first one through. |
| `AppTests.Archive_pages_depend_only_on_the_narrow_archive_contract` | With a 3-method `IArticleArchive` stub registered, `/api/archive` and `/archive` are served from the stub (ISP). |
| `AppTests.A_withdrawn_story_disappears_from_every_public_read_at_once` | After a delete: page 404, API 404, search, list and trending no longer contain it, RSS no longer contains it. |
| `MemoryAppCacheTests` (2) | A list built while the cache was being cleared is never served afterwards; clearing one region keeps the other. |
| `MappingDepthTests.Every_mapping_has_a_depth_limit` | Every AutoMapper type map has `MaxDepth` between 1 and 3. |

**Changed tests:**
- `AppTests.Author_submission_stays_hidden_until_an_admin_approves_it` now also checks that the ordinary 409 says "Only pending articles can be rejected."
- `EmailSenderTests.Development_outbox_writes_the_message_and_production_without_smtp_reports_failure` now expects `false` in Production without SMTP. Before, it expected `true`.

## Test count by file

| File | Tests |
|---|---|
| Integration/AppTests | 17 |
| Patterns/ArticleBuilderTests | 7 |
| Patterns/EmailSenderTests | 5 |
| Rules/RuleTests | 29 [Fact] + Theory rows |
| Security/SecurityTests | 8 |
| Security/MappingDepthTests | 1 |
| Services (5 files) | 29 |

The runner counts each Theory row as its own test, which is how the total reaches 116.

## Databases used

| Database | Purpose | Notes |
|---|---|---|
| `NewsArchiveTestDB` | Integration tests | Created and migrated by the tests |
| `NewsArchiveSmokeDB` | Smoke run and benchmark | Dropped and recreated by `tools/smoke/smoke.py`, then filled with `database/sample-data.sql` and 5,000 generated stories |
| `NewsArchiveDB` | The user's data | **Not touched.** 28 articles, checked with `sqlcmd` during this revision. The new migration was not applied to it. |

## Reproducible smoke run

**Command (from the repository root):**

```bash
python tools/smoke/smoke.py --stories 5000
```

**What the script does:**
1. It refuses any database name that does not contain `Smoke` or `Test`, and always refuses `NewsArchiveDB`.
2. It builds the solution in Release.
3. It drops and recreates the smoke database, starts the app once so EF Core applies the migrations, and loads `database/sample-data.sql`.
4. It starts the app on `127.0.0.1:5199` in Development, so emails are only written to the local outbox.
5. It reads the demo accounts from `appsettings.Development.json`. No password is stored in the script.
6. It runs the checks, then the benchmark, and stops the app.

**Result: 40/40 checks passed, exit code 0.** The checks:

- **Pages** (`/health`, `/`, `/?lang=bn`, `/archive`, search, `/rss.xml`, `/sitemap.xml`, `/swagger/index.html`, `/Auth/Login`, `/offline.html`): 200. The sign-in page lists 3 demo accounts.
- **Barriers:**
  - anonymous `/Admin` → 302 to sign-in;
  - anonymous `/api/articles/pending` → 401;
  - reader → 403, admin → 200;
  - reader cannot create an article → 403.
- **Editorial flow:**
  - author submits → 201; the pending story → 404;
  - author cannot approve → 403; admin approves → 204; story public → 200;
  - a decision on the already decided story → 409 "Only pending articles…";
  - search finds the story.
- **Rejection:** reject with a note → 204; the author sees the note; the story stays hidden.
- **Stale edits:**
  - API: first edit → 204, the stale copy → 409 "Someone else changed…", readers see the first edit;
  - MVC: the admin signs in through the form; the edit page carries a hidden `Version`; the first save → 302; the same form again → 200 with the conflict message.
- **Scheduling:** schedule for +75 s → 204; not public yet; in the scheduled list; **published by the background job after 85 s**.
- **Newsletter:** subscribe → 200, exactly one new file in the local outbox (no real email sent).
- **Withdrawal:**
  - before the delete: the story is on its page, in the list and in RSS;
  - delete → 204;
  - after the delete: gone from the page, the API, the list and RSS.

**Benchmark output (same run):**

```text
GENERATED 5000/5000 stories through the admin form in 27 s (status counts: {302: 5000})
DATASET published=5024 total=5031 word_rows=915558
BENCH search page sequential    n=400 p50=47.1ms p95=72.2ms p99=80.3ms throughput=20.9 req/s errors=0
BENCH search page concurrent=20 n=800 p50=446.7ms p95=672.5ms p99=779.5ms throughput=43.0 req/s errors=0
SUMMARY 40/40 checks passed; benchmark errors=0
```

Dataset, configuration, workload and limits are described in `08`.

**Earlier attempt (kept for honesty).** The first 5,000-story run created the stories through the REST API. Only 352 were created, because the API allows 120 requests per minute per IP (`Program.cs:92`). That run also failed one check, because `/news/{id}` answers with a redirect. The script was corrected:
- stories are now created through the admin form;
- the withdrawal check follows redirects and first confirms that the story is visible.

The 40/40 result above comes from the corrected script.

**Shutdown.** The script stopped the app. No `NewsArchive.dll` process was left running. The user's Visual Studio app (ports 5096 and 7240) was not touched.

## Earlier baseline (previous revision, for comparison)

- 108/108 tests.
- Smoke 32/32 on 31 stories.
- Search p95: 51.2 ms with one user; 621.9 ms with 20 users (44.7 req/s).

## Not verified

- A real deployment, HTTPS on a host, and SMTP delivery.
- Behaviour with 2+ app instances.
- This branch on GitHub Actions: it has not been pushed yet. The previous branch's 108 tests passed on `main`, as the reviewer reported.
- The MVC "decision conflict" flash message. It is reached only by a true race between two requests, which the HTTP tests cannot time reliably. The service result behind it is tested.
- The Bangla wording of the two new messages, as rendered in a browser. It is present in `SharedResource.bn.resx` but was not viewed on screen.
