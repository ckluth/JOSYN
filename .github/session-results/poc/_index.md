# Story Index: poc

## Key Decisions

| # | Decision |
|---|---|
| 1 | Full physical rename: `JOSYN.Core.*` → `JOSYN.Foundation.*`, `JOSYN.Core.IPC` → `JOSYN.Foundation.JIP` |
| 2 | Implementation order follows logical layering: Foundation → Contract → Frontend → Backend |
| 3 | All Foundation + Contract NuGet packages must reach high maturity (Readme, meta, XML comments) |
| 4 | JOSYN.Contract naming is unsatisfactory and must be resolved before starting that layer |
| 5 | `JOSYN.System` is the accepted grouping layer: `JOSYN.System.Frontend`, `JOSYN.System.Backend`, `JOSYN.System.Contract` |
| 6 | `JOSYN.System.Contract` depends only on `JOSYN.Foundation.ResultPattern` — not on JIP (contract is transport-agnostic) |
| 7 | Layer packages follow the Foundation placement pattern: grouped under their layer folder (`JOSYN.System/`) |

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
| 0004 | session-0004-foundation-propertybag-rename.md | Physical rename `JOSYN.Core.PropertyBag` → `JOSYN.Foundation.PropertyBag`; 47 tests green; NuGet packed |
| 0005 | session-0005-ariadne-thread-summary.md | AI continuity document: full big-picture, repo map, status per layer, immediate next step |
| 0006 | session-0006-jip-maturity-and-system-contract.md | JIP XML docs + README finished; `JOSYN.JAP.Protocol` → `JOSYN.System.Contract`; both packed |
| 0007 | session-0007-milestone-ariadne-v2.md | Milestone checkpoint: human report + Ariadne's Thread v2 (supersedes session-0005) |
| 0008 | session-0008-reference-fixes-sealing-summary.md | Side-session: fixed stale `JOSYN.JAP.Protocol` refs; sealed 16 types across all packages |
