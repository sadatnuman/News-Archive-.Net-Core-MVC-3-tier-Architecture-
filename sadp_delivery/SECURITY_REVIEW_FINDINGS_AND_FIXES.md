# Security Review: Findings and Fixes (branch `sadp/final-hardening`)

**Sources of the findings:**
- The last two reviews: `/ponytail-audit` (8 findings + 4 smaller) and `/ponytail-review` of `sadp/review-corrections` (2 findings).
- The master prompt's phases P0–P2 (P0 = security, P1 = correctness).

**Method.** Each finding was first checked against the code at `main@12d1893`, then fixed, then covered by a test. Paths are relative to `NewsArchiveSolution/`. The line numbers are the final ones on this branch.

## Summary

| ID | Severity | Finding | Final status | Regression test(s) |
|---|---|---|---|---|
| S1 | **P0** | An author could delete other users' pictures by putting their path in a story's "Image path" | **FIXED** | `HardeningTests.Deleting_one_story_never_removes_a_picture_another_story_still_uses`, `A_story_pointing_at_someone_elses_avatar_never_deletes_it`, `Only_canonical_unused_upload_paths_can_ever_be_deleted` |
| S2 | **P0** | Parallel sign-in requests bypassed the 5-per-15-minutes lockout | **FIXED** | `SecurityTests.Twenty_simultaneous_wrong_passwords_get_at_most_five_checks`, `A_correct_password_clears_earlier_failures_and_a_crash_does_not_count`, `HardeningTests.Twelve_parallel_wrong_passwords_get_five_checks_and_then_both_sign_in_paths_are_locked` |
| C1 | P1 | The MVC story form accepted a missing section: 500 error and lost text | **FIXED** | `A_form_for_a_missing_section_keeps_the_text_and_removes_only_its_own_upload`, `Saving_into_a_section_that_vanished_returns_false_instead_of_crashing` |
| C2 | P1 | User deletion ran in separate steps (could lose comments, then fail) | **FIXED** | `Deleting_a_user_removes_their_comments_reports_and_moderator_links_together`, `A_user_delete_that_fails_at_the_last_step_rolls_everything_back` |
| C3 | P1 | Two registrations for one email at the same moment → 500 | **FIXED** | `Two_registrations_for_one_email_at_once_create_exactly_one_account`, `A_duplicate_insert_returns_false_and_the_repository_keeps_working` |
| C4 | P1 | API edit without `version` was told "someone else changed this article" | **FIXED** (400 with guidance) | `The_edit_api_explains_a_missing_version_and_keeps_409_for_real_conflicts`; `AppTests.An_edit_from_a_stale_form_never_overwrites_a_newer_edit` (updated) |
| E1 | P1 (user requirement) | Production without SMTP: one error with stack trace per mail; SMTP selectable by config | **FIXED** (email disabled outside Development) | `Outside_development_mail_is_disabled_even_with_an_smtp_host_and_sends_nothing`, `Newsletter_double_opt_in_works_offline_through_the_private_outbox`, `EmailSenderTests` |
| D1 | P2 | Unused `IArticleService.Get()` (loads every story), `ICommentService.Get()`, `ICommentService.Update()` (could unhide a comment) | **FIXED** (removed) | Build; no caller in code, views or tests |
| D2 | P2 | Clean-up of tokens, audit and sign-ups ran only at start-up | **FIXED** (`MaintenanceJob`, every 24 h) | `Daily_maintenance_removes_expired_tokens_and_old_audit_entries` |
| D3 | P2 | News sitemap loaded every story to keep two days of them | **FIXED** (filter + `TOP` in SQL; main sitemap capped at 45,000) | `The_news_sitemap_lists_only_recent_stories_and_feeds_are_cached_briefly`; smoke timing |
| D4 | P2 | Two reports at the limit could hide a comment and audit it twice | **FIXED** (conditional update) | `CommentServiceTests.Two_reports_that_both_reach_the_limit_hide_and_audit_only_once`, `HardeningTests.Two_reports_cannot_hide_a_comment_twice` |
| D5 | P2 | A delete racing an edit threw an unhandled concurrency exception (500) | **FIXED** | `A_delete_racing_an_edit_fails_cleanly_instead_of_crashing` |
| D6 | P2 | Anyone knowing a confirmed subscriber's email could change their digest language | **FIXED** (no change without the confirmation link) | `NewsletterServiceTests.Knowing_an_email_is_not_enough_to_change_its_digest_language`, offline double-opt-in test |
| D7 | P2 | RSS / news sitemap allowed caching for 15 minutes | **FIXED** (5 minutes) | sitemap/feed test checks `max-age=300` |
| D8 | P2 | AutoMapper 14 advisory GHSA-rvv3-g6hj-g44x | **DEFERRED WITH REASON**: mitigated by `MaxDepth(3)` on every map, guarded by `MappingDepthTests`; the fixed versions (15.1.1/16.1.1) are commercially licensed | `MappingDepthTests.Every_mapping_has_a_depth_limit` |
| D9 | P2 | Sitemap index for more than 45,000 stories | **DEFERRED WITH REASON**: not reachable at the expected size; the cap prevents an invalid file | — |
| J1 | — | Wider scan (Razor raw output, JSON-LD, service worker, suggestions, bookmarks, author pages) | **NOT REPRODUCED**: no new exploitable issue found | see "Wider scan" |

## S1: Cross-user picture deletion (P0) — FIXED

**Before.**
- `MediaService.DeleteIfUploaded` deleted any path that started with `/uploads/` and had no `..`.
- `ArticleFormDTO.ImagePath` accepts typed paths, and both the edit form and the API passed the stored value to the delete.
- Attack: put another user's avatar or story picture path into your own story, then clear it or delete the story.

**After.**
- Two narrow methods (`BLL/Services/MediaService.cs:41`, `:48`):
  - `DeleteArticleImageIfUnused` accepts only `^/uploads/articles/[0-9]{4}/[0-9]{2}/[0-9a-f]{32}\.(jpg|png|gif|webp)$` (`:24`);
  - `DeleteAvatarIfUnused` accepts only `^/uploads/avatars/[0-9a-f]{32}\.(…)$` (`:27`).
- Both refuse when `Articles.ImagePath` (`DAL/Repos/ArticleRepo.cs:109`) or `Users.AvatarPath` (`DAL/Repos/UserRepo.cs:64`) still uses the path.
- Anything else is never deleted: case variants, a missing leading slash, `//`, `..`, backslashes, URL-encoding, `https://`, static files, other extensions.
- `WebRootFileStorage.Resolve` still keeps every delete under `wwwroot/uploads`.
- Callers:
  - `ArticleController.cs:233`, `:270`, `:285`, `:314`;
  - `Api/ArticlesApiController.cs:183`;
  - `Api/AuthApiController.cs:103`, `:107`, `:118`;
  - `AvatarController.cs:47`, `:52`, `:67`.

**Evidence.**
- Real files in an isolated temporary web root (`IsolatedUploads`) are checked for existence and content, not just status codes.
- **Mutation check:** with the reference check removed, `Deleting_one_story_never_removes…` fails; restored, it passes.

**Residual risk.**
- There is a check-then-delete gap of a few milliseconds: a reference added between the check and `File.Delete` would point at a deleted file.
- One server, human-speed edits: accepted and documented. The fix for many servers would be a deferred orphan sweep.

## S2: Lockout bypass by parallel requests (P0) — FIXED

**Before.**
- `IsLocked()` was called first, then the password was checked.
- `RecordFailure()` read the count and wrote count + 1, with no lock.
- 20 parallel requests all passed the gate.

**After.**
- `NewsArchive/Auth/LoginThrottle.cs:41` `Attempt(email, verify)`:
  1. Under a lock, it refuses when `failures + inFlight ≥ 5` (`:47`) and reserves a slot.
  2. It checks the password **outside** the lock.
  3. Under the lock again, it releases the slot and records the result.
- A success clears the failures; an exception releases the slot without counting.
- The same method is used by the website (`AuthController.cs:53`), the API (`Api/AuthApiController.cs:38`) and the current-password check (`Auth/PasswordChanger.cs:48`).

**Evidence.**
- Unit test: exactly 5 of 20 parallel attempts reach the password check.
- Integration test: 12 parallel API attempts give 5 × 401 and 7 × 429. The correct password is then refused (429) on the API, and the MVC form shows the lock message.
- **Mutation check:** with `inFlight` removed from the gate, both concurrency tests fail.

**Residual risk.** The counters live in one process. With 2+ servers each would allow 5. Planned in ADR-005; not implemented.

## C1: Missing section (P1) — FIXED

- **Validation:** `ArticleController.RequireSection` (`:65`) runs before any upload is saved, on both create (`:213`) and edit (`:262`). The form comes back with the text kept and "This section no longer exists…" (EN + BN).
- **Race:** if the section disappears during the save, `ArticleRepo.Create`/`Update` (`DAL/Repos/ArticleRepo.cs:14`, `:27`) catch only SQL error 547 (`DAL/Repos/DbErrors.cs`) and return `false`. The controller shows the section message and deletes only the file uploaded in that request. Other database errors still surface.

## C2: User deletion (P1) — FIXED

- `DAL/Repos/UserRepo.cs:32` runs the whole operation in one transaction (`:34`):
  - the authored-or-reviewed check;
  - deleting the user's reports and comments;
  - clearing their moderator links;
  - removing the user.
- A foreign-key conflict (for example a story written concurrently) rolls everything back and returns `false` (`:49`); the admin sees "reassign or delete them first".
- Any other failure also rolls back, because the transaction is disposed without a commit, and then surfaces.
- **Test with an injected failure:** a temporary trigger on the test database makes the final DELETE fail. Comments, reports and the moderator link are all still there afterwards.

## C3: Duplicate registration (P1) — FIXED

- `UserRepo.Create` (`:19`) catches only SQL errors 2601/2627 (duplicate key) and returns `false`. Registration then answers "Email already exists" (409 on the API) or "Failed to create user" (MVC).
- A real outage is not turned into "email exists".
- **Tests:** two parallel registrations give exactly one 201 and one 409, one row and one audit entry. A duplicate insert in another scope returns `false`, and that repository still works afterwards.

## C4: API version contract (P1) — FIXED

| Case | Answer |
|---|---|
| `version` missing or malformed (`ArticleRules.IsWellFormedVersion`, `BLL/Rules/ArticleRules.cs:28`) | **400** ValidationProblem, key `version`: "Send the version returned by GET /api/articles/{id} in the JSON property "version"." (`Api/ArticlesApiController.cs:146-150`) |
| Well-formed but stale | 409 "Someone else changed this article…" |
| No such article / anonymous / not allowed | 404 / 401 / 403 |
| Current version | 204 |

- The Swagger description says the same (`[EndpointDescription]`, `:136`); a test checks `swagger.json`.
- The README has a curl example and an answer table.
- The MVC hidden field is unchanged.

## E1: Offline email (user requirement) — see `EMAIL_OFFLINE_MODE_AND_TESTS.md`

## Wider scan (J1): no new exploitable issue

| Area | Result |
|---|---|
| Razor raw output | Only `_SeoHead.cshtml:140` JSON-LD, built with `System.Text.Json` default escaping (`<`, `>`, `&` are escaped), so `</script>` cannot break out |
| Service worker | Does not cache HTML navigations or private paths (`/api`, `/Admin`, `/Auth`, `/Member`, …); caches static files and pictures only |
| Search suggestions, author list, bookmarks | All filter on `StatusId == Published` (`ArticleRepo.Suggest`, `UserRepo.SuggestAuthors`, `BookmarkService.cs:27`, `BookmarkRepo.cs:59`) |
| Antiforgery | Global `AutoValidateAntiforgeryToken`. The API ignores it but uses bearer tokens or a `SameSite=Strict`, `Path=/api` cookie. The one-click unsubscribe POST ignores it by design (RFC 8058). |
| Not inspected in depth | `site.js` beyond the service worker, analytics queries, CI workflow, HTTPS/proxy trust on a real host |
