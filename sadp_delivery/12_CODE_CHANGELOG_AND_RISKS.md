# 12 · Code Changelog and Risks

## Revision 3: `sadp/final-hardening` (from `main@12d1893`)

The full file list, commits, rollback and risks are in `FINAL_CHANGELOG.md`; findings and tests are in `SECURITY_REVIEW_FINDINGS_AND_FIXES.md`.

**Summary:**
- P0 cross-user picture deletion and P0 parallel lockout bypass: fixed.
- P1 missing section, user delete, duplicate registration and API `version`: fixed.
- Email disabled outside Development (ADR-007).
- Seven P2 items fixed.
- 137 tests; smoke 45/45; no new migration.

## Revision 2: `sadp/review-corrections` (from `main@484deb3`)

**State.** Pushed by the user and merged into `main` as `12d1893` (pull request #10).

| # | Review point → problem | Design choice | Files | Tests | Result | Trade-off / rollback |
|---|---|---|---|---|---|---|
| R2-1 | P2.1 Stale edits: a form opened long ago could overwrite a newer edit (`02` F3) | A version string `StatusId-UpdatedAt.Ticks` (`ArticleRules.Version`) is sent with the edit form (hidden field) and the API (`ArticleDTO.Version`, `ArticleFormDTO.Version`). `ArticleService.Update` refuses a mismatch before writing. `UpdatedAt` also becomes an EF concurrency token, so two saves in flight from the same version cannot both pass. | `BLL/Rules/ArticleRules.cs:23`, `BLL/Services/ArticleService.cs:85–121`, `BLL/DTOs/ArticleDTO.cs`, `BLL/DTOs/ArticleFormDTO.cs`, `NewsArchive/Views/Article/_ArticleForm.cshtml:16`, `DAL/EF/NewsArchiveDbContext.cs:92`, migration `20261109152432_ArticleEditConcurrencyToken` (empty) | `An_edit_from_a_stale_form_…`, `Two_saves_in_flight_…`; smoke (API + MVC) | Pass | **API change:** `PUT /api/articles/{id}` now needs the `version` from `GET`; without it the answer is 409. View-count updates do not touch `UpdatedAt`, so readers never cause false conflicts. Rollback: revert these files and remove the empty migration. |
| R2-2 | P2.2 409 could not tell a race from a wrong state | `ChangeResult { Done, NotAllowed, Conflict }` returned by `Update`, `Approve`, `Reject`, `PublishNow`, `Unschedule`. Conflict → "Another editor acted on this story…" / "Someone else changed this article…"; NotAllowed keeps the old messages. Bangla translations added. | `BLL/ChangeResult.cs` (new), `BLL/Interfaces/IArticleService.cs`, `NewsArchive/Controllers/AdminController.cs:105`, `Api/ArticlesApiController.cs` (`Update`, `Decided`), `ArticleController.cs` (Edit), `Resources/SharedResource.bn.resx` | `A_decision_racing_another_editor_…`; extended `Author_submission_stays_hidden_…` | Pass | Five method signatures changed from `bool` to `ChangeResult`; all callers updated (build 0 errors). |
| R2-3 | P2.3 Production email reported success when nothing was sent (`02` F4) | `OutboxTransport` throws "No email transport is configured" when there is no outbox folder (outside Development). `EmailSender.SendAsync` turns it into `false` and logs an error. Pages still answer normally; the password-reset page still answers the same whether or not the account exists. | `NewsArchive/Email/OutboxEmailSender.cs:39` | `EmailSenderTests.Development_outbox_writes_the_message_and_production_without_smtp_reports_failure` | Pass | The digest no longer marks subscribers as sent when nothing was sent, so they are tried again next time. One error log line per attempted mail in a misconfigured Production. |
| R2-4 | P1.2 M4's ISP was weak | New `IArticleArchive` (3 members). `ArticleService` implements it; the 3 methods left `IArticleService` (29 → 26 members). `ArchiveController`, `ArchiveApiController` and `SeoController` depend on it. Registered in `BusinessLayerServices.cs:17`. | `BLL/Interfaces/IArticleArchive.cs` (new), `BLL/Interfaces/IArticleService.cs`, `BLL/Services/ArticleService.cs:12`, three controllers | `Archive_pages_depend_only_on_the_narrow_archive_contract` | Pass | One more scoped registration (a second `ArticleService` instance per request when both interfaces are used; it shares the request's `DbContext`). The Abstract Factory is untouched. |
| R2-5 | P2.5 Stale caches must not keep withdrawn content | No code change was needed: invalidation runs after every successful write, and version keys stop late writes. Tests were added to prove it. | — | `A_withdrawn_story_disappears_…`, `MemoryAppCacheTests` (2) | Pass | RSS and sitemap `Cache-Control` (15 and 60 min) let proxies keep a withdrawn title; documented in `08` and `15` R-10. |
| R2-6 | P3.5 AutoMapper advisory | No upgrade (15+ is commercially licensed, and a major-version change is untested). A test makes sure every map keeps `MaxDepth`. | `NewsArchive.Tests/Security/MappingDepthTests.cs` (new) | 1 test | Pass | Advisory still reported; mitigation and upgrade path in `08`. |
| R2-7 | P3.3 Reproducible smoke | `tools/smoke/smoke.py` (standard library + `sqlcmd`). It refuses database names without `Smoke`/`Test`, reads demo passwords from the dev settings, and sends no real email. | `tools/smoke/smoke.py` (new) | 40 checks | 40/40 | Drops and recreates the smoke database on each run (only guarded names). |
| R2-8 | P1.5–1.6 C4 review and rendering | Diagrams corrected; `render_c4.py` writes SVG and PNG (local Chrome, nothing installed). | `sadp_delivery/09_C4_AND_4PLUS1/*` | Checked by eye | — | — |
| R2-9 | Documentation | README (test count, smoke script), ARCHITECTURE (version check, `IArticleArchive`, email behaviour, migrations, tests, limits) and every `sadp_delivery` file listed in the handoff | — | — | — | — |

**Impact review (P2.4):**

| Area | Effect |
|---|---|
| Published-content visibility | Unchanged. `CanView`, the list filters and the search filters were not touched. A refused edit writes nothing. |
| Authorization | Unchanged. The `CanEdit` checks run before the version check; a forbidden user still gets "You cannot edit this article" (MVC) or 403 (API, checked in the controller first). |
| Audit log | Written only on `Done`. A conflict writes no audit entry and does not clear the cache, because nothing changed. |
| Scheduling | `PublishDue` uses `ExecuteUpdate`, which does not use concurrency tokens; it cannot be blocked by an editor. An editor whose form was opened before the scheduler published the story gets a conflict (the status part of the version changed) instead of silently pushing the story back to Draft or Pending. |

## Revision 1: `sadp/midterm-readiness` (from `develop@c9f08a3`)

**State.** This revision was later pushed to GitHub and merged into `main` by the user as `484deb3` (pull request #9).

| # | Requirement → problem | Design choice | Files | Tests | Result |
|---|---|---|---|---|---|
| C1 | §9 Builder | `ArticleBuilder` + director `ArticleService.Create` | `BLL/Builders/ArticleBuilder.cs`, `ArticleService.cs:60` | 7 | Pass |
| C2 | §9 Factory Method + OCP | `EmailSender.CreateTransport()` with SMTP and outbox products | `NewsArchive/Email/*` | 5 | Pass |
| C3 | §3 Reliability (two reviewers) | `StatusId` concurrency token; empty migration | `NewsArchiveDbContext.cs:91`, `Repository.cs:47` | 1 | Pass |
| C4 | Defect F1 | Case-insensitive language code | `BLL/AppRoles.cs:27` | builder test | Pass |
| C5 | Abstract Factory / Singleton evidence | Tests only | `Integration/AppTests.cs` | 2 | Pass |
| C6 | Rate limits in tests | Test-only client-IP startup filter | `Integration/AppFactory.cs` | — | Pass |

## Safety record (both revisions)

- No force push, no merge by Claude Code, no remote change. Revision 1 was pushed as a new branch at the user's request; revision 2 is local only.
- No deployment.
- No email sent: SMTP host empty, outbox only.
- No destructive migration (both new migrations are empty).
- The user's `NewsArchiveDB` was not touched (28 articles). The new migration was not applied to it; Visual Studio will apply it on the next Development start, and it changes no schema.
- The sample-data script and the 5,000 generated stories went only into the disposable `NewsArchiveSmokeDB`.
- No secret was added. The smoke script reads the existing development demo password at run time; it is not stored in the script.

## Residual risks

- R2-1: API clients that never send `version` can no longer edit. This is intended, but it is a contract change for any external script.
- R2-2: the MVC flash for a decision race is reached only by a true race and is checked by the service test, not by an HTTP test.
- R2-3: real SMTP delivery is still untested.
