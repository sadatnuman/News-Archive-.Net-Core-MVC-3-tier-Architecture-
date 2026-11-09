# 04 · Midterm Sections 0–10 Matrix (P2, Lectures 1–3 only)

| § | Owner (P2 p2) | Requirement | Evidence in code / tests | Draft location | Status | Remaining gap |
|---|---|---|---|---|---|---|
| 0 | Group | Recap + table member → feature → NFA → SOLID + pattern | Modules map to real code (`13`) | Draft §0 | PARTIAL | Names and IDs are TBD; the group must confirm the module owners |
| 1 | Group (Q1) | 2–3 architectural + 2–3 design decisions, with the reason for each | A1–A3, D1–D3 with file:line | Draft §1 | READY | — |
| 2 | Group (Q2) | Style (layered / MVC / client–server) + monolith vs SOA + justification | Compile-time layering; grep checks (`03`) | Draft §2 | READY | — |
| 3 | Individual (Q3) | ≥ 1 attribute each, a measurable target, a concrete tactic | 4 rows, each with a test or a measurement | Draft §3 | READY (code side) | Each member must name themselves and be able to defend it |
| 4 | Group (Q4) | C4 L1 or L2 + two lines on why | Mermaid L1 / L2 / L3 | `09/C4_DIAGRAMS.md`, Draft §4 | PARTIAL | The image must be rendered and exported (no renderer installed here) |
| 5 | Group (Q5) | Logical, process, development, physical + scenarios | All from the code; 5 tested scenarios | `09/4PLUS1_VIEWS.md`, Draft §5 | READY | — |
| 6 | Individual (Q6) | One SOLID principle per member in their own feature | SRP, DIP, OCP, ISP (+ an LSP contract test) | Draft §6 | READY | ISP caveat (factory exposes all repositories) must be stated in the viva |
| 7 | Group (Q7) | CP or AP for the most critical data during a partition, with the business reason | ACID vs CAP separated; CP editorial / AP reading | Draft §7, `08` | READY | It is a design decision; no replica exists (stated) |
| 8 | Group (Q8) | Migration path: first service and why | Phased plan; newsletter first | Draft §8 | READY | Plan only (as P2 requires) |
| 9 | Individual (Q9) | One creational pattern per member, justified | Builder and Factory Method implemented; Abstract Factory verified + test; DI singleton test | Draft §9, `07` | READY with one risk | M2's Singleton is a DI-managed single instance, not the classic private-constructor form (allowed by W3 s11 / s31; see `15` R-V1) |
| 10 | Group | IEEE references | 9 entries | Draft §10 | READY | Check each reference against the library copy |

**Midterm boundary check.**
- The draft uses only Lectures 1–3: W1 SOLID, attributes, CAP, 4+1; W2 styles, C4, monolith; W3 creational.
- No Week 4+ pattern, EDA, circuit breaker, saga, observability, blockchain, IoT or AI is used to answer Q1–Q9.
- `DigestSender` is cited only as an existing tactic under Availability; it is not presented as EDA.
