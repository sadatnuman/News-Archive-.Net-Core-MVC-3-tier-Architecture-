# 15 · Remaining Actions

Priority scale:
- **P0** blocks the midterm submission.
- **P1** matters before a public launch or the Week 12 checkpoint.
- **P2** is good engineering, can wait.

| ID | Priority | Item | File / place | Impact | How to verify | Needs from the user |
|---|---|---|---|---|---|---|
| R-T1 | P0 | Fill in member IDs, names, group number, section, date; confirm who owns each module | `MIDTERM_DRAFT_FOR_REVIEW.md` §0, §3, §6, §9; `13` | The report cannot be submitted with TBD | Read the report | **Yes: personal data** |
| R-T2 | P0 | Render C4 L1 (and L2 for the proposal) to an image and insert it | `09_C4_AND_4PLUS1/C4_DIAGRAMS.md` | §4 needs a figure | Open the exported PNG or SVG | A tool choice (mermaid.live, draw.io) |
| R-V1 | P0 (viva risk) | M2 must be ready to defend a DI-managed singleton, or switch to a classic Singleton if the lecturer requires the private-constructor form | `13` M2 | A possible mark loss | Ask the lecturer | Lecturer's answer |
| R-1 | P1 | Move session, lockout, reset codes and cache to a shared store before running 2+ instances | `Program.cs`, `Auth/LoginThrottle.cs`, `Auth/PasswordResetStore.cs` | Scale-out and restart safety (ADR-005) | Integration test with two app factories sharing one store | Choice of SQL or Redis |
| R-2 | P1 | Show "someone else already decided this story" instead of a generic failure when a concurrency conflict happens | `AdminController` approve and reject, `ArticlesApiController` | Clearer UX | An extra assertion in the race test | No |
| R-3 | P1 | Benchmark again with 5k–10k generated stories; check whether making the EF calls async reduces the 12× concurrent slowdown | Repositories / services | Performance evidence at a realistic size | Rerun `sadp_smoke.py` benchmark | No |
| R-4 | P1 | Production deploy following the README go-live list (TrustAllProxies, SMTP, change the sample passwords, HTTPS) | Host | Availability and security evidence | Uptime monitor | **Yes: host choice and credentials** |
| R-5 | P2 | Add a staleness check to the article edit form (hidden `UpdatedAt` or `rowversion` compared on save) | `ArticleService.Update`, `_ArticleForm.cshtml` | Prevents lost edits (`02` F3) | A test with two edits | No |
| R-6 | P2 | Split `IArticleService` (reader vs editor) and `AdminController` (per admin area) | BLL interfaces, controllers | ISP / SRP smells (`03`) | Build + tests | No |
| R-7 | P2 | Inject `TimeProvider` instead of the static `SiteClock` | `BLL/SiteClock.cs` | Deterministic time in tests | Tests with a frozen clock | No |
| R-8 | P2 | AutoMapper 14 advisory (suppressed) | `BLL.csproj` | Known DoS advisory with deep graphs; mitigated by `MaxDepth(3)` | `dotnet list package --vulnerable` | Decision to upgrade or replace |
| R-9 | P2 | Demo password `Meridian@2026` appears in the README and development settings | `README.md`, `appsettings.Development.json` | Sample data only. Production settings have no demo accounts (tested). | — | Optional: change it |

## Ready-to-send follow-up prompt for Claude Code (if the reviewer agrees)

```text
On branch sadp/midterm-readiness (local only, do not push):
1. Implement R-2: when ArticleService.Approve/Reject/PublishNow/Unschedule returns false because of a
   concurrency conflict (status changed since load), the MVC flash and API ProblemDetails must say
   "This story was already decided by someone else." Distinguish this from "not pending".
   Add an assertion to AppTests.Two_reviewers_cannot_both_decide_the_same_article via the API.
2. Implement R-5: optimistic check for the article edit form using UpdatedAt as a hidden field
   compared in ArticleService.Update; add a test with two sequential edits from the same stale form.
3. Run R-3: generate 5,000 stories in NewsArchiveSmokeDB only, rerun the benchmark, report p50/p95/p99.
Rebuild, run all tests, update sadp_delivery/11, 12, 15 and the handoff. Never touch NewsArchiveDB.
```
