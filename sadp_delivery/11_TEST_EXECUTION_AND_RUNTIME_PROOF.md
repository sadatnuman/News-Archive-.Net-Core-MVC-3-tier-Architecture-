# 11 · Test Execution and Runtime Proof

The full command table, expected-vs-actual results and benchmark are in `TEST_MATRIX_AND_COMMANDS.md`. This file summarises them and keeps the history across revisions.

## Current state (branch `sadp/final-hardening`, 2026-11-09)

| Item | Result |
|---|---|
| Release build | 0 warnings, 0 errors |
| Automated tests | **137 passed, 0 failed, 0 skipped** |
| Mutation check | The 3 P0 regression tests fail when the fix is removed and pass when it is restored |
| EF model vs migrations | No pending model changes; no migration added in this phase |
| Vulnerable packages | Only AutoMapper 14.0.0 (GHSA-rvv3-g6hj-g44x), mitigated by `MaxDepth(3)` and a test |
| `git diff --check` | Clean |
| Smoke (`python tools/smoke/smoke.py --stories 5000`) | **45/45 checks, benchmark errors 0, exit 0** |
| Search, 1 user (5,024 stories) | p50 46.7 / p95 66.9 / p99 71.0 ms, 21.0 req/s |
| Search, 20 users | p50 476.8 / p95 784.2 / p99 1,028.0 ms, 38.9 req/s, 0 errors |
| News sitemap / main sitemap | p95 80.5 ms / 150.4 ms |
| GitHub CI on the new commit | **Not run** (not pushed) |

## Databases used

| Database | Purpose | Notes |
|---|---|---|
| `NewsArchiveTestDB` | Integration tests | One test adds a temporary trigger to inject a failure; it is dropped in `finally` |
| `NewsArchiveSmokeDB` | Smoke run and benchmark | Dropped and recreated by the guarded script; sample data plus 5,000 generated stories |
| `NewsArchiveDB` | The user's data | **Not touched.** Checked with `sqlcmd`: 28 articles; last applied migration `20261109091901_AddArticleWords`. None of the later migrations were applied. |

## Email during testing

- No real email was sent and no SMTP connection was made.
- Development tests and the smoke script write to the private `App_Data/outbox` with synthetic `@example.test` recipients; both delete their own files.
- The Production-mode test proves that no transport is created and no socket opens to an SMTP port.
- Thirteen older outbox files from earlier sessions (`example.com` / `.local` addresses) remain in the gitignored folder. They were left untouched.

## History

| Revision | Tests | Smoke | 20-client search p95 |
|---|---|---|---|
| 1 `sadp/midterm-readiness` (31 stories) | 108 | 32/32 | 621.9 ms |
| 2 `sadp/review-corrections` (5,024 stories) | 116 | 40/40 | 672.5 ms |
| 3 `sadp/final-hardening` (5,024 stories) | 137 | 45/45 | 784.2 ms (two other runs: 727.8, 672.5 ms) |

## Not verified

- A deployment, HTTPS on a host, real email, two app instances, uptime.
- A visual browser test: the smoke checks HTTP responses and HTML text only.
- GitHub Actions on this branch.
