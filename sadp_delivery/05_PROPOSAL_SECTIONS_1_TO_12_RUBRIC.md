# 05 · Proposal Sections 1–12 and Rubric (P1)

**Format required (P1 p3):**
- PDF or DOCX, 5–8 pages.
- One group submission via Teams; every member named.
- No deadline is given in the sources, and none is invented.

## Section check and suggested content

| § | Requirement | Where the content already exists | Status |
|---|---|---|---|
| 1 Title & overview | What the system is, who it is for, its value | Midterm draft §0, README | READY |
| 2 Problem & motivation | The real problem and why it needs a deliberate architecture | Problem: small newsrooms publish bilingual news without review, history or search; publishing an unreviewed story is a legal and trust risk. Architecture is needed for review integrity, Bangla search, and growth. | DRAFT TEXT BELOW |
| 3 Scope, actors, stakeholders | In / out of scope; actors; stakeholders | In: FR-1…12. Out: payments, mobile apps, live video, AI, multi-tenant. Actors: Reader, Author, Admin, API client, Scheduler. Stakeholders: editors-in-chief, advertisers (future), subscribers, the course instructor, hosting operator. | READY |
| 4 FRs ★ | ≥ 6, each with ID, actor, priority, testable | 12 FRs in `02` | READY |
| 5 NFRs ★★ | All six, measurable | `08` table (numbers, tactics, verification) | READY |
| 6 Architecture vs design | Hard to reverse vs cheap | Midterm draft §1 | READY |
| 7 Style & justification | Layered / MVC / client–server; monolith vs SOA; what was rejected | Midterm draft §2 | READY |
| 8 One diagram | C4 L1/L2 or 4+1 logical | `09` (use L2 for the proposal, L1 for the midterm) | PARTIAL (needs rendering) |
| 9 Anticipated patterns | ≥ 1 creational, later ones noted | Builder, Factory Method, Abstract Factory, DI Singleton; later: Facade, State, Strategy (`07`) | READY |
| 10 One trade-off | One honest tension | CAP: editorial status CP vs reading AP; or freshness vs performance (2-minute cache) | READY |
| 11 Team & milestones | Roles + W9 / W12 / W14 | `14` | PARTIAL (names TBD) |
| 12 Assumptions, risks, references | IEEE | Assumptions: one host, < 10k users, SQL Server available. Risks: `08` R1–R4 with mitigations. References: draft §10. | READY |

**Draft text for §2.**
Small Bangladeshi newsrooms and student publications often publish straight from a shared editor: stories go live without review, older stories are hard to find, and Bangla search works poorly. A published mistake cannot be taken back once it is shared. The Meridian Newsfolio enforces review before publication, keeps every story searchable in both languages, and records who did what. That needs deliberate architecture: strict layering for testable rules, a consistent record of editorial decisions, and a search design that works without commercial search engines.

## Indicative self-rating against the rubric (P1 p3)

This is NOT a predicted instructor grade.

| Criterion | 0–3 | Basis |
|---|---|---|
| Problem, scope & stakeholders | 2 | Clear; stakeholders need the group's real context |
| Functional requirements | 3 | 12 testable FRs, each with actor and priority, most covered by tests |
| Non-functional requirements | 3 | All six with numbers; measured where possible, and the unmeasured ones labelled |
| Architectural style & justification | 3 | Style matches the code; monolith-vs-SOA reasoned with W2 s29 criteria |
| High-level view | 2 | Correct L1/L2 as code; still needs exporting as a figure |
| Design-pattern & SOLID awareness | 3 | Patterns exist in the code with tests; the DI-singleton nuance is explained |
| Trade-off analysis | 3 | CAP with ACID ≠ CAP stated |
| Team plan & feasibility | 1 | Names, roles and owners are TBD |
| **Total** | **20 / 24** | Becomes 22–23 once team details and the rendered figure are added |

## 5–8 page outline

| Page | Content |
|---|---|
| 1 | Cover table + §1–2 |
| 2 | §3 + §4 (FR table) |
| 3 | §5 NFR table |
| 4 | §6–7 |
| 5 | §8 diagram (L2) |
| 6 | §9–10 |
| 7 | §11–12 + references |
