# 12 · Code Changelog and Risks

Branch `sadp/midterm-readiness`, created from `develop@c9f08a3`. Committed locally only; **not pushed, not merged.**

Size: 17 code and doc files changed, +1,655 / −85 lines. About 1,050 of the added lines are the EF-generated migration designer.

| # | Requirement → problem | Design choice | Files | Tests | Result | Trade-off / rollback |
|---|---|---|---|---|---|---|
| C1 | §9 Builder; article creation mixed field mapping with status rules | `ArticleBuilder` with fluent steps and a validating `Build()`; `ArticleService.Create` acts as the director | `BLL/Builders/ArticleBuilder.cs` (new), `BLL/Services/ArticleService.cs:60` | `Patterns/ArticleBuilderTests` (7) | Pass; behaviour unchanged (smoke: submit, approve, schedule all work) | One more class. Rollback: revert the two files. |
| C2 | §9 Factory Method + OCP; two email senders duplicated their logic | Abstract creator `EmailSender` + `CreateTransport()`; `SmtpTransport` and `OutboxTransport` products | `NewsArchive/Email/EmailSender.cs` (new), `SmtpEmailSender.cs`, `OutboxEmailSender.cs` | `Patterns/EmailSenderTests` (5) | Pass; the smoke run wrote 1 outbox file | Success is now also logged for SMTP; the outbox file name uses UTC. Rollback: revert the three files. |
| C3 | §3 Reliability; two reviewers could both "win" | `StatusId` as an EF concurrency token; `Repository.Update` returns `false` on conflict and clears the tracker | `DAL/EF/NewsArchiveDbContext.cs:91`, `DAL/Repos/Repository.cs:41–52`, migration `20261109095936_ArticleStatusConcurrencyToken` (empty Up/Down) | `AppTests.Two_reviewers…` | Pass; smoke second decision 409 | **No schema change.** Every update of an article now includes `AND StatusId=@original`. Rollback: remove the line and the migration (`dotnet ef migrations remove` before it is applied anywhere; it is a no-op either way). |
| C4 | Defect F1: language code was case-sensitive | `AppLanguages.Normalize` trims and ignores case | `BLL/AppRoles.cs:27` | ArticleBuilderTests | Pass | None |
| C5 | §9 Abstract Factory / Singleton evidence | Tests only | `Integration/AppTests.cs` | 2 new tests | Pass | — |
| C6 | Integration tests would hit the per-IP rate limit (all clients shared the IP "unknown") | Test-only `IStartupFilter` sets the client IP from an `X-Test-Ip` header; each test client gets a random 10.x address | `Integration/AppFactory.cs` | All integration tests | Pass | Test code only; production is unchanged |
| C7 | Documentation rule | ARCHITECTURE (builder, concurrency, factory method, migration list, tests), README (test count) | `ARCHITECTURE.md`, `README.md` | — | — | — |
| C8 | Deliverables | `sadp_delivery/` (00–15, diagrams, ADRs, draft, handoff) | new folder | — | — | Contains no lecturer files, only short anchored quotes |

## Safety record

- No push, merge, force or remote change.
- No deployment.
- No email sent: SMTP host empty, outbox only.
- No destructive migration (the only new one is empty).
- The user's `NewsArchiveDB` was not touched.
- The sample-data script ran only on the disposable `NewsArchiveSmokeDB`.
- No secret was added. The sample password appears only in the existing development settings and README (pre-existing; recorded in `15`).

## Residual risks introduced

- C3: the optimistic check returns `false` (409 / "could not save") in a real race. The UI shows a generic failure message rather than "someone else decided first" (`15` P2).
- C2: SMTP behaviour against a real server was not tested here.
