# 07 · Creational and Other Patterns: What Is Real

Rule used here: a pattern is claimed only when its GoF participants exist in the code, with a file:line, and a test exercises it.

- Using a framework feature does not count as implementing a GoF pattern.
- A class name does not count either.

## Creational patterns (midterm §9, W3)

| Pattern | Participants (GoF role → class, file:line) | Motivation | Tests | Drawbacks | Verdict |
|---|---|---|---|---|---|
| **Builder** | **Builder** `ArticleBuilder` (`BLL/Builders/ArticleBuilder.cs:5`), with steps `WrittenBy`, `WithText`, `InSection`, `InLanguage`, `WithImage`, `AsBreakingNews`, `SubmittedForReview`, `ApprovedBy`. **`Build()`** validates and returns the product (`:76`). **Product** `Article`. **Director** `ArticleService.Create` (`BLL/Services/ArticleService.cs:60`) picks the steps by role. | 12+ fields, optional image, breaking flag and publish time, plus status rules that depend on who creates the article (W3 s31 Q2 → Builder). Before, `Create` built the object and decided the status inline. | `NewsArchive.Tests/Patterns/ArticleBuilderTests.cs` (7) | One more class. `Update` still edits the tracked entity directly, so the builder covers creation only. | IMPLEMENTED IN THIS TASK |
| **Factory Method** | **Creator** `EmailSender` (`NewsArchive/Email/EmailSender.cs:11`) holds the business logic in `SendAsync` (`:22`). **Factory method** `CreateTransport()` (`:20`). **Concrete creators** `SmtpEmailSender` (`SmtpEmailSender.cs:20`), `OutboxEmailSender` (`OutboxEmailSender.cs:22`). **Product** `IMailTransport`. **Concrete products** `SmtpTransport`, `OutboxTransport`. Config picks the creator in `Program.cs:112–114`, as in W3 s18. | The two senders duplicated their logging and failure handling. A new channel should not touch existing code (OCP). SMTP needs a fresh client per message. | `Patterns/EmailSenderTests.cs` (5, including a contract test across two creators) | Four small types instead of two. A per-message transport object costs one allocation. | IMPLEMENTED IN THIS TASK (refactor of two existing senders) |
| **Abstract Factory** | **Abstract factory** `IDataAccessFactory` (`DAL/Interfaces/IDataAccessFactory.cs:5`). **Concrete factories** `DataAccessFactory` (`DAL/DataAccessFactory.cs`, SQL Server family) and the DispatchProxy `FakeFactory` (`NewsArchive.Tests/Support/TestServices.cs:52`, test family). **Abstract products** `IArticleRepository`, `ICategoryRepository`, `IReadingHistoryRepository`, … (11). **Concrete products** `ArticleRepo`, … **Client** every service. | Repositories used together must share one unit of work (one `DbContext`, one identity map, one transaction). Services must not know the concrete family (test vs SQL). | `AppTests.Repositories_from_one_factory_share_one_unit_of_work` (new); every `Services/*Tests` uses the fake family | Only one production family. Every service can reach all 11 repositories (ISP smell). | ALREADY PRESENT, VERIFIED IN THIS TASK. The name alone was not taken as proof: the participants and the family guarantee were checked and tested. |
| **Singleton** | **DI-managed single instance**: `LoginThrottle` (`Program.cs:52`), `PasswordResetStore` (`Program.cs:118`). The container lifetime replaces the classic private constructor + static instance + `GetInstance()` (W3 s8). | Lockout counters and reset codes must be shared by every request; two instances would silently weaken security. | `AppTests.Lockout_and_reset_stores_are_one_instance_for_the_whole_app` (`Assert.Same` across scopes) | Process-wide state is lost on restart and is not shared across servers. | VERIFIED as a "DI-scoped singleton" (W3 s11, s31). **Not** a classic GoF Singleton: no class here has a private constructor. Classic Singleton was rejected on purpose (global state, hard to test). `SiteClock` is a static utility, not a Singleton. |

**Not claimed:**
- `DataAccessFactory` by its name alone.
- `AddSingleton` registrations in general.
- `new` calls.
- `MailComposer`: a plain composer of messages, not a Builder.

## Later-week patterns (W4–W6): status

| Pattern | Status | Evidence / reason |
|---|---|---|
| Adapter (W4) | PARTIAL, adapter-like | `SmtpTransport` adapts `System.Net.Mail.SmtpClient` to `IMailTransport`; `WebRootFileStorage` adapts the file system to the BLL `IFileStorage`. Not built to show the pattern; documented only. |
| Facade (W4) | DOCUMENTED / PROPOSED | No class orchestrates a multi-step subsystem for several callers. `ArticleController.Details` (55 lines) is the candidate if a job or the API needs the same page data. |
| Proxy (W4) | NOT IMPLEMENTED | Caching is cache-aside inside the services (`IAppCache.GetOrCreate`), not a proxy with the same interface as the real subject. |
| Decorator (W4) | NOT CLAIMED | ASP.NET Core middleware is decorator-like (W4 s22), but that is the framework, not project code. |
| Composite (W4) | NOT APPLICABLE | No meaningful part–whole tree. Sections are flat, and tags are flat. |
| Strategy (W5) | PROPOSED | Recommendation scoring is one formula (`RecommendationRules`). It becomes a Strategy only when a second ranking (for example "most read") must be chosen per request. |
| Observer (W5) | NOT IMPLEMENTED | Events such as publish → cache invalidation and audit are direct calls inside `ArticleService`. Observer is justified if more reactions are added. |
| Command (W5) | NOT IMPLEMENTED | The audit log records actions but they are not objects with `undo`. Undo is not a requirement. |
| Template Method (W5) | NOT CLAIMED | `EmailSender.SendAsync` is a fixed skeleton, but its varying step *creates a product*, so the honest name is Factory Method. |
| State (W5) | PROPOSED | The editorial lifecycle is `int StatusId` + guards (W5 s23 "state-flag conditional"). W5 s28 names Draft→Review→Published as the textbook State case. Worth doing only if more states arrive (for example "Embargoed"); see `15`. |
| API Gateway, EDA, Circuit Breaker, Saga (W6) | NOT APPLICABLE today | Single deployable, so there is no inter-service call to route, break or compensate. An in-process `Channel` queue is **not** a broker-based EDA, and a single SQL transaction is **not** a Saga. |
