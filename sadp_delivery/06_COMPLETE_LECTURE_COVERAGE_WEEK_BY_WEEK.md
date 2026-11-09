# 06 · Lecture Coverage, Week by Week (full course; separate from the midterm matrix)

## Status values

| Status | Meaning |
|---|---|
| VERIFIED | Already in the code, checked with evidence |
| IMPLEMENTED | Built in this task |
| DOCUMENTED | Written down as analysis or a proposal |
| PLANNED / PROPOSED | Intended for a later milestone, with the trigger that would justify it; not in the code |
| N/A | Not applicable, with the reason given |
| NOT VERIFIED | Could not be checked |

There is no Week 7 deck: W6 s24 says it is the mid-term examination.

| Week · slide | Concept | Status | Project mapping and evidence | Test |
|---|---|---|---|---|
| W1 s5–7 | Architecture vs design | DOCUMENTED | Midterm draft §1 | — |
| W1 s8–9 | 4+1 views | DOCUMENTED | `09_C4_AND_4PLUS1/4PLUS1_VIEWS.md` | Scenarios S1–S5 are tested |
| W1 s12–14 | SRP | VERIFIED + IMPLEMENTED | `ArticleBuilder` / `ArticleService` / `ArticleRules` | ArticleBuilderTests |
| W1 s15–16 | OCP | IMPLEMENTED | `EmailSender` factory method | EmailSenderTests |
| W1 s17–18 | LSP | VERIFIED by test | Every `IEmailSender` meets one contract; `FakeFactory` replaces `DataAccessFactory` | `Every_sender_honours_the_same_contract` |
| W1 s19–20 | ISP | IMPLEMENTED (one smell remains) | `IArticleArchive` (3 members) is the only contract the archive pages, archive API and sitemap use; narrow repositories; `IAppCache`. `IArticleService` still has 26 members for the editorial pages. | `AppTests.Archive_pages_depend_only_on_the_narrow_archive_contract` |
| W1 s21–25 | DIP | VERIFIED | Constructor injection; BLL-owned abstractions | SecurityTests (fake `IAuthService`) |
| W1 s27–39 | Six quality attributes | DOCUMENTED + MEASURED | `08` | Benchmark, tests |
| W1 s40–43 | CAP | DOCUMENTED | `08` (ACID vs CAP separated) | — |
| W2 s6–10 | N-tier layered, downward only | VERIFIED | `03` §1; compile-time `PrivateAssets` | Build |
| W2 s11–19 | MVC, fat controller, composition root | VERIFIED (two smells recorded) | `Program.cs`; `AdminController` has 12 dependencies | — |
| W2 s13–14 | MVP / MVVM | N/A (theory) | Razor MVC website plus a small JS layer; no reason to rewrite as MVVM | — |
| W2 s21–28 | C4 levels and notation | DOCUMENTED + RENDERED | `09_C4_AND_4PLUS1/C4_DIAGRAMS.md` (L1, L2, L3 as Mermaid); L1 and L2 rendered to SVG/PNG by `render_c4.py` and reviewed against the notation rules | Checked by eye |
| W2 s29–31 | Monolith vs SOA; migration phases | DOCUMENTED | Midterm draft §2 and §8 | — |
| W3 s8–12 | Singleton | VERIFIED as a DI-scoped singleton | `Program.cs:52`, `:118` | Lockout_and_reset_stores… |
| W3 s13–18 | Factory Method | IMPLEMENTED | `NewsArchive/Email/EmailSender.cs:20` | EmailSenderTests |
| W3 s20–24 | Abstract Factory | VERIFIED + test added | `IDataAccessFactory` | Repositories_from_one_factory… |
| W3 s25–29 | Builder | IMPLEMENTED | `BLL/Builders/ArticleBuilder.cs` | ArticleBuilderTests |
| W4 | Adapter / Facade / Proxy / Decorator / Composite | DOCUMENTED (see `07`) | Adapter-like transports; Facade proposed; Composite N/A | — |
| W5 | Strategy / Observer / Command / Template Method / State | DOCUMENTED (see `07`) | State and Strategy proposed with triggers | — |
| W6 s5–10 | Monolith pain points, decomposition | DOCUMENTED | Midterm §8 | — |
| W6 s11–19 | API Gateway, EDA, Circuit Breaker, Saga | N/A today | One deployable and no remote business dependency at all (real email is not implemented; ADR-007) | — |
| W8 s6–8 | Vertical vs horizontal scaling | DOCUMENTED | `08` Scalability: scale-out is blocked by in-memory state | — |
| W8 s11–13 | Sharding, consistent hashing | N/A | 28–10,000 stories fit one database; sharding needs a measured need | — |
| W8 s15–17 | Retry, timeout, bulkhead, fallback | PARTIAL | Fallback: mail is disabled outside Development and returns `false` without a log storm; background queue for digests; no retry (a retried digest could double-send); the dormant SMTP transport has a 5–120 s timeout | EmailSenderTests, HardeningTests |
| W8 s18–20 | Load balancing | N/A until 2 instances exist | Round robin would do for these short stateless requests, after ADR-005 | — |
| W8 s21–23 | ATAM | DOCUMENTED | `08` mini-ATAM | — |
| W9 s5–6 | p50 / p95 / p99 | MEASURED | Search benchmark with 5,024 stories (`08`, `11`); script `tools/smoke/smoke.py` | Re-runnable |
| W9 s7–10 | Cache-aside with TTL | VERIFIED | `IAppCache.GetOrCreate` with 30 min / 2 min TTLs and region invalidation (`NewsArchive/Caching/MemoryAppCache.cs:7`); no write-through or read-through | — |
| W9 s11–14 | Zero Trust, JWT / OAuth2 / API key | VERIFIED (accurately named) | Every request is authenticated per endpoint. **The API uses opaque 256-bit random tokens stored as SHA-256 hashes, not JWT.** No OAuth2 or OIDC. | AppTests roles |
| W9 s16–19 | Observability | PARTIAL | Compiled structured logging (`LoggerMessage`), masked emails, `ProblemDetails` with a trace id; no metrics or tracing pipeline | — |
| W9 s20–23 | ADRs | IMPLEMENTED (documents) | `10_ADRS/`: ADR-001…008 (ADR-007 offline email, ADR-008 lockout reservation; ADR-005 still Proposed) | — |
| W10 s5–11 | Decentralized / blockchain | N/A | The newsroom is the single trusted owner, with no distrusting parties sharing a ledger. The audit log is append-only (W10 s11: "a normal append-only audit log … gives most of the benefit"). Hash-chaining the audit log is a possible future step; it would still **not** be a blockchain. | — |
| W10 s13–20 | IoT, MQTT, edge | N/A | No devices or telemetry in a news archive | — |
| W11 | Agents, RAG, multi-agent | N/A now, future option | Need: a possible "ask the archive" Q&A. Benefit: grounded answers with citations. Cost: an LLM API budget plus a vector store. Risk: hallucinated news and PII. **Verdict: not built.** Recommendations stay deterministic and are **not** AI. | — |
| W12 s5–6 | Research proposal | DOCUMENTED (outline) | `14` | — |
| W12 s10–14 | C4 L1–L3 as code | IMPLEMENTED (Mermaid) | `09` | — |
| W12 s15 | Golden thread | DOCUMENTED | `14` traceability table | — |
| W13 s5–8 | Serverless | N/A now | Candidate only for the digest sender (bursty); the always-on web app must not move | — |
| W13 s10–14 | Blue-green / canary | DOCUMENTED, not deployed | A deployment-slot swap would give blue-green; a canary is not needed for a small site | — |
| W13 s17 | Final presentation | DOCUMENTED | `14` | — |
