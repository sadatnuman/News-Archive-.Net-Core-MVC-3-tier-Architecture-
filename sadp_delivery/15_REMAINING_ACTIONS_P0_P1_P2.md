# 15 · Remaining Actions

**Priority scale:**
- **P0** blocks the midterm submission.
- **P1** matters before a public launch or the Week 12 checkpoint.
- **P2** is good engineering and can wait.

## Closed in revision 2 (`sadp/review-corrections`)

| ID | Item | How it was closed | Evidence |
|---|---|---|---|
| R-T2 | Render C4 L1 (midterm) and L2 (proposal) | `render_c4.py` → SVG + PNG; reviewed against C4 rules | `09_C4_AND_4PLUS1/c4_level1_context.png`, `c4_level2_containers.png` |
| R-V1 | DI-managed Singleton: keep it and explain it | Kept (W3 s11, s31). Side-by-side table and viva answer in `07` and `13`. | — |
| R-2 | A concurrent decision looked like an ordinary wrong state | `ChangeResult.Conflict` with its own message (EN + BN), MVC and API | `A_decision_racing_another_editor_…` |
| R-3 | Benchmark with 5k–10k stories | 5,024 published stories, 915,558 word rows; p95 72.2 ms and 672.5 ms | `08`, `11` |
| R-5 | Stale edit forms overwrote newer edits | Form/API version + `UpdatedAt` concurrency token | `An_edit_from_a_stale_form_…`, `Two_saves_in_flight_…`, smoke |
| F4 | Production without SMTP reported success | `SendAsync` returns `false` and logs | `EmailSenderTests` |
| — | M4's ISP justification was weak | `IArticleArchive` (3 members) with a stub test | `Archive_pages_depend_only_…` |
| — | 4+1 process view said one thread per request | Corrected and cited [10] | `09/4PLUS1_VIEWS.md` |
| — | Smoke test only in a scratch folder | `tools/smoke/smoke.py` in the repository, guarded and without secrets | 40/40 |
| — | No in-text citations | IEEE [n] citations in every section; journal references checked against Crossref | Draft §10 |

## Still open

| ID | Priority | Item | File / place | Impact | How to verify | Needs from the user |
|---|---|---|---|---|---|---|
| R-T1 | **P0** | Fill in member IDs, names, group number, section and date; confirm who owns each module | `MIDTERM_DRAFT_FOR_REVIEW.md` cover, §0, §3, §6, §9; `13` | The report cannot be submitted with [TBD] | Read the report | **Yes: personal data** |
| R-T3 | **P0** | Turn the Markdown draft into the submission file (DOCX or PDF, as the lecturer asks) with Figure 1 embedded | Draft | Submission format | Open the file | Format choice |
| R-T4 | P1 | Check the 4 book references against the library copies | Draft §10 [1], [2], [3], [12] | Citation accuracy | Compare title pages | Library access |
| R-1 | P1 | Move session, lockout, reset codes and cache invalidation to a shared store before running 2+ instances | `Program.cs`, `Auth/LoginThrottle.cs`, `Auth/PasswordResetStore.cs`, `Caching/MemoryAppCache.cs` | Scale-out and restart safety (ADR-005) | Integration test with two app factories sharing one store | Choice of SQL or Redis |
| R-3b | P1 | Make the EF calls on the search path async and compare with the 20-client benchmark | `ArticleRepo.Search`, services, controllers | Tests the "blocked pool threads" hypothesis for the 9× slowdown | `python tools/smoke/smoke.py --stories 5000` before/after | No |
| R-4 | P1 | Production deploy following the README go-live list (TrustAllProxies, SMTP, new sample passwords, HTTPS) | Host | Availability and security evidence | Uptime monitor | **Yes: host choice and credentials** |
| R-11 | P1 | Push `sadp/review-corrections` and let CI run the 116 tests on GitHub | GitHub | CI evidence for this revision | Actions run | **Yes: the user decides to push** |
| R-6 | P2 | Split the rest of `IArticleService` (26 members) by reader and editor use; split `AdminController` (12 dependencies) | BLL interfaces, controllers | ISP / SRP smells (`03`) | Build + tests | No |
| R-7 | P2 | Inject `TimeProvider` instead of the static `SiteClock` | `BLL/SiteClock.cs` | Deterministic time in tests | Tests with a frozen clock | No |
| R-8 | P2 | AutoMapper advisory: upgrade to 16.1.1 (Community licence, if the group qualifies) or replace it with hand-written mapping, in its own branch | `BLL.csproj`, `Mapping/MappingProfile.cs` | Removes the advisory; today it is mitigated by `MaxDepth(3)` and guarded by `MappingDepthTests` | `dotnet list package --vulnerable` | Licence decision |
| R-9 | P2 | Demo password `Meridian@2026` appears in the README and development settings | `README.md`, `appsettings.Development.json` | Sample data only. Production settings have no demo accounts (tested). | — | Optional: change it |
| R-10 | P2 | RSS and sitemaps allow proxies to cache for 15 and 60 minutes, so a withdrawn title can stay visible there (its link answers 404) | `SeoController` `[ResponseCache]` | Bounded AP staleness (`08`) | Shorter durations, or `no-cache` with ETag | Decision |
| R-12 | P2 | API clients must now send `version` with `PUT /api/articles/{id}` | Swagger description, README API table | Contract change for any outside script | Swagger page | No |

## Ready-to-send follow-up prompt for Claude Code (if the reviewer agrees)

```text
On branch sadp/review-corrections (do not push or merge unless the user says so):
1. R-3b: make ArticleRepo.Search and the search page call path async (ToListAsync/CountAsync through
   the BLL and the ArticleController.Index action). Keep the layering. Run
   python tools/smoke/smoke.py --stories 5000 before and after and report p50/p95/p99 for 1 and 20 clients.
2. R-12: describe the version field in the Swagger summary of PUT /api/articles/{id} and in the README API table.
3. R-10: set RSS and news-sitemap Cache-Control to 300 s; keep the main sitemap at 3600 s. Add an AppTests check of the header.
Rebuild, run all tests, update sadp_delivery/08, 11, 12, 15 and the handoff. Never touch NewsArchiveDB.
```
