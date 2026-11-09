# 11 · Test Execution and Runtime Proof

All commands below were run on this machine in this task. Nothing below is estimated. Any output that was not captured in full is marked as such.

## Build and tests

| When | Command (from `NewsArchiveSolution/`) | Exit | Result |
|---|---|---|---|
| Baseline | `dotnet restore NewsArchiveSolution.slnx` | 0 | OK |
| Baseline | `dotnet build NewsArchiveSolution.slnx -c Release` | 0 | 0 warnings, 0 errors |
| Baseline | `dotnet test NewsArchive.Tests -c Release --no-build` | 0 | 92 passed, 0 failed, 0 skipped |
| Mid-task | same | 1 | 106 passed, **1 failed**. `ArticleBuilderTests.An_author_draft…` expected `bn` but got `en`. This exposed defect F1, which was fixed in `BLL/AppRoles.cs:27`. |
| Mid-task | same | 1 | 107 passed, **1 failed**. The first version of the repository-family test used `UserRepo.Get`, which reads with `AsNoTracking`. The test was changed to use the tracked category repository. |
| Final | `dotnet build NewsArchiveSolution.slnx -c Release` | 0 | 0 warnings, 0 errors |
| Final | `dotnet test NewsArchive.Tests -c Release --no-build` | 0 | **108 passed, 0 failed, 0 skipped** (5 s) |
| Final | `git diff --check` | 0 | No whitespace errors (only Git's LF→CRLF notices) |
| Final | `dotnet list NewsArchiveSolution.slnx package --vulnerable --include-transitive` | 0 | Only AutoMapper 14.0.0 (GHSA-rvv3-g6hj-g44x, High), which was already present and suppressed by an earlier user decision. DAL is clean. |

Test count by file:

| File | Tests |
|---|---|
| Integration/AppTests | 12 |
| Patterns/ArticleBuilderTests | 7 |
| Patterns/EmailSenderTests | 5 |
| Rules/RuleTests | 29 [Fact] + Theory rows |
| Security/SecurityTests | 8 |
| Services | 27 |

The runner counts each Theory row as its own test, which is how the total reaches 108.

## Databases used

| Database | Purpose | Notes |
|---|---|---|
| `NewsArchiveTestDB` | Integration tests | Created and migrated by the tests |
| `NewsArchiveSmokeDB` | Smoke run and benchmark | Created by the app in this task, then filled with `database/sample-data.sql` |
| `NewsArchiveDB` | The user's data | **Not touched.** It had 28 articles before and after. |

## Runtime smoke run

**How the app was started (loopback only, isolated database):**

```bash
cd NewsArchiveSolution/NewsArchive
env ASPNETCORE_ENVIRONMENT=Development ASPNETCORE_URLS=http://127.0.0.1:5199 \
    "ConnectionStrings__DbConn=Data Source=localhost\SQLEXPRESS;Initial Catalog=NewsArchiveSmokeDB;Integrated Security=True;TrustServerCertificate=True;" \
    dotnet bin/Release/net10.0/NewsArchive.dll
```

**Script.** `sadp_smoke.py` (session scratchpad, standard library only). It signs in with the documented sample accounts; the password was passed through the `SAMPLE_PW` environment variable and is not written here.

**Result: 32/32 checks passed.** Every check:

- Pages: `/health`, `/`, `/?lang=bn`, `/archive`, `/Article/Index?keyword=river`, `/rss.xml`, `/sitemap.xml`, `/swagger/index.html`, `/Auth/Login`, `/offline.html` → 200.
- Development sign-in page lists 3 demo accounts.
- Anonymous `/Admin` → 302 to `/Auth/Login`.
- Anonymous `/api/articles/pending` → 401.
- Reader → 403; admin → 200.
- Reader cannot create an article (403).
- Author submits (201). The pending story is hidden (404). The author cannot approve (403). The admin approves (204). The story is public (200). A second decision gets 409. Search finds the published story.
- Admin rejects with a note (204). The author sees the note. The rejected story stays hidden (404).
- Admin schedules for +75 s (204). The story is not public yet (404) and is in the scheduled list. **The scheduler published it by itself, about 85 s after the schedule request.**
- Newsletter subscribe returns ok and writes exactly one confirmation file to the local outbox. SMTP host is empty, so **no real email was sent**.

**App log.** `Added 28 article(s) to the search index` and `Published 1 scheduled article(s)`. The only warning was `Failed to determine the https port for redirect`, which is expected on an HTTP-only loopback run.

**Shutdown.** The smoke server process was stopped. A final check found no remaining `bin/Release/net10.0/NewsArchive.dll` process. The user's Visual Studio app (ports 5096 and 7240) was not touched.

## Benchmark (measured, one run)

**Conditions:**
- Windows 11, AMD Ryzen 5 7520U (8 logical CPUs), 13.8 GB RAM.
- SQL Server 2025 Express on the same machine.
- Release build, Development environment, anonymous requests.
- 10 warm-up requests, then 200 sequential requests per page.
- Concurrent run: 400 requests from 20 threads.

| Page | Mode | n | p50 | p95 | p99 | Errors |
|---|---|---|---|---|---|---|
| Home `/` | sequential | 200 | 37.7 ms | 83.4 ms | 88.1 ms | 0 |
| Search `/Article/Index?keyword=river` | sequential | 200 | 33.5 ms | 51.2 ms | 67.6 ms | 0 |
| Article `/news/{id}` | sequential | 200 | 33.2 ms | 50.1 ms | 51.4 ms | 0 |
| Archive `/archive` | sequential | 200 | 33.4 ms | 50.5 ms | 55.7 ms | 0 |
| Search | 20 concurrent | 400 | 424.0 ms | 621.9 ms | 762.0 ms | 0 (throughput 44.7 req/s) |

**Limits of these numbers:**
- 31 stories only.
- The client runs on the same machine, so it competes for CPU.
- One run, with no variance analysis.
- No production hardware or network.
- No availability measurement was possible.

## Not verified

- A real deployment, HTTPS on a host, and SMTP delivery.
- Behaviour with 2+ instances.
- The Bangla UI on real phones in this task (it was checked in earlier work, not here).
- Rendering the Mermaid diagrams to images.
