# 15 · Remaining Actions

**Priority scale:**
- **P0** blocks the midterm submission or is a known exploitable defect.
- **P1** matters before a public launch or the Week 12 checkpoint.
- **P2** is good engineering and can wait.

## Closed in the final hardening (`sadp/final-hardening`)

See `SECURITY_REVIEW_FINDINGS_AND_FIXES.md` for the evidence.

| ID | Item | Closed by |
|---|---|---|
| S1 | Cross-user picture deletion | Canonical paths + reference check; 3 tests |
| S2 | Parallel lockout bypass | `LoginThrottle.Attempt` reservation; 3 tests |
| C1 | Missing section → 500 and lost text | Validation + foreign-key catch; 2 tests |
| C2 | Non-atomic user delete | Transaction with rollback; 2 tests |
| C3 | Duplicate registration → 500 | Duplicate-key catch; 2 tests |
| C4 / R-12 | API missing `version` gave a false 409 | 400 with guidance, Swagger text, README; 1 test |
| E1 | Email log storm, SMTP selectable | Disabled outside Development (ADR-007); 2 tests |
| D1–D7 | Dead methods, start-up-only purge, sitemap query, double auto-hide, delete/edit race, newsletter language, feed caching | See the security file |
| R-T2, R-V1, R-2, R-3, R-5 | Earlier revisions | Done |

## Still open

| ID | Priority | Item | Impact | How to verify | Needs from the user |
|---|---|---|---|---|---|
| R-T1 | **P0 (submission)** | Member IDs, names, group, section, date; confirm module owners and group size | The report cannot be submitted with [TBD] | Read the report | **Yes: personal data** |
| R-T3 | **P0 (submission)** | Convert the Markdown draft to the required DOCX/PDF with Figure 1 embedded | Submission format | Open the file | Format choice |
| R-T4 | P1 | Check the 4 book references against library copies | Citation accuracy | Title pages | Library access |
| R-11 | P1 | Push `sadp/final-hardening` and open a pull request so CI runs the 137 tests | CI evidence for this commit | Actions run | **Yes: approval to push** |
| R-1 | P1 | Shared store for lockout, reset codes, session and cache invalidation before 2+ instances | Lockout is per process today | Two-factory integration test | SQL or Redis choice |
| R-3b | P1 | Async EF on the search path; re-measure (20-client throughput 38.9–43.0 req/s, near the 40 req/s goal) | Performance and scalability | `tools/smoke/smoke.py --stories 5000` before/after | No |
| R-4 | P1 | Production deployment (TrustAllProxies, HTTPS, new sample passwords) | Availability evidence | Uptime monitor | **Yes: host and credentials** |
| R-E1 | P2 (scope, not a defect) | Real email delivery: not implemented by decision. Adding it needs a provider, secrets handling, a registered sender, and tests that do not send to real people. | Public newsletter confirmation and password reset by email do not work | — | Owner's future decision |
| R-6 | P2 | Split `IArticleService` (26 members) and `AdminController` (12 dependencies) | ISP/SRP smells | Build + tests | No |
| R-7 | P2 | Inject `TimeProvider` instead of the static `SiteClock` | Deterministic time tests | Frozen-clock tests | No |
| R-8 | P2 | AutoMapper advisory: upgrade to 16.1.1 under a Community licence, or hand-written mapping | Removes the advisory | `dotnet list package --vulnerable` | Licence decision |
| R-9 | P2 | Change the demo password `Meridian@2026` (dev settings and README) | Sample data only; not in Production settings | — | Optional |
| R-13 | P2 | Picture delete has a millisecond check-then-delete gap; a periodic orphan sweep would remove it | Very small at one server | — | No |
| R-14 | P2 | Sitemap index beyond 45,000 stories | Not reachable at the expected size | — | No |

## Ready-to-send follow-up prompt for Claude Code (only if the reviewer agrees)

```text
On branch sadp/final-hardening (do not push or merge unless the user says so; never touch NewsArchiveDB; no real email):
1. R-3b: make ArticleRepo.Search, its service method and ArticleController.Index async (ToListAsync/CountAsync),
   keeping the layering. Run python tools/smoke/smoke.py --stories 5000 three times before and three times after;
   report p50/p95/p99 and throughput for 1 and 20 clients.
2. R-1 (design only): write ADR-005 v2 comparing a SQL table vs Redis for lockout counters and reset codes,
   with the reservation semantics of LoginThrottle.Attempt preserved.
Rebuild, run all tests, update sadp_delivery/08, 11, 15, TEST_MATRIX_AND_COMMANDS.md and the handoff.
```
