# Session 0021 — Unit Test Coverage: Comprehensive Gap-Fill Across All Solutions

**Date:** 2026-05-25  
**Story:** poc  
**Type:** generation

---

## What Was Done

A full unit-test gap analysis and fill across the entire JOSYN repository. All testable solutions were reviewed; genuine gaps were identified and filled without rewriting tests that were already solid.

---

## Scope Decisions (What to Test / Skip)

| Solution | Decision | Rationale |
|---|---|---|
| `JOSYN.Foundation.JIP` | ✅ New test project | Zero unit tests for the core library; only demo-exe test existed |
| `JOSYN.Foundation.PropertyBag` | ✅ Augment existing | 47 tests existed; type converters and edge cases uncovered |
| `JOSYN.Foundation.ResultPattern` | ✅ Augment existing | 113 tests existed; 4 genuine gaps identified |
| `JOSYN.System.Frontend` | ✅ New test project | Zero unit tests; `JobInvoker` was fully untested |
| `JOSYN.System.Shared` | ⛔ Skip | `ErrorReport` is a trivial data record; `LocalLog` writes to filesystem — integration territory |
| `JOSYN.System.Backend` | ⛔ Skip | `JAPServer` is a console exe orchestrating IPC — integration territory |
| Demo executables | ⛔ Skip | PoC / exploratory code, not stable enough for pinning |

---

## Test Counts

| Solution | Before | After | Delta |
|---|---|---|---|
| `JOSYN.Foundation.JIP` | 1 (demo-exe test) | 49 | +48 |
| `JOSYN.Foundation.PropertyBag` | 47 | 61 | +14 |
| `JOSYN.Foundation.ResultPattern` | 113 | 125 | +12 |
| `JOSYN.System.Frontend` | 0 | 3 | +3 |
| **Total** | **161** | **238** | **+77** |

All 238 tests green, 0 compiler warnings.

---

## JOSYN.Foundation.JIP — New Test Project

**Created:** `JOSYN.Foundation.JIP\JOSYN.Foundation.JIP.Test\` (new project, added to `.slnx`).

The existing test project (`JOSYN.Foundation.JIP.Demo.ServerExe.Test`) referenced the demo server exe, not the core library — so it was appropriate to create a separate library-level test project.

**4 test files, 48 tests:**

| File | Tests | Coverage |
|---|---|---|
| `PipesProtocolTests.cs` | 11 | `PipesProtocol` static methods: CLI arg formatting/parsing, pipe name derivation |
| `JipProtocolTests.cs` | 13 | `JipProtocol`: `ParseRequest`, `ParseResponse`, `ToResponse`, `ToResult` |
| `JipDispatcherTests.cs` | 18 | `JipDispatcher`: all 5 `Register` overloads, `Dispatch`, `RegisterAll` edge cases |
| `WireTypesTests.cs` | 7 | `Request`/`Response` wire types and round-trips |

**Key technical notes:**
- `JipDispatcher.RegisterAll` uses `typeof(TProtocol).GetMethods()` — does NOT return Object base methods for interfaces; empty interface → registers nothing (confirmed by test).
- `Result<T>.Value` is `T?`; after asserting `Succeeded`, use null-forgiving operator `result.Value!.Something` to suppress CS8602.
- `JipDispatcher.RegisterAll` throws `InvalidOperationException` for unsupported method signatures (not silent-skip).

---

## JOSYN.Foundation.PropertyBag — Augmented Tests (+14)

**Files modified:**
- `PropertyBagTests.cs`: added `AdditionalTypesRecord` + 12 new tests covering round-trips for `bool`, `Guid`, `TimeSpan`, `decimal`, `DateTime`, `DateOnly`, `TimeOnly`; explicit de-DE decimal assertion (`"1234,56"`); both default-serializer overloads (`Serialize<TRecord>(record)` and `Serialize(object, Type)`).
- `IniDictionarySerializerTests.cs`: 2 edge cases — `\r\n` line endings parsed correctly, line without `=` silently ignored.
- `JsonDictionarySerializerTests.cs`: 2 edge cases — empty `Dictionary` → `{}`, empty `{}` → empty dictionary.

**Key technical note:** The `CultureAwareXxxConverter` JSON converters only fire if `JsonDictionarySerializer` is called directly with a non-string type — PropertyBag always routes through `Dictionary<string,string>` so the converters are not exercised by standard PropertyBag tests.

---

## JOSYN.Foundation.ResultPattern — Augmented Tests (+12)

### New file: `CallerInfoTests.cs` (4 tests)
Pins `CallerInfo.ToString()` — two distinct paths in the source:
- With file + non-zero line → `"ClassName.Method() in File.cs:42"` format.
- Empty file path → `"ClassName.Method()"` (short form).
- Zero line number → `"ClassName.Method()"` (short form).
- Default record (all empty) → `".()"`.

### Additions to `ResultTests.cs` and `ResultGenericTests.cs` (+8 tests, 4 each):
| Test | What it pins |
|---|---|
| `Fail_Exception_ErrorMessage_HasAusnahmefehlerPrefix` | `ErrorMessage` always starts with `"Ausnahmefehler: "` (German prefix via `ResultHelper.FormatExceptionMessage`) |
| `ImplicitConversion_FromException_ErrorMessage_HasAusnahmefehlerPrefix` | Same for the implicit `Exception → Result` conversion path |
| `Fail_String_CallerInfo_HasMethodName` | `Callers[0].MethodName` is captured and non-empty |
| `ToResult_Generic_OnSucceeded_ThrowsDebugAssert` | Calling `ToResult<T>()` on a *succeeded* `Result` fires `Debug.Assert` (programming-error contract) |

**Key technical note:** `ToResult<T>()` on a succeeded result triggers `Debug.Assert` which throws `DebugAssertException` in the test runner. The test therefore uses `Throws.InstanceOf<Exception>()`, not an outcome assertion. This is by design — calling it on a succeeded result is a programming error.

---

## JOSYN.System.Frontend — New Test Project (+3 tests)

### Why it was non-trivial

`JobInvoker.FindJobFunction` scans **the entire assembly** (via `GetExportedTypes()`) for `[JobEntryPoint]` methods. A single test assembly with multiple public job stubs causes every test to report "Mehrere Methoden gefunden". The solution is separate fixture assemblies, one per distinct "entry-point count" scenario.

### Project structure created:

```
JOSYN.System.Frontend\
  JOSYN.System.Frontend.JobHost.Test\             ← main test project (no public job stubs)
    JobInvokerTestSupport.cs                       ← FakeProtocol, FailingGetArgumentsProtocol (internal)
    JobInvokerTests.cs                             ← 3 tests
  JOSYN.System.Frontend.JobHost.Test.Fixtures.Single\   ← exactly 1 [JobEntryPoint] (VoidJob.Run)
  JOSYN.System.Frontend.JobHost.Test.Fixtures.Multi\    ← exactly 2 [JobEntryPoint] (JobAlpha.Run, JobBeta.Run)
```

`InternalsVisibleTo("JOSYN.System.Frontend.JobHost.Test")` added to `JobHost.csproj` via `AssemblyAttribute` item.

### 3 tests:

| Test | Fixture assembly | What it pins |
|---|---|---|
| `InvokeJob_NoEntryPoint_Fails` | Test assembly itself (0 exported entry points) | Correct error message containing `JobEntryPointAttribute` name |
| `InvokeJob_MultipleEntryPoints_Fails` | `Fixtures.Multi` (2 entry points) | Error message contains `"Mehrere"` |
| `InvokeJob_VoidEntryPoint_Succeeds` | `Fixtures.Single` (1 void entry point) | Happy path: `Succeeded == true` |

### Not tested (deferred):
- Job that throws unhandled exception (needs its own fixture assembly)
- Entry point with record parameter (needs own fixture assembly)
- Entry point that returns a record result (same)
- These are cleanly testable as integration tests or with additional fixture projects in a future session.

---

## Architectural Observation (for future sessions)

`JobInvoker.FindJobFunction` is assembly-scoped by design — JOSYN assumes one job per assembly. This is a strong architectural constraint. Future unit tests for invocation scenarios **must** use dedicated one-job fixture assemblies. If this becomes tedious, consider exposing `FindJobFunction(Assembly)` as `internal` so it can be tested in isolation.

---

## Files Created

| File | Purpose |
|---|---|
| `JOSYN.Foundation.JIP\JOSYN.Foundation.JIP.Test\JOSYN.Foundation.JIP.Test.csproj` | New test project |
| `JOSYN.Foundation.JIP\JOSYN.Foundation.JIP.Test\PipesProtocolTests.cs` | 11 tests |
| `JOSYN.Foundation.JIP\JOSYN.Foundation.JIP.Test\JipProtocolTests.cs` | 13 tests |
| `JOSYN.Foundation.JIP\JOSYN.Foundation.JIP.Test\JipDispatcherTests.cs` | 18 tests |
| `JOSYN.Foundation.JIP\JOSYN.Foundation.JIP.Test\WireTypesTests.cs` | 7 tests |
| `JOSYN.Foundation.ResultPattern\JOSYN.Foundation.ResultPattern.Test\CallerInfoTests.cs` | 4 tests |
| `JOSYN.System.Frontend\JOSYN.System.Frontend.JobHost.Test\JOSYN.System.Frontend.JobHost.Test.csproj` | New test project |
| `JOSYN.System.Frontend\JOSYN.System.Frontend.JobHost.Test\JobInvokerTestSupport.cs` | Fake protocol implementations |
| `JOSYN.System.Frontend\JOSYN.System.Frontend.JobHost.Test\JobInvokerTests.cs` | 3 tests |
| `JOSYN.System.Frontend\JOSYN.System.Frontend.JobHost.Test.Fixtures.Single\*.csproj + VoidJob.cs` | 1-entry-point fixture |
| `JOSYN.System.Frontend\JOSYN.System.Frontend.JobHost.Test.Fixtures.Multi\*.csproj + MultipleJobs.cs` | 2-entry-point fixture |

## Files Modified

| File | Change |
|---|---|
| `JOSYN.Foundation.JIP\JOSYN.Foundation.JIP.slnx` | Added `JOSYN.Foundation.JIP.Test` project |
| `JOSYN.Foundation.PropertyBag\…\PropertyBagTests.cs` | `AdditionalTypesRecord` + 12 new tests |
| `JOSYN.Foundation.PropertyBag\…\IniDictionarySerializerTests.cs` | 2 edge case tests |
| `JOSYN.Foundation.PropertyBag\…\JsonDictionarySerializerTests.cs` | 2 edge case tests |
| `JOSYN.Foundation.ResultPattern\…\ResultTests.cs` | 4 new tests |
| `JOSYN.Foundation.ResultPattern\…\ResultGenericTests.cs` | 4 new tests |
| `JOSYN.System.Frontend\JOSYN.System.Frontend.JobHost\*.csproj` | Added `InternalsVisibleTo` AssemblyAttribute |
| `JOSYN.System.Frontend\JOSYN.System.Frontend.slnx` | Added 3 new projects |
