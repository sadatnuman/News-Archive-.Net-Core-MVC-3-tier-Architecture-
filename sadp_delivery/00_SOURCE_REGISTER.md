# 00 · Source Register

Course: AIUB CSC 4273 Software Architecture & Design Patterns, Summer 25-26, lecturer MD. Al-Amin (from each deck's title slide).

## Where the sources came from

The sources were read from `C:\Users\User\Downloads\SADP`, outside the repository: 2 PDFs and 12 decks (Weeks 1–6 and 8–13).

- The Week 2 deck was missing at the start. The user added it during the task, and it was then read in full.
- Nothing from the sources is committed, except short quotations with page or slide anchors.

## Priority order used

1. The two official PDFs.
2. The Week 1–3 decks, for the midterm.
3. The later decks, for full-course work.
4. The source code.
5. General knowledge.

## How the sources were read

- **PDFs:** both were read page by page, including the tables and the blank report templates. The page images were inspected.
- **Decks:** each `.pptx` was opened with the Python standard library (zip + XML), because `python-pptx` is not installed and nothing was installed.
  - The extraction covered slide text, text inside shapes, tables and speaker notes.
  - The script is `pptx_text.py`. It stays in the session scratchpad, not in the repository.
- **Diagrams:** every course diagram is drawn with shapes, so its boxes, labels and arrows were captured as text.
- **Images:** every embedded image was listed.
  - Each deck has exactly one image, the AIUB logo.
  - Week 12 repeats the same logo 17 times (31,146 bytes each). One copy was opened and confirmed.
  - No diagram exists only as a picture.
- **Speaker notes:** none of the decks has speaker notes with content.

## Register

| # | File | Pages / slides | Read | SHA-256 (first 16) | Key topics (anchor) |
|---|---|---|---|---|---|
| P1 | CSC4273_Semester_Project_Proposal_Template.pdf | 6 pages | Yes, all 6 | 620ecbf06ef45b9b | p1: milestones (Proposal → W9 ADR draft → W12 checkpoint → W14 final) and §1–6; p2: §7–12, ★ on §4 and ★★ on §5, six quality attributes with SmartLib examples; p3: submission list (≥ 6 FRs with ID, actor and priority; all six attributes measurable; style and the monolith-vs-SOA call; one diagram; ≥ 1 creational pattern; team roles and milestones; PDF or DOCX, 5–8 pages, via Teams, one per group, every member named); 8-criterion rubric scored 0–3, mark = total ÷ 24 × 100, CO3 + CO4; p4–6: blank template |
| P2 | CSC4273_MidTerm_CaseStudy_Guideline.pdf | 5 pages | Yes, all 5 | cb6cf0efd18f5597 | p1: Lectures 1–3 only, group + individual, banned later topics, table for §0–3; p2: table for §4–10, "the group owns the system (§1, 2, 4, 5, 7, 8); each member … does §3, 6 and 9", work-split table; p3–5: report template, viva Q1–Q9, CO1 (with CO3). No page limit is stated. |
| W1 | Week_01_SADP_Architecture_Foundations_details.pptx | 45 | Yes | cd0b5edec0882267 | s5–7 architecture vs design (worked table s6); s8–9 4+1 views; s10–26 SOLID (SRP s12–14, OCP s15–16, LSP s17–18, ISP s19–20, DIP s21 and s24–25); s27–39 six quality attributes with UniRide targets; s40–43 CAP (precise table s41, real systems s42, polyglot s43) |
| W2 | Week_02_SADP_Foundational_Styles_details.pptx | 33 | Yes | c216c682f493e4e2 | s5 what a style is; s6 the four N-tier layers; s7 golden rule (dependencies only downward, never skipping a layer); s8 three violations; s9–10 layered code; s11–12 MVC; s13–14 MVC vs MVP vs MVVM; s15–19 fat controller and the composition root (s17); s21–25 C4 levels and notation (each diagram states level, title and key; arrows say what AND how); s26–28 C4 L1 exercise; s29 monolith vs SOA (monolith "best for 0–12 months · 1–3 devs · <10k users"); s30 modular-monolith decision; s31 phased migration (extract the CPU-hot module first) |
| W3 | Week_03_SADP_Creational_Patterns_details.pptx | 34 | Yes | b22991eeb14d1e99 | s5 the problem with `new`; s7 SOLID links; s8–12 Singleton (three parts s8, debate and DI-scoped singleton s11); s13–18 Factory Method (four participants s14, creator holds the business logic s16); s20–24 Abstract Factory (family consistency); s25–29 Builder (validation in build(), optional Director s26); s30 comparison; s31 decision flowchart; s32 Assignment 1 |
| W4 | Week_04_SADP_Structural_Patterns_details.pptx | 31 | Yes | 8f9468e551936f09 | Adapter s8–11, Facade s12–16 (vs fat controller s16), Proxy s18–21, Decorator s22–24, Composite s25–27, comparison and drill s28–29 |
| W5 | Week_05_SADP_Behavioral_Patterns_details.pptx | 30 | Yes | 1cced0e0f1708c25 | Strategy s7–10, Observer s11–14, Command s16–18, Template Method s19–21, State s22–25, Strategy vs State s27, drill s28 ("Draft→Review→Published → State") |
| W6 | Week_06_SADP_Distributed_CloudNative_details.pptx | 24 | Yes | 4d6343424ea975d4 | monolith pain points s5, trade-off s6, rule of thumb s7, principles s8, decomposition s9–10, API Gateway s11–12, EDA s14–15, Circuit Breaker s16–17, Saga s18–19, Netflix s20–21; s24: "Week 7 — Mid-Term Examination, covers Weeks 1–6" |
| W7 | Week 7 | — | Not supplied | — | W6 s24 says Week 7 is the mid-term examination. No Week 7 requirement is invented. |
| W8 | Week_08_SADP_NFAs_Scalability_details.pptx | 25 | Yes | bc06c87fe3457880 | vertical vs horizontal s6–8; CAP CA/CP/AP s9–10; sharding and consistent hashing s11–13; resilience s15–17; load balancing s18–20; ATAM s21–23 |
| W9 | Week_09_SADP_Performance_Security_ADRs_details.pptx | 25 | Yes | e5e12880ae9bfb30 | latency, throughput, capacity, p99 s5–6; caching strategies s7–10; Zero Trust s11–14; JWT / OAuth2 / API key s12; observability s16–19; ADR format s20–23 |
| W10 | Week_10_SADP_Decentralized_IoT_details.pptx | 22 | Yes | 8dfa7a6238f1675d | centralized vs decentralized s5–6; blockchain s7–10; fit / no fit s11 (a signed append-only audit log is usually enough); Device–Edge–Cloud s13–16; MQTT and QoS s17–20 |
| W11 | Week_11_SADP_Agentic_AI_details.pptx | 23 | Yes | 2f8196fe19db6480 | Perceive–Reason–Act s6–7; ReAct vs Plan-and-Execute s8; RAG s9–11; tool agent s13–14; multi-agent s15–17; memory s18; guardrails s19–21 |
| W12 | Week_12_SADP_Project_Documentation_details.pptx | 19 | Yes | 5c8f65b47069745c | research proposal s5–6; ADR clinic s7–8 (valid statuses); C4 levels and checks s10–13 and s18; C4 as code s14; golden thread s15 |
| W13 | Week_13_SADP_Architecture_Review_details.pptx | 19 | Yes | f7559e96e5e4a4de | serverless s5–8; blue-green vs canary s10–14; course map s15–16; presentation do's and don'ts s17 |

## Discrepancies logged

| # | Where | What | How it was handled |
|---|---|---|---|
| D1 | W1, W2, W3 code | The examples are TypeScript and Node. This project is C# and ASP.NET Core. | The principles are applied in C#. |
| D2 | P2 p1 vs the master prompt | The prompt says the midterm uses "Weeks 1–3"; P2 says "Lectures 1–3". | Treated as the same thing: decks W1, W2 and W3. |
| D3 | P1 p1 vs P1 p6 | p1 lists the Week 9/12/14 milestones. The p6 template wants one line per milestone, with owners. | Both are covered in the proposal outline; owners stay TBD. |
| D4 | W8 s9 vs W1 s42 | W8 lists "CA: single-node RDBMS", while W1 lists PostgreSQL as CP. | The CAP answer (§7) says today's single SQL Server node has no partition to tolerate. It then sets CP for review decisions and AP for reading as the rule for a later replicated deployment. |
| D5 | P2 | No page count is given for the case study. | None is claimed. The 5–8 pages apply only to the proposal (P1 p3). |
