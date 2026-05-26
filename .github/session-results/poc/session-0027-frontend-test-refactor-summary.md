# Session 0027 — Frontend Test Structure Refactor

**Date:** 2026-05-26
**Story:** poc
**Type:** summary

---

## What Was Done

Collapsed the over-structured 3-project Frontend test setup (introduced in session 0021) into a single test project.

---

## Problem

`JobInvoker.FindJobFunction` scanned an assembly via `GetExportedTypes()`. Tests needing different entry-point counts (0, 1, 2) could not coexist in the same assembly, so session 0021 introduced two dedicated fixture assemblies (`Fixtures.Single`, `Fixtures.Multi`). This was correctly identified as a workaround, not a real design decision.

---

## Solution

- `FindJobFunction(Assembly)` → refactored to `internal FindJobFunction(IEnumerable<Type>)`; the assembly-based call in `InvokeJob` delegates to it via `GetExportedTypes()`.
- Added `internal InvokeJob(IJosynApplicationProtocol, IEnumerable<Type>)` overload — allows tests to supply types directly, bypassing assembly scanning entirely.
- Moved job stubs (`StubVoidJob`, `StubJobAlpha`, `StubJobBeta`) as `internal` types into `JobInvokerTestSupport.cs` — they are invisible to `GetExportedTypes()` and do not interfere with each other.
- Deleted `JOSYN.System.Frontend.JobHost.Test.Fixtures.Single` and `…Fixtures.Multi` projects and removed them from `.csproj` and `.slnx`.

---

## Result

- 3 projects → 1 project; all 3 tests remain, all green.
- No fixture assemblies, no `ProjectReference` scaffolding for test-only types.
- Production behaviour unchanged; `InvokeJob(japClient)` still scans the entry assembly at runtime.
