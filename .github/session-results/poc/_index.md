# Story Index: poc

## Key Decisions

| # | Decision |
|---|---|
| 1 | Full physical rename: `JOSYN.Core.*` → `JOSYN.Foundation.*`, `JOSYN.Core.IPC` → `JOSYN.Foundation.JIP` |
| 2 | Implementation order follows logical layering: Foundation → Contract → Frontend → Backend |
| 3 | All Foundation + Contract NuGet packages must reach high maturity (Readme, meta, XML comments) |
| 4 | JOSYN.Contract naming is unsatisfactory and must be resolved before starting that layer |
| 5 | `JOSYN.System` is the accepted grouping layer: `JOSYN.System.Frontend`, `JOSYN.System.Backend`, `JOSYN.System.Contract` |

## Open Questions

*None currently open.*

## Clarified Points

| # | Clarification |
|---|---|
| 1 | "Logically archive" = PoC must be structured so the next milestone *could* move to a new repo without information loss. No concrete archiving action required in this PoC. |

## Sessions

| # | File | Summary |
|---|---|---|
| 0001 | session-0001-story-definition-summary.md | Story definition: goals, four-aspect structure, rename scope, implementation order |
| 0002 | session-0002-system-namespace-decision-summary.md | Resolved naming: `JOSYN.System` grouping layer accepted; Open Question #1 closed |
| 0003 | session-0003-foundation-resultpattern-rename.md | Physical rename `JOSYN.Core.ResultPattern` → `JOSYN.Foundation.ResultPattern`; 113 tests green; NuGet packed |
