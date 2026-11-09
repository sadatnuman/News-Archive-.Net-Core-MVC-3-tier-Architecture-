# Final Changelog (`sadp/final-hardening`)

| Item | Value |
|---|---|
| Base | `main` at `12d1893913787c12ff429f4b73809426c9d6d73f` (PR #10 merged; baseline 116/116 tests, build 0/0) |
| Branch | `sadp/final-hardening` (local) |
| Code + documents commit | `c1dfb82ef1985b89732f57cd5c8d32cf89b22b0a` |
| Handoff commit | The next commit on the same branch (`git log -1 -- sadp_delivery/CLAUDE_FINAL_COPY_TO_CHATGPT.txt`) |
| Pushed / merged / deployed | **No / No / No** |
| Size, code + README/ARCHITECTURE + tools | 47 files, +1,147 / −149 |
| Size, `sadp_delivery/` | 25 files, +610 / −238 at `c1dfb82` (more in the handoff commit) |
| Migrations | **None added.** `dotnet ef migrations has-pending-model-changes` reports no changes. |
| Database safety | `NewsArchiveDB` not touched (28 articles; last migration `20261109091901_AddArticleWords`). Only `NewsArchiveTestDB` and `NewsArchiveSmokeDB` were used. |
| Email safety | No real email, no SMTP connection, no provider, no credentials; synthetic `@example.test` recipients only |

## Files changed (code)

| Area | Files | Change |
|---|---|---|
| Media (S1) | `BLL/Services/MediaService.cs`, `BLL/Interfaces/IMediaService.cs`, `BLL/Rules/MediaLimits.cs`, `DAL/Interfaces/IArticleRepository.cs`, `DAL/Interfaces/IUserRepository.cs`, `DAL/Repos/ArticleRepo.cs`, `DAL/Repos/UserRepo.cs`; callers in `ArticleController`, `Api/ArticlesApiController`, `Api/AuthApiController`, `AvatarController` | `DeleteArticleImageIfUnused` / `DeleteAvatarIfUnused`: canonical paths + reference check; `UsesImage` / `UsesAvatar` queries |
| Lockout (S2) | `NewsArchive/Auth/LoginThrottle.cs`, `Auth/PasswordChanger.cs`, `Controllers/AuthController.cs`, `Controllers/Api/AuthApiController.cs` | `Attempt(email, verify)` with slot reservation |
| Section (C1) | `Controllers/ArticleController.cs`, `DAL/Repos/ArticleRepo.cs`, `DAL/Repos/DbErrors.cs` (new), `BLL/ChangeResult.cs`, `Resources/SharedResource.bn.resx` | `RequireSection`; foreign-key error → `false`; EN/BN message |
| User delete (C2) | `DAL/Repos/UserRepo.cs` | One transaction; foreign-key conflict → rollback + `false` |
| Registration (C3) | `DAL/Repos/UserRepo.cs`, `DAL/Repos/DbErrors.cs` | Duplicate key → `false` |
| API version (C4) | `Controllers/Api/ArticlesApiController.cs`, `BLL/Rules/ArticleRules.cs`, `BLL/ChangeResult.cs`, `README.md` | 400 for a missing/malformed version; Swagger description; docs |
| Email (E1) | `NewsArchive/Email/DisabledEmailSender.cs` (new), `EmailSender.cs`, `EmailOptions.cs`, `DigestSender.cs`, `BLL/Email/IEmailSender.cs`, `Program.cs`, `Controllers/AdminController.cs`, `appsettings.json` | Disabled outside Development; `IsEnabled`; start-up warning; digest button refuses; SMTP block removed from settings |
| Dead code (D1) | `BLL/Interfaces/IArticleService.cs`, `ICommentService.cs`, `BLL/Services/ArticleService.cs`, `CommentService.cs` | Removed `IArticleService.Get()`, `ICommentService.Get()`, `ICommentService.Update()` |
| Maintenance (D2) | `NewsArchive/Publishing/MaintenanceJob.cs` (new), `BLL/BusinessLayerServices.cs`, `Program.cs` | Daily purge; shared `PurgeExpiredData` |
| Sitemap (D3, D7) | `Controllers/SeoController.cs`, `DAL/Repos/ArticleRepo.cs`, `DAL/Interfaces/IArticleRepository.cs`, `BLL/Interfaces/IArticleService.cs`, `BLL/Services/ArticleService.cs`, `BLL/Rules/SeoLimits.cs` (new) | `GetPublishedForSitemap(since, max)` in SQL; RSS/news cache 300 s |
| Auto-hide (D4) | `DAL/Repos/CommentRepo.cs`, `DAL/Interfaces/ICommentRepository.cs`, `BLL/Services/CommentService.cs` | `HideIfVisible` conditional update |
| Delete race (D5) | `DAL/Repos/Repository.cs` | Concurrency conflict on delete → `false` |
| Newsletter (D6) | `BLL/Services/NewsletterService.cs` | No language change without the confirmation link |
| Tests | `Integration/HardeningTests.cs` (new, 17), `Integration/AppFactory.cs` (`Scalar`), `AppTests.cs` (missing version → 400), `SecurityTests.cs` (+2), `CommentServiceTests.cs` (+1), `NewsletterServiceTests.cs` (+1) | 116 → 137 |
| Smoke | `tools/smoke/smoke.py` | Exact database allow-list + local server only; strips email env vars; offline email checks; version 400 check; sitemap timing |
| Docs | `README.md`, `ARCHITECTURE.md`, `sadp_delivery/*` | Email section, API version table, limits, ADR-007/008, C4 without SMTP |

## Rollback

- `git checkout main` (the branch is local and adds no migration).
- To undo part of it, revert `c1dfb82` or individual files. No data migration is involved.

## Regression risks

| Risk | Mitigation |
|---|---|
| API clients without `version` now get 400 | Documented with a curl example and an answer table; Swagger describes it |
| On a public host, newsletter confirmation and password reset by email do not work | Intended (ADR-007); documented in the README and the handoff |
| Picture paths typed by hand (non-canonical) are never cleaned up | They could be orphaned; harmless, deliberate |
| The lockout is per process | Documented; shared store planned (ADR-005) |
| `MaintenanceJob` runs on every instance if 2+ are started | The deletes are idempotent |
| `DbErrors` relies on SQL Server error numbers 547 / 2601 / 2627 | SQL Server is the only provider used (ADR-002) |
