# Work Log

**Revision 1:** branch `sadp/midterm-readiness`, created from `develop` at `c9f08a3`. The user later pushed it and merged it into `main` (`484deb3`, PR #9).

**Revision 2 (independent review corrections):** branch `sadp/review-corrections`, created from `main` at `484deb3`. Local only: not pushed, not merged.

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

## Revision 2 phases

| Phase | Status | Notes |
|---|---|---|
| 1. Branch from `main` | Done | `main@484deb3` has the same file tree as `origin/sadp/midterm-readiness` |
| 2. Re-read the midterm guideline (P2) | Done | Section by section; matrix in `04` |
| 3. Code: stale edits, 409 messages, Production email, ISP | Done | `ChangeResult`, version check + `UpdatedAt` token (empty migration), outbox refusal, `IArticleArchive` |
| 4. Tests | Done | 8 new, 2 changed; 116/116 pass |
| 5. Smoke + 5,000-story benchmark | Done | `tools/smoke/smoke.py`: 40/40; search p95 72.2 ms (1 user) and 672.5 ms (20 users) |
| 6. C4 review and rendering | Done | L1 and L2 as SVG + PNG, checked by eye |
| 7. Documents and handoff | Done | 04, 05, 07, 08, 09, 11, 12, 13, 15, draft, handoff |
| 8. Commit | Done | Local commit on `sadp/review-corrections` only |

## Continuation instruction

If anything must be redone:

1. Check out `sadp/review-corrections`.
2. Read `15_REMAINING_ACTIONS_P0_P1_P2.md` and run its follow-up prompt.
3. Never touch `NewsArchiveDB`. Use `NewsArchiveTestDB` and `NewsArchiveSmokeDB` only.
4. Do not push.
