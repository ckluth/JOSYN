# Story Index: poc

## Key Decisions

| # | Decision |
|---|---|
| 1 | Full physical rename: `JOSYN.Core.*` → `JOSYN.Foundation.*`, `JOSYN.Core.IPC` → `JOSYN.Foundation.JIP` |
| 2 | Implementation order follows logical layering: Foundation → Contract → Frontend → Backend |
| 3 | All Foundation + Contract NuGet packages must reach high maturity (Readme, meta, XML comments) |
| 4 | JOSYN.Contract naming is unsatisfactory and must be resolved before starting that layer |

## Open Questions

| # | Question |
|---|---|
| 1 | What is the new name/namespace/artifact for `JOSYN.Contract` (currently `JOSYN.JAP`)? — Deliberately open; must be resolved before starting that layer. |

## Clarified Points

| # | Clarification |
|---|---|
| 1 | "Logically archive" = PoC must be structured so the next milestone *could* move to a new repo without information loss. No concrete archiving action required in this PoC. |

## Sessions

| # | File | Summary |
|---|---|---|
| 0001 | session-0001-story-definition-summary.md | Story definition: goals, four-aspect structure, rename scope, implementation order |
