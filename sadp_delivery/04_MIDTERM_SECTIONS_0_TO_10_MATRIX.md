# 04 · Midterm Sections 0–10 Matrix (P2, Lectures 1–3 only)

Checked section by section against the official guideline (P2 p1–5) on 2026-11-09.

| § | Owner | What P2 asks for | Evidence in code / tests | Draft location | Status | Remaining gap |
|---|---|---|---|---|---|---|
| Cover | Group | Topic, group no. & section, members (ID — Name), date, outcome | Topic and outcome filled | Draft cover table | PARTIAL | Group, section, members and date are [TBD] (personal data, never invented) |
| 0 | Group | Recap in two sentences; table member → feature → NFA → SOLID + pattern | 4 modules, each mapped to real code (`13`) | Draft §0 | PARTIAL | Names and IDs [TBD]; the group must confirm who owns which module |
| 1 | Group (Q1) | 2–3 architectural + 2–3 design decisions, with what makes each one so | A1–A3 and D1–D3 with file:line; rule cited [9, L1 s5–6], [1] | Draft §1 | READY | — |
| 2 | Group (Q2) | Style (layered / MVC / client–server), monolith vs SOA, why it fits the drivers | Compile-time layering; grep checks (`03`); drivers against [9, L2 s29–30] | Draft §2 | READY | — |
| 3 | Individual (Q3) | Each member: ≥ 1 attribute, a **measurable** target, the concrete tactic; name yourself | 4 member rows, each labelled VERIFIED / MEASURED / NOT VERIFIED; the other two attributes listed at group level | Draft §3 | READY (code side) | Each member must put their name on the row and be able to defend it |
| 4 | Group (Q4) | A C4 L1 or L2 diagram + two lines on why that view | **Rendered** `c4_level1_context.png` (Figure 1), checked by eye; reviewed against C4 rules (`09/C4_DIAGRAMS.md`) | Draft §4 | READY | — |
| 5 | Group (Q5) | Logical, process, development, physical + scenarios, one row each | All from the code; the process row was corrected (no dedicated thread per request [10]); 6 tested scenarios | Draft §5, `09/4PLUS1_VIEWS.md` | READY | — |
| 6 | Individual (Q6) | Each member: one SOLID principle on a feature they own, and why | SRP (builder/service/rules), DIP (`PasswordChanger`), OCP (`EmailSender`), **ISP now implemented**: `IArticleArchive` with a stub test | Draft §6 | READY | — |
| 7 | Group (Q7) | CP or AP for the most critical data during a partition, with the business reason | ACID vs CAP separated; CP for editorial state, AP for reads with the "withdrawn must vanish" limit, backed by a test | Draft §7, `08` | READY | It is a design decision; no replica exists (stated) |
| 8 | Group (Q8) | Migration path: first service, why, what each step buys | 5 phases; newsletter first | Draft §8 | READY | A plan, as P2 asks |
| 9 | Individual (Q9) | Each member: one creational pattern on their feature, justified | Builder and Factory Method implemented; Abstract Factory verified + test; DI-managed Singleton test + classic-vs-DI explanation (`07`) | Draft §9 | READY | Members rehearse the viva answers in `07` and `13` |
| 10 | Group | Sources in IEEE numeric style | 12 references, cited in the text as [n]; journal entries checked against Crossref, web pages opened | Draft §10 | READY | Check the 4 book editions against the library copy |

## Midterm boundary check (P2 p1: "Do not use later-week material")

- The draft answers Q1–Q9 with Lecture 1–3 concepts only: architecture vs design, the six attributes, CAP, 4+1, SOLID (L1); styles, C4, monolith vs SOA, migration (L2); creational patterns (L3).
- **Not used in the draft:** Week 4+ patterns, EDA, circuit breakers, sagas, observability, blockchain, IoT, AI agents.
- `DigestSender` appears only as an existing availability tactic (work moved off the request), never as EDA.
- The latency percentiles in §3 are numbers that make a target measurable; the draft does not cite the W9 lecture for them.
- The references to Microsoft documentation [8], [10], [11] support facts about our own code (concurrency tokens, thread pool, DI lifetimes). They are not new course concepts.

## Viva questions this revision answers better

| Earlier weakness | Now |
|---|---|
| M4's ISP rested on a "narrow repository" claim with a caveat | M4 owns a real narrow interface (`IArticleArchive`, 3 members) and a test that serves the archive pages from a 3-method stub. The Abstract Factory row is unchanged. |
| M2's Singleton could be challenged as "not a real Singleton" | `07` gives a side-by-side table (who guarantees one instance, global access, testability, DIP) and a one-paragraph viva answer, backed by W3 s11/s31 and [11]. |
| The process view said one thread per request | Corrected: pool threads, released at `await`; our synchronous EF calls block a pool thread for each query. |
| C4 diagram not rendered | Rendered PNG + SVG for L1 and L2; L1 is Figure 1. |
| No in-text citations | Every section cites [n]. |
