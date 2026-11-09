# Full-Course Applicability Matrix (Weeks 1–6, 8–13; not the midterm)

**Sources.** The 12 supplied decks in `C:\Users\User\Downloads\SADP`. There is no Week 7 deck: W6 s24 says Week 7 is the mid-term exam, and nothing is invented for it. Slide numbers are listed in `00_SOURCE_REGISTER.md`.

**Decision values:**

| Value | Meaning |
|---|---|
| IMPLEMENT(ED) | In the code, with a test |
| DOCUMENT(ED) | Analysis or a decision written down |
| PLAN(NED) | For a later milestone, with a trigger |
| N/A | Not applicable, with the reason given |

Nothing below was added to the code just to tick a lecture box.

| Concept | Lecture | Existing real implementation | Decision | Why | Evidence / test | NFA / ADR | Future trigger |
|---|---|---|---|---|---|---|---|
| Architecture vs design | W1 s5–7 | A1–A3 vs D1–D3 | DOCUMENTED | Midterm §1 | Draft §1 | Maintainability / ADR-001 | — |
| 4+1 views | W1 s8–9 | Views from the real code | DOCUMENTED | Midterm §5 | `09/4PLUS1_VIEWS.md`, S1–S6 tested | — | — |
| SRP, OCP, LSP, ISP, DIP | W1 s10–26 | Builder/service/rules; `EmailSender` subclasses; contract tests; `IArticleArchive`; constructor injection | IMPLEMENTED | Each has a real feature | ArticleBuilderTests, EmailSenderTests, `Archive_pages_depend…`, SecurityTests | Maintainability | Split `IArticleService` (26 members) when the editorial area grows (R-6) |
| Six quality attributes | W1 s27–39 | Targets + tactics | DOCUMENTED + partly MEASURED | Proposal §5 | `08` | all | — |
| CAP | W1 s40–43 | One SQL Server node | DOCUMENTED | ACID ≠ CAP; CP editorial / AP reads planned | `08`, `A_withdrawn_story…` | Reliability | Read replicas |
| Layered N-tier, MVC, client–server | W2 s6–19 | UI → BLL → DAL, compile-time | IMPLEMENTED | Core style | Build; grep checks | ADR-001 | — |
| MVP / MVVM | W2 s13–14 | — | N/A | Razor MVC plus a small JS layer; no rich client | — | — | A SPA front end |
| C4 L1–L3 (L4 code is the code itself) | W2 s21–28, W12 s10–14 | L1/L2 rendered, L3 Mermaid | DOCUMENTED | Midterm §4, Proposal §8 | `09/*.png` | — | — |
| Composition root, fat controllers | W2 s15–19 | `Program.cs`; `AdminController` has 12 dependencies (smell) | DOCUMENTED | Smell recorded, not refactored | `03` | Maintainability | Split per admin area (R-6) |
| Monolith vs SOA, extraction | W2 s29–31 | Modular monolith | DOCUMENTED | Midterm §2, §8 | Draft | ADR-001 | Newsletter extraction when mail volume grows |
| Singleton, Factory Method, Abstract Factory, Builder | W3 | DI-managed singletons, `EmailSender`, `IDataAccessFactory`, `ArticleBuilder` | IMPLEMENTED | Real GoF roles | `07`, tests | — | — |
| Adapter | W4 | `SmtpTransport` (dormant), `WebRootFileStorage` adapt platform APIs | DOCUMENTED (adapter-like) | Not built to show the pattern | — | — | — |
| Facade | W4 | — | PLANNED | Article page assembly in `ArticleController.Details` | — | Maintainability | A second caller (job or API) needs the same page data |
| Proxy / Decorator | W4 | Cache-aside inside services; middleware is the framework's | N/A (not claimed) | No real need | — | — | — |
| Composite | W4 | — | N/A | No part–whole tree (sections and tags are flat) | — | — | Nested sections |
| Strategy | W5 | One recommendation formula | PLANNED | Only one algorithm today | — | Performance | A second ranking chosen per request |
| Observer | W5 | Direct calls (cache clear, audit) | N/A now | Two reactions only | — | — | More reactions to publish |
| Command | W5 | Audit log, no undo | N/A | Undo is not a requirement | — | — | — |
| Template Method | W5 | `EmailSender.SendAsync` skeleton | DOCUMENTED (named Factory Method) | The varying step creates a product | — | — | — |
| State | W5 | `StatusId` + guards + `ChangeResult` | PLANNED | 5 states are manageable | — | Reliability | New states (for example "Embargoed") |
| API Gateway, EDA, Circuit Breaker, Saga | W6 | — | N/A | One deployable, no remote business calls (no email provider either). An in-process `Channel` is not broker EDA, and one SQL transaction is not a Saga. | — | — | Extracting a service |
| Vertical / horizontal scaling | W8 | One instance; in-memory state | DOCUMENTED | Scale-out blocked by in-memory state | `08` | ADR-005 (Proposed) | > ~1k concurrent readers |
| Sharding, consistent hashing | W8 | — | N/A | 5k–10k stories fit one database | Benchmark | — | Measured data growth |
| Timeout / retry / fallback / bulkhead | W8 | Fallback: disabled mail returns `false` without a log storm; background queue; no retry (a retried digest could double-send) | PARTIAL | — | HardeningTests | Availability / ADR-007 | Real mail provider |
| Load balancing | W8 | — | N/A until 2 instances | — | — | ADR-005 | Second instance |
| ATAM | W8 | Mini-ATAM | DOCUMENTED | Week 8 exercise | `08` | — | — |
| p50/p95/p99, throughput | W9 | Benchmark script | IMPLEMENTED (measurement) | 5,024 stories | `08`, `11`, `tools/smoke/smoke.py` | Performance | Re-run after async EF (R-3b) |
| Cache-aside, TTL, invalidation | W9 | `MemoryAppCache` with version keys | IMPLEMENTED | — | `MemoryAppCacheTests` | Performance / ADR-004 | Distributed cache with 2+ instances |
| Zero Trust, auth schemes | W9 | Per-endpoint auth; **opaque bearer tokens stored as SHA-256 hashes, not JWT; no OAuth2** | IMPLEMENTED (named accurately) | — | AppTests roles | Security | External identity provider |
| Observability | W9 | `LoggerMessage` logs, ProblemDetails trace id, masked emails | PARTIAL | No metrics or tracing pipeline | — | — | Production hosting |
| ADRs | W9, W12 s7 | ADR-001…008 | DOCUMENTED | Week 9 milestone | `10_ADRS/` | all | — |
| Blockchain, smart contracts | W10 | Append-only audit log | N/A | One trusted owner; an audit log is **not** a blockchain | — | — | Several distrusting publishers |
| IoT, edge, MQTT | W10 | — | N/A | No devices | — | — | — |
| Agentic AI, RAG, multi-agent | W11 | Deterministic recommendations (**not** AI) | N/A now / possible future | Cost, hallucination and PII risk outweigh the benefit now | — | — | "Ask the archive" Q&A with citations |
| Research proposal | W12 s5–6 | Outline | DOCUMENTED (draft) | Week 12 | `14` | Performance | — |
| Golden thread (QA → decision → tactic → ADR → test) | W12 s15 | Table | DOCUMENTED | — | `14` | all | — |
| Serverless / FaaS | W13 | — | N/A now | Only the digest is bursty, and email is disabled | — | — | Real mail volume |
| Blue-green / canary | W13 | — | PLANNED (not deployed) | No deployment exists | — | — | First production host |
| Final review presentation | W13 s17 | Plan | PLANNED | Week 14 | `14` | — | — |

**Later milestones (not claimed as done):**
- **Week 9:** ADR review against the W12 s7 checklist.
- **Week 12:**
  - render C3;
  - research proposal draft;
  - golden thread refresh.
- **Week 14:**
  - final report;
  - demo;
  - 10-minute presentation.
