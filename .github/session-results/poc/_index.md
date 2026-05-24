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
| 8 | Grouping layers (`JOSYN.System.Frontend`, `JOSYN.System.Backend`) are pure namespace containers — concrete projects live one level deeper with fully-qualified names |
| 9 | `JOSYN.System.Backend.JAPServer` is an exe, not a NuGet library — it has one consumer (itself) and is never packed |
| 10 | `JOSYN.System.Frontend.JobHost` (renamed from `JOSYN.System.Frontend`) is the library name; similarly Backend's concrete project is `JOSYN.System.Backend.JAPServer` |
| 11 | Root `.local-build/` orchestrates crystal-clean build-all (empty Local Packages → clear NuGet cache → build+pack in order) and demo launchers (Release + Debug) |

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
| 0009 | session-0009-frontend-rename-summary.md | Physical rename `JOSYN.JobHost` → `JOSYN.System.Frontend`; scaffold, namespace, XML docs, README, NuGet packed |
| 0010 | session-0010-milestone-ariadne-v3.md | Milestone checkpoint + Ariadne's Thread v3 (supersedes session-0007); commit & push |
| 0011 | session-0011-backend-generation.md | `JOSYN.System.JAPServer` → `JOSYN.System.Backend`; scaffold, namespace, README, NuGet packed; `JOSYN.JobHost` tombstone removed |
| 0012 | session-0012-milestone-ariadne-v4.md | Milestone checkpoint + Ariadne's Thread v4 (supersedes session-0010); all 6 sub-repos complete, demo runnable, commit & push |
