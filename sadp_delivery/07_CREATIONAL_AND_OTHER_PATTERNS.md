# 07 · Creational and Other Patterns: What Is Real

**Rule used here.** A pattern is claimed only when:
- its GoF participants exist in the code, with a file and line; and
- a test exercises it.

Using a framework feature does not count as implementing a GoF pattern. A class name does not count either. Paths are relative to `NewsArchiveSolution/` on branch `sadp/review-corrections`.

## Creational patterns (midterm §9, W3)

### Builder: implemented

| Item | Detail |
|---|---|
| Participants | **Builder** `ArticleBuilder` (`BLL/Builders/ArticleBuilder.cs:5`), with steps `WrittenBy`, `WithText`, `InSection`, `InLanguage`, `WithImage`, `AsBreakingNews`, `SubmittedForReview`, `ApprovedBy`. **`Build()`** validates and returns the product (`:76`). **Product** `Article`. **Director** `ArticleService.Create` (`BLL/Services/ArticleService.cs:60`), which picks the steps by role. |
| Motivation | 12+ fields, an optional image, breaking flag and publish time, plus status rules that depend on who creates the article (W3 s31, Q2 → Builder). |
| Tests | `NewsArchive.Tests/Patterns/ArticleBuilderTests.cs` (7) |
| Drawbacks | One more class. `Update` still edits the tracked entity directly, so the builder covers creation only. |

### Factory Method: implemented

| Item | Detail |
|---|---|
| Participants | **Creator** `EmailSender` (`NewsArchive/Email/EmailSender.cs:11`), whose business logic is in `SendAsync` (`:22`). **Factory method** `CreateTransport()` (`:20`). **Concrete creators** `SmtpEmailSender`, `OutboxEmailSender`. **Product** `IMailTransport`. **Concrete products** `SmtpTransport`, `OutboxTransport`. Configuration picks the creator in `Program.cs:111–114`. |
| Motivation | The two senders used to duplicate their logging and failure handling. A new channel should not touch existing code (OCP). |
| Change in this revision | Outside Development with no SMTP host, `OutboxTransport` now throws "No email transport is configured" (`OutboxEmailSender.cs:39`). `SendAsync` turns that into `false`, so the sender no longer claims a mail was sent when nothing left the server. |
| Tests | `Patterns/EmailSenderTests.cs` (5), including a contract test across two creators, and the Production-without-SMTP case |
| Drawbacks | Four small types instead of two. |

### Abstract Factory: present, verified

| Item | Detail |
|---|---|
| Participants | **Abstract factory** `IDataAccessFactory` (`DAL/Interfaces/IDataAccessFactory.cs:5`). **Concrete factories** `DataAccessFactory` (SQL Server family) and the DispatchProxy `FakeFactory` (`NewsArchive.Tests/Support/TestServices.cs:52`, test family). **Abstract products** `IArticleRepository`, `ICategoryRepository`, … (11). **Client** every service. |
| Motivation | Repositories used together must share one unit of work (one `DbContext`, one identity map). Services must not know which family they get. |
| Tests | `AppTests.Repositories_from_one_factory_share_one_unit_of_work`; every `Services/*Tests` uses the fake family. |
| Drawbacks | Only one production family. Every service can reach all 11 repositories (an ISP smell, `15` R-6). |
| Unchanged | The ISP fix in this revision (`IArticleArchive`) is in the BLL. It does not touch the factory, so the Abstract Factory keeps its role. |

### Singleton: DI-managed single instance, verified

| Item | Detail |
|---|---|
| Where | `LoginThrottle` (`Program.cs:52`) and `PasswordResetStore` (`Program.cs:118`) are registered with `AddSingleton`. |
| Motivation | Lockout counters and reset codes must be shared by every request. Two copies would silently weaken security. |
| Test | `AppTests.Lockout_and_reset_stores_are_one_instance_for_the_whole_app` (`Assert.Same` across scopes) |
| Drawbacks | The state lives in the process. It is lost on restart and not shared across servers (`15` R-1). |
| Verdict | A "DI-scoped singleton", as W3 s11 and s31 call it. **Not** the classic GoF form: no class here has a private constructor. |

**Viva explanation: DI-managed versus classic Singleton.**

| Question | Classic GoF Singleton [2], W3 s8 | DI-managed single instance (our code) [11] |
|---|---|---|
| Who guarantees one instance? | The class itself: a private constructor and a private static field | The DI container: `AddSingleton` creates one instance per app and returns it every time |
| How do callers get it? | A global access point, `Instance` or `GetInstance()` | Constructor injection, the same as any other dependency |
| Can a test replace it? | Hard: the static access point is hard-wired | Easy: register a different instance |
| Coupling | Every caller depends on the concrete class (it violates DIP) | Callers depend on what is injected; the lifetime is chosen in one place (`Program.cs`) |
| Thread safety of creation | Must be hand-written (lock or `Lazy<T>`) | Handled by the container |
| Weakness | Global state | Nothing stops someone writing `new LoginThrottle()` by hand; the guarantee holds only for instances taken from the container. In this code base the class is never created with `new` outside the container. |

What to say in the viva: "The intent of Singleton is one instance with a well-known way to reach it. We kept the intent (one instance, proved by `Assert.Same`) but moved the guarantee from the class to the container. That removes the global access point the lecture warns about (W3 s11), so the class stays testable and DIP holds."

**Not claimed as patterns:**
- `DataAccessFactory` by its name alone.
- `AddSingleton` registrations in general.
- `new` calls.
- `MailComposer`: it composes messages; it is not a Builder.

## Later-week patterns (W4–W6): status

These are **outside the midterm** (P2 forbids later-week material) and are listed only for the full-course record.

| Pattern | Status | Evidence / reason |
|---|---|---|
| Adapter (W4) | PARTIAL, adapter-like | `SmtpTransport` adapts `System.Net.Mail.SmtpClient` to `IMailTransport`; `WebRootFileStorage` adapts the file system to the BLL `IFileStorage`. Documented only. |
| Facade (W4) | PROPOSED | No class runs a multi-step subsystem for several callers. `ArticleController.Details` is the candidate if a job or the API needs the same page data. |
| Proxy (W4) | NOT IMPLEMENTED | Caching is cache-aside inside the services, not a proxy with the same interface as the real subject. |
| Decorator (W4) | NOT CLAIMED | ASP.NET Core middleware is decorator-like, but that is the framework, not project code. |
| Composite (W4) | NOT APPLICABLE | No meaningful part–whole tree: sections and tags are flat. |
| Strategy (W5) | PROPOSED | Recommendation scoring is one formula. It becomes a Strategy only when a second ranking must be chosen per request. |
| Observer (W5) | NOT IMPLEMENTED | Publish → cache clear and audit are direct calls inside `ArticleService`. |
| Command (W5) | NOT IMPLEMENTED | The audit log records actions, but they are not objects with `undo`, and undo is not a requirement. |
| Template Method (W5) | NOT CLAIMED | `EmailSender.SendAsync` is a fixed skeleton, but its varying step *creates a product*, so the honest name is Factory Method. |
| State (W5) | PROPOSED | The editorial lifecycle is `int StatusId` + guards, now returning `ChangeResult`. Worth a State class only if more states arrive (for example "Embargoed"). |
| API Gateway, EDA, Circuit Breaker, Saga (W6) | NOT APPLICABLE today | Single deployable: no call between services to route, break or compensate. An in-process `Channel` queue is **not** a broker-based EDA, and one SQL transaction is **not** a Saga. |
