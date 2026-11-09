# 14 · Research and Later Milestones (not midterm)

Milestones from P1 p1. Owners stay TBD until the group fills them in.

| Milestone | Target (what will be ready) | Already available | Still to do | Owner(s) |
|---|---|---|---|---|
| Week 9: ADR draft | ADRs for the key decisions | `10_ADRS/README.md`: ADR-001…006, four Accepted and one Proposed (ADR-005) | Review against the W12 s7 checklist; one owner per ADR | TBD |
| Week 12: checkpoint | Updated architecture description + 3 diagrams (C1–C3) + ADR status | C1, C2, C3 as Mermaid (`09`); 4+1 (`09`) | Render C3 (C1 and C2 are already rendered); apply the justified W4–W5 items (Facade for article page data, State if new states arrive); measure again with 10k+ stories | TBD |
| Week 14: final | Report, demo or presentation, research proposal | Runnable app, CI, 137 tests, `tools/smoke/smoke.py` (45 checks + 5,000-story benchmark) | Final report; 10-minute talk (W13 s17: problem first, C4 L2, one ADR, numbers, every member speaks) | TBD |

## Golden-thread traceability (W12 s15)

| Quality attribute | Decision | Pattern / tactic | ADR | C4 element | Evidence |
|---|---|---|---|---|---|
| Reliability | Optimistic concurrency on editorial status and edits | Concurrency tokens (`StatusId`, `UpdatedAt`), form version; atomic scheduled publish | ADR-003 | L3 "Data access", L2 Database | 4 AppTests; smoke API 409 + MVC conflict, scheduler 85 s |
| Security | Server-side roles, lockout, CSRF | Filters, `LoginThrottle` (DI singleton) | ADR-001 (layering), ADR-005 (proposed shared store) | L3 "Auth and filters" | SecurityTests, AppTests |
| Availability | Contain mail failures; background digest | Factory Method `EmailSender` | ADR-006 | L3 "Email delivery", "Background jobs" | EmailSenderTests |
| Performance | Own word index; card projections; caches | Cache-aside with TTL and version keys | ADR-004 | L2 Database, L3 "Data access" | 5,024-story benchmark (`08`, `11`) |
| Maintainability | Compile-time layering; builder | Builder, Abstract Factory | ADR-001 | L3 all | 137 tests; CI on push and pull request |
| Scalability | Shared state store before scale-out | — | ADR-005 (Proposed) | L2 | Not measured |

## Research proposal seed (W12 s5–6)

- **Title:** "Word-prefix indexing versus SQL Server LIKE scans for bilingual (Bangla–English) news search on a single-node relational database."
- **Question:** For a news archive of 1k–100k stories, how much does a stored word-prefix index reduce p95 search latency compared with `LIKE '%term%'`, and what does it cost in storage and freshness for Bangla text?
- **Method:** design science (Wieringa) with controlled benchmarks. Generate 1k, 10k and 100k stories, run fixed query sets, and measure p50, p95 and p99 plus index size and indexing lag.
- **Limitations:** one machine, synthetic text, no full-text engine on the test database.
- **Literature (to verify):**
  - Kleppmann (DDIA, ch. 3).
  - SQL Server documentation on index seeks for `LIKE`.
  - Work on Bengali tokenization.
