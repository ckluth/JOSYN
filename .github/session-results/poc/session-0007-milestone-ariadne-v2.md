# JOSYN PoC — Milestone Checkpoint & Ariadne's Thread v2

**Story:** poc  
**Session:** 0007  
**Type:** summary  
**Date:** 2026-05-24  
**Purpose:** Two-in-one document.
- **Part A** — Human milestone report: what was built, decisions made, where we stand.
- **Part B** — Ariadne's Thread v2: full AI continuation context. Supersedes session-0005.
  Read Part B first when starting a new session with "continue poc".

---

# PART A — HUMAN MILESTONE REPORT

> *For the Pilot-in-Charge. The Co-Pilot had the wheel for a while — here is a full debrief.*

---

## What We Set Out To Do (PoC Goals)

JOSYN ("JobSystem Next") is a job execution system in its genesis/PoC phase.
The goal: reach **show-and-tell maturity** — code solid enough to present to colleagues,
with all building blocks documented, tested, packaged, and the full system wired up end-to-end.

The four-layer architecture we committed to:

```
Layer 1 — JOSYN.Foundation    (transport primitives, zero JOSYN dependencies)
Layer 2 — JOSYN.System.Contract  (application protocol, transport-agnostic)
Layer 3 — JOSYN.System.Frontend  (JobHost: the job client)
Layer 4 — JOSYN.System.Backend   (JAPServer: the job server)
```

---

## What Was Accomplished (Sessions 0001–0006)

### Sessions 0001–0002: Architecture Decisions

- **Naming resolved:** `JOSYN.Foundation` (was `JOSYN.Core`), `JOSYN.System` grouping layer
  accepted for Frontend/Backend/Contract.
- **Implementation order fixed:** bottom-up, strict dependency order.
- **No throw-away code:** every package must reach NuGet maturity before the next layer starts.

### Session 0003: JOSYN.Foundation.ResultPattern

- Physical rename: `JOSYN.Core.ResultPattern` → `JOSYN.Foundation.ResultPattern`
- Namespace: `JOSYN.Foundation.ResultPattern`
- Tests: **113 green**
- NuGet: `JOSYN.Foundation.ResultPattern.1.0.0-preview01.nupkg` ✅

### Session 0004: JOSYN.Foundation.PropertyBag

- Physical rename: `JOSYN.Core.PropertyBag` → `JOSYN.Foundation.PropertyBag`
- Namespace: `JOSYN.Foundation.PropertyBag`
- Tests: **47 green**
- NuGet: `JOSYN.Foundation.PropertyBag.1.0.0-preview01.nupkg` ✅

### Session 0005: Ariadne's Thread v1

AI continuity document written — captured full context for seamless continuation.

### Session 0006: JIP Maturity + JOSYN.System.Contract

**Two phases in one session:**

**Phase 1 — JOSYN.Foundation.JIP brought to full maturity:**
- XML documentation written in German for all remaining public interfaces:
  `IPipesServer`, `IPipesClient`, `IPipesProtocol`, `IServerStartArguments`
  (the Jip-layer interfaces `IJipProtocol`, `IRequest`, `IResponse` were already documented)
- All implementation class summaries updated to `/// <inheritdoc cref="IXxx"/>`
- README rewritten to Foundation standard (motivation, quick-start, architecture,
  reference tables, maintainer section) — replacing the 5-line placeholder
- NuGet refreshed: `JOSYN.Foundation.JIP.1.0.0-preview01.nupkg` ✅

**Phase 2 — JOSYN.JAP.Protocol → JOSYN.System.Contract:**
- Physical rename: `JOSYN.JAP/JOSYN.JAP.Protocol/` → `JOSYN.System/JOSYN.System.Contract/`
- Namespace cleaned: `JOSYN.System.Contract` (no more `#pragma warning disable IDE0130`)
- `IJosynApplicationProtocol` received full German XML docs
- Full self-contained repo scaffold: `.slnx`, `.local-build`, `nuget.config`,
  `Directory.Build.props`, `README.md`
- Key architectural decision: Contract depends **only on ResultPattern**, not on JIP.
  The contract is transport-agnostic; only the implementations know about pipes.
- `JOSYN.JAP/` reduced to `.gitkeep`
- NuGet: `JOSYN.System.Contract.1.0.0-preview01.nupkg` ✅

---

## Current State — What's Done vs. What's Left

| Layer | Package | Tests | NuGet | Status |
|---|---|---|---|---|
| Foundation | `JOSYN.Foundation.ResultPattern` | 113 ✅ | ✅ | **Done** |
| Foundation | `JOSYN.Foundation.PropertyBag` | 47 ✅ | ✅ | **Done** |
| Foundation | `JOSYN.Foundation.JIP` | 1 ✅ | ✅ | **Done** |
| System | `JOSYN.System.Contract` | — | ✅ | **Done** |
| System | `JOSYN.System.Frontend` (JobHost) | — | ❌ | **Next** |
| System | `JOSYN.System.Backend` (JAPServer) | — | ❌ | Pending |
| — | Final docs + logical archive | — | — | Pending |

**4 of 6 packages done and packed.**

---

## What Remains

### Next session (0008): `JOSYN.System.Frontend`

The `JOSYN.JobHost/` folder exists but is placeholder quality. Work:
1. Assess current content
2. Rename/restructure → `JOSYN.System/JOSYN.System.Frontend/`
3. Wire up: `JOSYN.Foundation.JIP` + `JOSYN.System.Contract`
4. XML docs + README to Foundation standard
5. Pack as `JOSYN.System.Frontend.1.0.0-preview01.nupkg`

### Session 0009: `JOSYN.System.Backend`

Same treatment for `JOSYN.System/JOSYN.System.JAPServer/` → proper maturity + pack.

### Session 0010: Final documentation + logical archive

End-state doc, repo-wide README update, freeze.

---

## Key Decisions Made (All Sessions)

| # | Decision |
|---|---|
| 1 | Physical rename: `JOSYN.Core.*` → `JOSYN.Foundation.*`; `JOSYN.Core.IPC` → `JOSYN.Foundation.JIP` |
| 2 | Implementation order: strict bottom-up (Foundation → Contract → Frontend → Backend) |
| 3 | All packages reach NuGet maturity (README, XML docs, pack) before the next layer starts |
| 4 | `JOSYN.System` grouping layer accepted: `.Frontend`, `.Backend`, `.Contract` |
| 5 | `JOSYN.System.Contract` depends only on `ResultPattern` — transport-agnostic by design |
| 6 | Layer packages live inside their layer folder: `JOSYN.System/JOSYN.System.Contract/`, etc. |
| 7 | Error messages in German throughout (project-wide convention) |
| 8 | `de-DE` default culture (affects number/date formatting) |

---

---

# PART B — ARIADNE'S THREAD v2

> *For the AI. Read this first when the user says "continue poc" or similar.*  
> *Supersedes session-0005. Written after session-0006.*

---

## 1. What Is JOSYN (One Paragraph)

JOSYN ("JobSystem Next") is a physical multi-repo monorepo in its genesis/PoC phase.
It is a job execution system: a **JobHost** (frontend/client) dispatches jobs via
**JIP** (JOSYN Interprocess Protocol, named-pipe IPC) to a **JAPServer** (backend/server).
The application-level protocol between them is **JAP** (JOSYN Application Protocol),
defined as `IJosynApplicationProtocol` in `JOSYN.System.Contract` with two methods:
`GetRawArguments()` and `PutRawResult(string)`.
All code is **.NET 10**, C# `latest`, `Nullable` enabled, **functional-first C#**
(static classes by default, Result pattern everywhere, no exceptions above catch blocks,
errors as values via `Result` / `Result<T>` from `JOSYN.Foundation.ResultPattern`).

---

## 2. PoC Goal

- Reach **show-and-tell maturity** suitable for colleagues.
- Produce a **comprehensive final documentation** with all next steps clearly defined.
- Reach a **logically archivable / frozen state** for the entire meta-repo.
- Not throw-away code: **mature building-block implementations** with XML docs, README, NuGet.

---

## 3. Repository Map (Current Physical State)

```
JOSYN/
├── .github/
│   └── session-results/
│       └── poc/           ← this story: sessions 0001–0007
├── JOSYN.Foundation/
│   ├── JOSYN.Foundation.ResultPattern/   ✅ packed 1.0.0-preview01 | 113 tests
│   ├── JOSYN.Foundation.PropertyBag/     ✅ packed 1.0.0-preview01 | 47 tests
│   └── JOSYN.Foundation.JIP/             ✅ packed 1.0.0-preview01 | 1 test
├── JOSYN.JAP/             ← tombstone folder (.gitkeep only); content moved
├── JOSYN.JobHost/         ← JOSYN.System.Frontend (PENDING — placeholder quality)
│   ├── JOSYN.JobHost/
│   └── JOSYN.MyDemoJob/
├── JOSYN.System/
│   ├── JOSYN.System.Contract/            ✅ packed 1.0.0-preview01
│   │   ├── .local-build/
│   │   ├── Directory.Build.props
│   │   ├── nuget.config
│   │   ├── JOSYN.System.Contract.slnx
│   │   └── JOSYN.System.Contract/
│   │       ├── JOSYN.System.Contract.csproj
│   │       ├── IJosynApplicationProtocol.cs
│   │       └── README.md
│   └── JOSYN.System.JAPServer/           ⏳ JOSYN.System.Backend (pending)
│       └── JOSYN.System.JapServer.Server/
├── Local Packages/
│   ├── JOSYN.Foundation.ResultPattern.1.0.0-preview01.nupkg
│   ├── JOSYN.Foundation.PropertyBag.1.0.0-preview01.nupkg
│   ├── JOSYN.Foundation.JIP.1.0.0-preview01.nupkg
│   └── JOSYN.System.Contract.1.0.0-preview01.nupkg
└── README.md
```

**Note:** `JOSYN.Core/` no longer exists. All former Core packages live under `JOSYN.Foundation/`.

---

## 4. Implementation Order (Bottom-Up)

| # | Item | Status |
|---|---|---|
| 1 | `JOSYN.Foundation.ResultPattern` | ✅ Done + Packed |
| 2 | `JOSYN.Foundation.PropertyBag` | ✅ Done + Packed |
| 3 | `JOSYN.Foundation.JIP` | ✅ Done + Packed |
| 4 | `JOSYN.System.Contract` | ✅ Done + Packed |
| 5 | `JOSYN.System.Frontend` (was `JOSYN.JobHost`) | ⏳ **NEXT** |
| 6 | `JOSYN.System.Backend` (was `JOSYN.System.JAPServer`) | ⏳ Pending |
| 7 | Final documentation + logical archive | ⏳ Pending |

---

## 5. IMMEDIATE NEXT STEP

**When user says "continue" — this is the answer:**

### Step 5 — `JOSYN.System.Frontend` (session 0008)

The `JOSYN.JobHost/` folder exists but is placeholder quality. Before starting:

1. **Assess** — read `JOSYN.JobHost/JOSYN.JobHost/` to understand what's there.
2. **Rename/restructure** — `JOSYN.JobHost/` → `JOSYN.System/JOSYN.System.Frontend/`
   following the same self-contained sub-repo pattern as `JOSYN.System.Contract/`.
3. **Wire dependencies** — reference `JOSYN.Foundation.JIP` + `JOSYN.System.Contract`
   from the local NuGet feed.
4. **XML docs + README** — to Foundation standard.
5. **Pack** — `JOSYN.System.Frontend.1.0.0-preview01.nupkg`.
6. **`JOSYN.JobHost/`** — reduce to `.gitkeep` after move.

---

## 6. Key Architectural Facts

### The JAP Protocol (application layer)

```csharp
// In JOSYN.System.Contract, namespace JOSYN.System.Contract:
public interface IJosynApplicationProtocol
{
    Task<Result<string>> GetRawArguments();   // JobHost calls this to get job args
    Task<Result> PutRawResult(string result); // JobHost calls this to return result
}
```

The contract is **transport-agnostic** — it says nothing about pipes.
The transport (JIP named pipes) is wired up in the implementations (Frontend + Backend).

### JIP Transport (two-pipe design)

```
Client                              Server
  │── req-pipe-<sessionKey> ──────►  │
  │◄── res-pipe-<sessionKey> ────────│
```

- Session isolated via `Guid`-based session keys
- CLI convention: server passes `"JOSYN-IPC <sessionKey>"` to client exe as args
- Length-prefix framing: `int32` (LE) + raw bytes on both sides

### JIP Two-Layer Structure

**Transport layer** (`PipesServer`, `PipesClient`, `PipesProtocol`, `ServerStartArguments`):
Understands bytes only. Magic tokens: `JOSYN-IPC`, `JOSYN-IPC-ERROR`, `JOSYN-IPC-ERROR-BUSY`, `JOSYN-IPC-SHUTDOWN`.

**Convention layer** (`JipClient`, `JipServer`, `JipProtocol`, `JipDispatcher`):
JSON-based `Request { What, Data? }` / `Response { Succeeded, Data?, Error? }`.
Handler signature: `Func<Request, Task<Result<string?>>>`.

### Known JIP Dirt Spots (Documented — Do NOT Fix Unless Asked)

| Limitation | Description |
|---|---|
| Single-in-flight | No multiplexing; requests are strictly sequential |
| Sync-ish handler | `Func<string, Task<string>>` — adequate for PoC |
| `ClientPipes`/`ServerPipes` as `record` | Semantically should be `sealed class` |

---

## 7. Code Conventions (Non-Negotiable)

| Topic | Rule |
|---|---|
| Default type | `static class` — instance only if justified |
| Data types | `record` over `class`; `init`-only properties |
| Error handling | `Result`/`Result<T>` — never `throw` above catch boundary |
| Exception boundary | `catch (Exception ex) { return ex; }` |
| Propagation | `Result.Propagate(inner)` — never re-wrap |
| Interfaces | `static abstract` members in `Contracts/` folder; `/// <inheritdoc cref="IXxx"/>` on static impls |
| Explicitness | No DI containers, no reflection wiring |
| Error messages | **German** (project-wide convention) |
| Culture | `de-DE` default (numbers, dates) |
| XML docs | On interfaces; implementations use `<inheritdoc>` |

---

## 8. Build & Pack Reference

Each self-contained sub-repo has `.local-build/` scripts:

| Command | Action |
|---|---|
| `.local-build\build.cmd` | Release build (default) |
| `.local-build\build.cmd Debug` | Debug build |
| `.local-build\build.release.cmd` | Release shortcut |
| `.local-build\build.debug.cmd` | Debug shortcut |
| `.local-build\test.cmd` | `dotnet test` |
| `.local-build\pack.cmd` | Pack → `..\..\Local Packages\` |

- Solution format: `.slnx` (not `.sln`)
- Test framework: NUnit 4.x (`[TestFixture]` / `[Test]`)
- NuGet feed: `nuget.config` → `..\..\Local Packages\` (relative to sub-repo root)
- Build output: `C:\Temp\VS.OUT\JOSYN\<ProjectName>\` (set in `Directory.Build.props`)

---

## 9. Self-Contained Sub-Repo Scaffold (Template)

Each logical package follows this pattern (example: `JOSYN.System.Contract`):

```
JOSYN.System/
└── JOSYN.System.Contract/          ← sub-repo root
    ├── .local-build/
    │   ├── build.cmd
    │   ├── build.debug.cmd
    │   ├── build.release.cmd
    │   ├── test.cmd
    │   └── pack.cmd                ← "dotnet pack JOSYN.X.Y --output ..\..\Local Packages"
    ├── Directory.Build.props       ← BuildRoot = C:\Temp\VS.OUT\JOSYN\
    ├── nuget.config                ← "..\\..\Local Packages"
    ├── JOSYN.System.Contract.slnx
    └── JOSYN.System.Contract/      ← project folder
        ├── JOSYN.System.Contract.csproj
        ├── README.md
        └── [source files]
```

**nuget.config path depth:** `..\..\Local Packages` (from sub-repo root, two levels up to JOSYN root).

---

## 10. PropertyBag Quick Reference

Serializes C# `record` types to/from `Dictionary<string, string>` → sectionless INI or JSON.

```csharp
var result = PropertyBag.Serialize(myRecord, IniDictionarySerializer.Serialize);
var result = PropertyBag.Deserialize<MyRecord>(rawString); // auto-detects INI vs JSON
```

- **Records only** — checks for `<Clone>$` method
- **`de-DE` culture** — number/date formatting
- **Auto-detection** — JSON if input starts with `{`, otherwise INI
- Supported types listed in `SupportedPropertyTypes.cs`

---

## 11. Result Pattern Quick Reference

```csharp
// Success
return Result.Success;
return Result<T>.Success(value);   // or implicit: return value;

// Failure
return Result.Error("Fehlermeldung");   // idiomatisch
return Result.Fail("Fehlermeldung");    // explizit

// Exception boundary (lowest layer only)
catch (Exception ex) { return ex; }

// Propagate up the chain
var inner = SomeOperation();
if (!inner.Succeeded) return Result.Propagate(inner);

// Consume
if (!result.Succeeded)
{
    Console.Error.WriteLine(result.ErrorMessage);
    Console.Error.WriteLine(result.CallStackAsString);
}
var value = result.Value; // only after Succeeded == true
```

---

*This document was written after poc/session-0006 (JIP maturity + JOSYN.System.Contract).*  
*It supersedes session-0005 (Ariadne's Thread v1).*  
*Next session target: JOSYN.System.Frontend (session 0008).*
