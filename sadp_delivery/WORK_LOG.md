# Work Log

Local branch: `sadp/midterm-readiness`, created from `develop` at `c9f08a3`. Nothing is pushed or merged.

| Phase | Status | Notes |
|---|---|---|
| 1. Discover sources | Done | `C:\Users\User\Downloads\SADP`: 2 PDFs + 12 decks. Week 2 was added by the user mid-task. Week 7 = exam week (W6 s24). |
| 2. Read both PDFs | Done | Proposal template (6 pages) and midterm guideline (5 pages) |
| 3. Read all decks | Done | Text, shapes, tables and notes extracted with the standard library. Images: AIUB logo only. |
| 4. Baseline | Done | restore 0; build 0 warnings / 0 errors; 92/92 tests pass |
| 5. Repository audit | Done | Layers clean (UI never uses the DAL, the DAL never uses the BLL, the BLL never uses HTTP); defects F1–F4 (`02`) |
| 6. Code changes | Done | ArticleBuilder, EmailSender factory method, StatusId concurrency token (empty migration), language-code fix, 16 new tests, test-only client-IP filter |
| 7. Tests and runtime | Done | 108/108 pass; smoke 32/32 on `NewsArchiveSmokeDB`; benchmark measured |
| 8. Documents and handoff | Done | 00–15, diagrams, ADRs, midterm draft, handoff |
| 9. Commit | Done | Local commits on `sadp/midterm-readiness` only |

## Continuation instruction

If anything must be redone:

1. Check out `sadp/midterm-readiness`.
2. Read `15_REMAINING_ACTIONS_P0_P1_P2.md` and run its follow-up prompt.
3. Never touch `NewsArchiveDB`. Use `NewsArchiveTestDB` and `NewsArchiveSmokeDB` only.
4. Do not push.
