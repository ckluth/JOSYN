# Session 0028 — LocalLog Unit Tests

## What Was Done

Added a complete unit test suite for `JOSYN.System.Shared.Log` — a component that had been overlooked during the general test-coverage session (0021).

---

## Discussion: Testing Code with File System Side Effects

Before implementing, a general discussion was held on how to approach tests for code that writes to the file system. Key conclusions:

- The real test target is never `File.AppendAllText` — that is the runtime's responsibility. The contracts to test are format correctness, routing logic, and silent failure behaviour.
- Three options were evaluated: abstract I/O away (IFileWriter), accept impurity with a controlled temp dir, or extract and test only the pure parts. The recommendation depends on whether the abstraction has independent design value — for a static infrastructure helper it typically does not.
- `LocalLog.LogDirectory` is a settable static property and acts as the seam. No refactoring required to redirect output.
- `Path.GetTempPath()` with a per-test subdirectory (using `TestContext.CurrentContext.Test.ID`) is the correct choice for test-scoped file output.
- `[NonParallelizable]` is needed because `LogDirectory` and `EnableConsoleOutput` are shared static state.
- The general discussion was also saved as a standalone reference artifact: `.github\.artifacts\testing-with-filesystem-sideeffects.md`

---

## Changes

### Production

| File | Change |
|---|---|
| `JOSYN.System.Shared.Log/LocalLog.cs` | `FormatEntry` promoted from `private` to `internal` |
| `JOSYN.System.Shared.Log/InternalsVisibleTo.cs` | New file — exposes internals to the test assembly |

### Test project (new)

`JOSYN.System.Shared.Log.Test/` added and registered in `JOSYN.System.Shared.slnx`.

**`LocalLogTests.cs`** — 22 I/O-based tests (fixture is `[NonParallelizable]`):

| Group | Tests |
|---|---|
| Format (via file content) | `[ERROR]`/`[INFO]` header, message present, 80-dash separator, callStack section present/absent (null and empty), exception section present/absent |
| Routing | No causer → root dir; causer → subdirectory; Info same; two causers → separate subdirs |
| Result overloads | ErrorMessage extracted; CallStack extracted; Exception extracted; causer+Result routes correctly |
| Resilience | `Error` and `Info` with invalid directory — both `DoesNotThrow` |
| Console output | `EnableConsoleOutput = true` writes to Console; `false` does not |

**`LocalLogFormatEntryTests.cs`** — 16 pure tests (no I/O, no fixture state):

| Group | Tests |
|---|---|
| Header | Timestamp regex match; `[TestCase]` for INFO/ERROR/WARN level tags |
| Message | Message content appears in output |
| CallStack | Header present with content; absent with null; absent with empty string |
| Exception | Header present with content; absent with null; absent with empty string |
| Separator | Exactly 80 dashes (also asserts not 81) |
| Section order | Index comparison: message < callstack < exception < separator |

**Test result: 38/38 passing.**

---

## Ambiguity Note

`LocalLog.Error(string, string)` is ambiguous between `Error(message, callStack)` and `Error(causer, message)`. All causer calls in tests require named parameters (`causer: "..."`, `message: "..."`). Worth bearing in mind for production callers as well.

---

## Commit

`46152fb` — `test(Log): add unit tests for JOSYN.System.Shared.Log` — pushed to `poc/evolution`.
