# 02 · Feature Inventory and Functional Requirements

Each feature was checked in the code, not taken from the README. Paths are relative to `NewsArchiveSolution/`.

## How each claim is labelled

| Label | Meaning |
|---|---|
| PROVEN BY TEST/RUNTIME | Covered by a passing automated test, or seen working in the smoke run (`11_TEST_EXECUTION_AND_RUNTIME_PROOF.md`) |
| PROVEN BY CODE | The code exists and was read, but no test exercises it |
| DOCUMENTATION ONLY | Written down, nothing in the code |
| PROPOSED | A plan, not built |

## Actors

| Actor | Role ID | Notes |
|---|---|---|
| Reader | 2 | Signs up freely |
| Author | 3 | Set by an admin |
| Admin | 1 | Editor and moderator |
| Anonymous visitor | — | Not signed in |
| Scheduler | — | Background job (`Publishing/ScheduledPublisher.cs:5`) |
| API client | — | Bearer token |

## Functional requirements (12; the proposal needs at least 6)

| ID | Requirement (one testable statement) | Actor | Priority | Controller / endpoint | BLL | DAL | Authorization | Evidence status | Test |
|---|---|---|---|---|---|---|---|---|---|
| FR-1 | An author can write an article and save it as a draft or submit it for review. | Author | Must | `Controllers/ArticleController.cs:191` (Create), `Controllers/Api/ArticlesApiController.cs:113` | `ArticleService.Create` (`BLL/Services/ArticleService.cs:55`), `ArticleBuilder.Build` (`BLL/Builders/ArticleBuilder.cs:76`) | `ArticleRepo` | `[ApiAuthorize(Admin, Author)]`; readers get 403 | PROVEN BY TEST/RUNTIME | `ArticleBuilderTests` (7), `AppTests.Author_submission_stays_hidden_until_an_admin_approves_it`, smoke "author submits a story (201)", "reader cannot create an article (403)" |
| FR-2 | An admin can approve a pending article, publishing it now or at a chosen time, or reject it with a note the author can read. | Admin | Must | `Api/ArticlesApiController.cs:197` approve, `:227` reject; `AdminController.cs:64` | `ArticleService.Approve` `:151`, `.Reject` `:209` | `ArticleRepo`, `Repository.Update` (concurrency check `DAL/Repos/Repository.cs:47`) | Admin only | PROVEN BY TEST/RUNTIME | `AppTests.Two_reviewers_cannot_both_decide_the_same_article`; smoke: approve 204, reject 204 + note visible, second decision 409, author approve 403 |
| FR-3 | A story becomes public only after approval; a scheduled story goes live by itself at its time. | Reader, Scheduler | Must | `ArticleController.cs:132` Details; `ScheduledPublisher` | `ArticleRules.CanView` (`BLL/Rules/ArticleRules.cs:17`), `ArticleService.PublishDue` `:193` | `ArticleRepo.PublishDue` (`DAL/Repos/ArticleRepo.cs:259`, atomic `ExecuteUpdate` with `WHERE StatusId=Scheduled`) | Public only when Published | PROVEN BY TEST/RUNTIME | smoke: pending 404 → 200 after approval; scheduled 404, then published by the job after 85 s |
| FR-4 | A reader can search stories by words (English or Bangla) and by date, and browse sections, topics, authors and a date archive. | Reader | Must | `ArticleController.cs:74`, `ArchiveController.cs:22`, `Api/ArticlesApiController.cs:31`, `Api/SearchApiController.cs:25` | `ArticleService.Search`, `SearchRules.QueryTerms/IndexWords` (`BLL/Rules/SearchRules.cs:53`) | `ArticleRepo.Search` `:101` over `ArticleWords` | Public | PROVEN BY TEST/RUNTIME | `AppTests.Search_finds_published_stories_by_word_start`, `RuleTests.Search_words_keep_bangla_marks…`, smoke search check |
| FR-5 | While reading, a reader gets "Up next" and "More like this" with the reason, and signed-in readers get personal recommendations from their history. | Reader | Should | `ArticleController.cs:132`, `Api/ReadingApiController.cs:20` | `RecommendationService` (`BLL/Services/RecommendationService.cs:89`, `:157`) | `ArticleRepo.GetRecommendationCandidates`, `ReadingHistoryRepo` | Public / signed in | PROVEN BY TEST/RUNTIME | `RecommendationServiceTests` (6) |
| FR-6 | A signed-in user can comment and report a comment; three reports hide it until an admin keeps or removes it. | Reader, Admin | Must | `CommentController.cs:37`, `Api/CommentsApiController.cs:45`, `AdminController.cs:200` | `CommentService.Report` (`BLL/Services/CommentService.cs:85`), `ModerationRules.AutoHideThreshold=3` | `CommentRepo` | Signed in; moderation Admin only | PROVEN BY TEST/RUNTIME | `CommentServiceTests` (9) |
| FR-7 | A visitor can subscribe to the weekly digest with double opt-in and unsubscribe in one click; an admin sends the digest. | Visitor, Admin | Should | `NewsletterController.cs:31`, `:57`; `AdminController.cs:159` | `NewsletterService.Subscribe` `:37`, `.BuildDigest` `:161`; `DigestSender` (`NewsArchive/Email/DigestSender.cs:25`) | `SubscriberRepo` | Admin for sending | PROVEN BY TEST/RUNTIME | `NewsletterServiceTests` (8), smoke: subscribe wrote exactly one outbox file |
| FR-8 | A user can register, sign in, change their password (with the current one), reset it by emailed code, and sign out. | Reader | Must | `AuthController.cs:47`, `:120`; `Api/AuthApiController.cs:30` | `AuthService.Authenticate` (`BLL/Services/AuthService.cs:27`), `PasswordChanger`, `PasswordResetStore` | `UserRepo`, `TokenRepo` | Antiforgery and rate limits | PROVEN BY TEST/RUNTIME | `SecurityTests` (8), `AppTests` (lockout, antiforgery) |
| FR-9 | A signed-in user can save stories and see and clear their reading history. | Reader | Could | `BookmarkController.cs:34`, `HistoryController.cs:24`, `Api/BookmarksApiController.cs:30` | `BookmarkService`, `RecommendationService.RecordRead` | `BookmarkRepo`, `ReadingHistoryRepo` | Own data only | PROVEN BY CODE (earlier HTTP scripts; no committed test) | — |
| FR-10 | An admin can see an audit log of editorial, moderation, account and sign-in actions, filtered by type and person. | Admin | Should | `AdminController.cs:131`, `Api/AuditApiController.cs:20` | `AuditService` (`BLL/Services/AuditService.cs:9`) | `AuditLogRepo` | Admin only | PROVEN BY TEST/RUNTIME | `AuditServiceTests` (4) |
| FR-11 | Every page, message and email is available in English and Bangla. | All | Must | Razor `L[...]`, `?lang=bn` | `MailComposer`, `AppLanguages.Normalize` (`BLL/AppRoles.cs:27`) | Article `LanguageCode` | — | PROVEN BY TEST/RUNTIME (pages) | smoke `GET /?lang=bn` 200; `ArticleBuilderTests` (language normalised) |
| FR-12 | Apps can use the same features through a token-protected REST API documented at `/swagger`. | API client | Should | `Controllers/Api/*` | same services | same repos | `ApiAuthorize` + `TokenAuthorizationFilter` | PROVEN BY TEST/RUNTIME | `AppTests.Api_checks_roles`, smoke 401/403/200 |

## Editorial lifecycle (as built)

```text
Draft ──submit──► Pending ──approve(no time)──► Published
  ▲                 │  └──approve(future time)──► Scheduled ──PublishDue / publish-now──► Published
  └──edit/resubmit──┴──reject(note)──► Rejected                │
                                                    unschedule ─┘──► Pending
```

The lifecycle lives in `int StatusId` plus guard clauses in `ArticleService`. It is not the GoF State pattern (see `07`).

## Defects found while checking features

| # | Defect | Evidence | Action |
|---|---|---|---|
| F1 | `AppLanguages.Normalize("BN")` returned `en`, so an API client sending `"BN"` got an English article. | Found by the new `ArticleBuilderTests` | **Fixed:** the comparison now ignores case (`BLL/AppRoles.cs:27`). |
| F2 | Two admins deciding the same pending story at once could both "win", because the last write overwrote the other. | Code reading of `Approve`/`Reject` | **Fixed:** concurrency token on `StatusId`; test added. |
| F3 | Editing an article form opened long ago can overwrite a newer edit of the same story, because the token only guards status changes. | Code reading of `ArticleService.Update` | **Fixed in `sadp/review-corrections`:** the edit form and the API carry a version (`StatusId` + `UpdatedAt`); `UpdatedAt` is also a concurrency token. A stale form gets a conflict message (MVC) or 409 (API). Tests: `AppTests.An_edit_from_a_stale_form_never_overwrites_a_newer_edit`, `Two_saves_in_flight_from_the_same_version_cannot_both_win`; smoke (API and MVC). |
| F4 | In Production without SMTP settings, emails are only logged, yet `SendAsync` returns `true`. | `NewsArchive/Email/OutboxEmailSender.cs` | **Fixed in `sadp/review-corrections`:** outside Development with no SMTP host, `SendAsync` now returns `false` and logs "No email transport is configured". Pages still answer normally, and the password-reset page still gives the same answer whether or not the account exists. Test: `EmailSenderTests.Development_outbox_writes_the_message_and_production_without_smtp_reports_failure`. |
