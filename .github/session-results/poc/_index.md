# Story Index: poc

## Key Decisions

| # | Decision |
|---|---|
| 1 | Full physical rename: `JOSYN.Core.*` → `JOSYN.Foundation.*`, `JOSYN.Core.IPC` → `JOSYN.Foundation.JIP` |
| 2 | Implementation order follows logical layering: Foundation → Contract → Frontend → Backend |
| 3 | All Foundation + Contract NuGet packages must reach high maturity (Readme, meta, XML comments) |
| 4 | JOSYN.Contract naming is unsatisfactory and must be resolved before starting that layer |
| 5 | `JOSYN.System` is the accepted grouping layer with three sub-layers: `JOSYN.System.Frontend`, `JOSYN.System.Backend`, `JOSYN.System.Shared` |
| 6 | `JOSYN.System.Shared.Contract` depends only on `JOSYN.Foundation.ResultPattern` — not on JIP (contract is transport-agnostic) |
| 7 | Layer packages follow the Foundation placement pattern: grouped under their layer folder (`JOSYN.System/`) |
| 8 | Grouping layers (`JOSYN.System.Frontend`, `JOSYN.System.Backend`, `JOSYN.System.Shared`) are pure namespace containers — concrete projects live one level deeper with fully-qualified names |
| 9 | `JOSYN.System.Backend.JAPServer` is an exe, not a NuGet library — it has one consumer (itself) and is never packed |
| 10 | `JOSYN.System.Frontend.JobHost` is the library name; `JOSYN.System.Backend.JAPServer` is the exe; `JOSYN.System.Shared` hosts two NuGet packages: `.Contract` and `.Log` |
| 11 | Root `.local-build/` orchestrates crystal-clean build-all (6 sub-repos, 7 NuGet packages) and demo launchers (Release + Debug) |
| 12 | `JOSYN.System.Contract` is superseded by `JOSYN.System.Shared.Contract`; old folder still on disk, not referenced |
| 13 | Error routing in `Core.cs` (Frontend): pipe failure → `LocalLog` only; job failure → `LocalLog` + `PutError`; `PutError` failure → `LocalLog` fallback |
| 14 | `LocalLog` (Shared.Log) logs to `<ExeDir>\logs\<date>.log`; default root is `AppContext.BaseDirectory\logs` (impersonation-safe, no user profile dependency); `LogDirectory` is a settable `public static string`; `EnableConsoleOutput` flag mirrors to console; causer overloads write to `Path.Combine(LogDirectory, causer)` subfolder |
| 15 | `ErrorReport.Causer` (first parameter) flows from JobHost → JSON-serialized IPC → JAPServer.PutError deserialization → `LocalLog.Error(causer, ...)` subfolder; JSON serialization required (INI truncates multi-line `CallStack`/`ExceptionDetails`) |
| 16 | Three canonical csproj templates (NuGet Library / Exe / Test) documented in `copilot-instructions/C# Project Files/AGENT.md`; single PropertyGroup, tabs, no `PackageReleaseNotes`; `GenerateDocumentationFile` on NuGet libraries only; each NuGet project has its own `icon.png` copy; each exe project has its own `icon.ico` copy |
| 17 | Exe icon in Explorer requires both `<ApplicationIcon>icon.ico</ApplicationIcon>` AND `<Content Include="icon.ico" />`; `<None Include>` alone is insufficient |

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
| 0013 | session-0013-shared-layer-discussion-summary.md | Discussion: JOSYN.System.Shared layer introduced; FakeCore eliminated; LocalLog + PutError wired; Contract renamed to Shared.Contract |
| 0015 | session-0015-locallog-enhancements-summary.md | LocalLog: EnableConsoleOutput flag, entry-assembly ProcessName, sender subfolder overloads, pause cleanup in cmd files |
| 0016 | session-0016-csproj-standardization-summary.md | LocalLog finalized (causer, exe-adjacent path, JSON fix); all 13 csproj files standardized to 3 templates; copilot-instructions template doc created |
| 0017 | session-0017-exe-icon-embedding-summary.md | Exe icon embedding: `ApplicationIcon` + `<Content Include="icon.ico" />` required; applied to all 4 exe projects; Template 2 in AGENT.md updated |
| 0018 | session-0018-contracts-grooming-summary.md | Full codebase contract audit; 8 missing interfaces created across JIP, PropertyBag, Shared.Log, Shared.Contract |
| 0019 | session-0019-xmldoc-grooming-summary.md | PoC-wide XML comment grooming: 4 findings reviewed, 3 fixes applied, full build/test/pack/commit/push completed |
| 0020 | session-0020-access-modifier-tightening-summary.md | Full codebase access-modifier audit; 2 fixes: JAPServer implicit → explicit interface impl; ArgumentsComparer public → internal |
| 0021 | session-0021-unit-test-coverage-generation.md | Comprehensive unit test gap-fill: +48 tests (JIP new project), +14 (PropertyBag), +12 (ResultPattern), +3 (Frontend); 161 → 238 total |
