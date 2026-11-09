# Test Matrix and Commands (branch `sadp/final-hardening`)

All commands were run on 2026-11-09 on this machine:
- Windows 11, AMD Ryzen 5 7520U (8 logical CPUs), 13.8 GB RAM;
- .NET SDK 10.0.403;
- SQL Server 2025 Express (`localhost\SQLEXPRESS`).

Commands run from `NewsArchiveSolution/` unless they say otherwise.

## Build and automated tests

| Step | Command | Exit | Result |
|---|---|---|---|
| Baseline (`main@12d1893`) | `dotnet build NewsArchiveSolution.slnx -c Release` | 0 | 0 warnings, 0 errors |
| Baseline | `dotnet test NewsArchive.Tests -c Release --no-build` | 0 | 116 passed, 0 failed, 0 skipped |
| Final build | `dotnet build NewsArchiveSolution.slnx -c Release` | 0 | 0 warnings, 0 errors |
| Final tests | `dotnet test NewsArchive.Tests -c Release --no-build` | 0 | **137 passed, 0 failed, 0 skipped** (about 10 s) |
| Mutation check (P0 tests) | Temporarily removed the reference check in `MediaService` and `InFlight` from the lockout gate, rebuilt, ran the 3 P0 tests, then restored | — | All 3 **failed** without the fix and pass with it |
| Pending model changes | `dotnet ef migrations has-pending-model-changes --project DAL --startup-project NewsArchive` | 0 | "No changes have been made to the model since the last migration." No migration was added in this phase. |
| Vulnerable packages | `dotnet list NewsArchiveSolution.slnx package --vulnerable --include-transitive` | 0 | Only AutoMapper 14.0.0, GHSA-rvv3-g6hj-g44x (High), already known; DAL clean |
| Whitespace | `git diff --check` | 0 | No whitespace errors (only LF→CRLF notices) |
| Secrets scan | `git grep -nE "(Password|Pwd|ApiKey|Secret)\s*[:=]\s*\"[^\"]{6,}"` over tracked files | — | No matches apart from the test passwords. Note: the pattern does not match JSON keys like `"Password": "…"`; the known demo password `Meridian@2026` in `appsettings.Development.json` and the README is pre-existing sample data (R-9). |

Test count by area: 137 =
- 116 earlier;
- \+ 2 lockout unit tests;
- \+ 1 auto-hide race unit test;
- \+ 1 newsletter language unit test;
- \+ 17 `HardeningTests`.

## New and changed tests (expected vs actual)

| Test | Expected | Actual |
|---|---|---|
| `HardeningTests.Deleting_one_story_never_removes_a_picture_another_story_still_uses` | Shared file kept after the copy is deleted; deleted with the last reference | Pass |
| `HardeningTests.A_story_pointing_at_someone_elses_avatar_never_deletes_it` | Avatar file kept | Pass |
| `HardeningTests.Only_canonical_unused_upload_paths_can_ever_be_deleted` | 13 unsafe path shapes refused; planted files kept; canonical unused files deleted | Pass |
| `HardeningTests.A_form_for_a_missing_section_keeps_the_text_and_removes_only_its_own_upload` | 200, title shown, section message, bystander file kept, no orphan | Pass |
| `HardeningTests.Saving_into_a_section_that_vanished_returns_false_instead_of_crashing` | `false`, no exception | Pass |
| `SecurityTests.Twenty_simultaneous_wrong_passwords_get_at_most_five_checks` | Exactly 5 checks, 15 locked | Pass |
| `SecurityTests.A_correct_password_clears_earlier_failures_and_a_crash_does_not_count` | Success resets; an exception is not counted | Pass |
| `HardeningTests.Twelve_parallel_wrong_passwords_get_five_checks_and_then_both_sign_in_paths_are_locked` | 5 × 401, 7 × 429; the right password then gets 429; the MVC form shows the lock | Pass |
| `HardeningTests.Two_registrations_for_one_email_at_once_create_exactly_one_account` | 1 × 201, 1 × 409, one row, one audit entry | Pass |
| `HardeningTests.A_duplicate_insert_returns_false_and_the_repository_keeps_working` | `false`, then a query still works | Pass |
| `HardeningTests.Deleting_a_user_removes_their_comments_reports_and_moderator_links_together` | All removed or cleared | Pass |
| `HardeningTests.A_user_delete_that_fails_at_the_last_step_rolls_everything_back` | Injected trigger failure → user, comment, report and moderator link all still there | Pass |
| `HardeningTests.The_edit_api_explains_a_missing_version_and_keeps_409_for_real_conflicts` | 400 (missing), 400 (malformed), 409 (stale), 404, 403, 401, 204; Swagger text present | Pass |
| `AppTests.An_edit_from_a_stale_form_never_overwrites_a_newer_edit` (changed) | A missing version now gives 400 (was 409) | Pass |
| `HardeningTests.A_delete_racing_an_edit_fails_cleanly_instead_of_crashing` | `false`, story kept | Pass |
| `CommentServiceTests.Two_reports_that_both_reach_the_limit_hide_and_audit_only_once` | One audit entry | Pass |
| `HardeningTests.Two_reports_cannot_hide_a_comment_twice` | `true` then `false` | Pass |
| `HardeningTests.Daily_maintenance_removes_expired_tokens_and_old_audit_entries` | Both rows gone; interval 24 h | Pass |
| `HardeningTests.The_news_sitemap_lists_only_recent_stories_and_feeds_are_cached_briefly` | Old story only in the main sitemap; RSS `max-age=300` | Pass |
| `NewsletterServiceTests.Knowing_an_email_is_not_enough_to_change_its_digest_language` | Language unchanged | Pass |
| `HardeningTests.Outside_development_mail_is_disabled_even_with_an_smtp_host_and_sends_nothing` | `DisabledEmailSender`; 2,000 × `false`; 0 new warnings; 1 start-up warning; no SMTP-port socket | Pass |
| `HardeningTests.Newsletter_double_opt_in_works_offline_through_the_private_outbox` | One outbox file; confirm, language kept, unsubscribe | Pass |

## Smoke run (loopback, disposable database)

**Command (repository root):** `python tools/smoke/smoke.py --stories 5000`

**Guards** (checked separately with 9 cases, all correct):
- Only `NewsArchiveSmokeDB*` / `NewsArchiveTestDB*` names on a local server are accepted.
- `NewsArchiveDB`, look-alike names and remote servers are refused.
- `Email__*`/`SMTP*` environment variables are removed.
- The app runs in Development, so mail goes to the private outbox, which the script cleans afterwards.

**Result: 45/45 checks passed, benchmark errors 0, exit code 0.** The checks cover:
- pages in English and Bangla, health, RSS, sitemap, Swagger;
- the 302/401/403 barriers;
- author → admin approve / reject / schedule;
- 409 for a decided story;
- the API stale edit (409) and missing version (400);
- the MVC stale form;
- offline email: confirmation file, confirm link, admin digest to the outbox, reset code accepted (code not printed), the unknown-email answer is the same and sends no mail;
- withdrawal from page, API, list and RSS;
- the scheduler publishing by itself.

**Benchmark (same run):**
- **Dataset:** 5,024 published stories, 915,558 word rows.
- **Workload:** 30 fixed search terms; 400 sequential requests; 800 requests from 20 threads; 50 requests to each sitemap.

| Measurement | n | p50 | p95 | p99 | Throughput | Errors |
|---|---|---|---|---|---|---|
| Search page, sequential | 400 | 46.7 ms | 66.9 ms | 71.0 ms | 21.0 req/s | 0 |
| Search page, 20 concurrent | 800 | 476.8 ms | 784.2 ms | 1,028.0 ms | 38.9 req/s | 0 |
| News sitemap (last 2 days, SQL-filtered) | 50 | 56.2 ms | 80.5 ms | 81.2 ms | 17.6 req/s | 0 |
| Main sitemap (≤ 45,000 stories) | 50 | 128.2 ms | 150.4 ms | 297.2 ms | 7.6 req/s | 0 |

Repeatability: three 5,000-story runs on this machine gave 20-client throughput of 43.0, 40.8 and 38.9 req/s and p95 of 672.5, 727.8 and 784.2 ms. The final run (above) is the one reported.

**Limits:**
- one run;
- client and server on one machine;
- synthetic text;
- SQL Server Express;
- no query plans were captured, so no "index seek" claim is made for the sitemap query;
- this is a headless HTTP check, not a visual browser test.

## Not run

- GitHub Actions on this branch: **GitHub CI not run on the new commit** (not pushed).
- Any real email, SMTP connection, deployment, HTTPS on a host, two-instance test or uptime measurement.
