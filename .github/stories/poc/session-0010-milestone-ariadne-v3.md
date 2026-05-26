# JOSYN PoC — Milestone Checkpoint & Ariadne's Thread v3

**Story:** poc  
**Session:** 0010  
**Type:** summary  
**Date:** 2026-05-24  
**Purpose:** Two-in-one document.
- **Part A** — Human milestone report: what was built, decisions made, where we stand.
- **Part B** — Ariadne's Thread v3: full AI continuation context. Supersedes session-0007.
  Read Part B first when starting a new session with "continue poc".

---

# PART A — HUMAN MILESTONE REPORT

> *For the Pilot-in-Charge.*

---

## What Was Accomplished (Sessions 0001–0009)

### Sessions 0001–0002: Architecture Decisions

Naming resolved, implementation order fixed, no-throw-away-code mandate established.

### Session 0003: JOSYN.Foundation.ResultPattern

Physical rename `JOSYN.Core.ResultPattern` → `JOSYN.Foundation.ResultPattern`. 113 tests green.
NuGet `JOSYN.Foundation.ResultPattern.1.0.0-preview01.nupkg` ✅

### Session 0004: JOSYN.Foundation.PropertyBag

Physical rename `JOSYN.Core.PropertyBag` → `JOSYN.Foundation.PropertyBag`. 47 tests green.
NuGet `JOSYN.Foundation.PropertyBag.1.0.0-preview01.nupkg` ✅

### Session 0006: JIP Maturity + JOSYN.System.Contract

- `JOSYN.Foundation.JIP` brought to full maturity (XML docs, README). 1 test. NuGet ✅
- `JOSYN.JAP.Protocol` physically renamed → `JOSYN.System.Contract`; transport-agnostic contract.
  NuGet `JOSYN.System.Contract.1.0.0-preview01.nupkg` ✅

### Session 0007: Ariadne's Thread v2 (milestone checkpoint)

AI continuity document v2 written capturing full state after sessions 0001–0006.

### Session 0008: Reference Fixes + Sealing Pass (side-session)

Fixed stale `JOSYN.JAP.Protocol` references in JAPServer and JobHost. Sealed 16 types across
all packages.

### Session 0009: JOSYN.System.Frontend

`JOSYN.JobHost/` physically renamed and restructured → `JOSYN.System/JOSYN.System.Frontend/`
following the standard self-contained sub-repo scaffold. All namespaces updated, `Interfaces/`
→ `Contracts/`, all 5 attributes sealed + documented, `ICore` fully documented,
`ParallelExecutionAllowedAttribute` cleaned up, README written to Foundation standard.
NuGet `JOSYN.System.Frontend.1.0.0-preview01.nupkg` ✅. `JOSYN.JobHost/` removed entirely.

### Session 0010: This Milestone Checkpoint

Ariadne's Thread v3 written. Full repo committed and pushed.

---

## Current State

| Layer | Package | Tests | NuGet | Status |
|---|---|---|---|---|
| Foundation | `JOSYN.Foundation.ResultPattern` | 113 ✅ | ✅ | **Done** |
| Foundation | `JOSYN.Foundation.PropertyBag` | 47 ✅ | ✅ | **Done** |
| Foundation | `JOSYN.Foundation.JIP` | 1 ✅ | ✅ | **Done** |
| System | `JOSYN.System.Contract` | — | ✅ | **Done** |
| System | `JOSYN.System.Frontend` | — | ✅ | **Done** |
| System | `JOSYN.System.Backend` | — | ❌ | **Next** |
| — | Final docs + logical archive | — | — | Pending |

**5 of 6 packages done and packed.**

---

## What Remains

### Next session: `JOSYN.System.Backend`

`JOSYN.System/JOSYN.System.JAPServer/` must be brought to maturity and renamed/restructured
into a proper sub-repo at `JOSYN.System/JOSYN.System.Backend/`. Then final documentation and
logical archive to freeze the PoC.

---

---

# PART B — ARIADNE'S THREAD v3

> *For the AI. Read this first when the user says "continue poc" or similar.*  
> *Supersedes session-0007. Written after session-0009.*

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
│       └── poc/           ← this story: sessions 0001–0010
├── JOSYN.Foundation/
│   ├── JOSYN.Foundation.ResultPattern/   ✅ packed 1.0.0-preview01 | 113 tests
│   ├── JOSYN.Foundation.PropertyBag/     ✅ packed 1.0.0-preview01 | 47 tests
│   └── JOSYN.Foundation.JIP/             ✅ packed 1.0.0-preview01 | 1 test
├── JOSYN.JAP/             ← tombstone folder (gone; .gitkeep removed in session-0009 commit)
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
│   ├── JOSYN.System.Frontend/            ✅ packed 1.0.0-preview01
│   │   ├── .local-build/
│   │   ├── Directory.Build.props
│   │   ├── nuget.config
│   │   ├── JOSYN.System.Frontend.slnx
│   │   ├── JOSYN.System.Frontend/
│   │   │   ├── JOSYN.System.Frontend.csproj
│   │   │   ├── Core.cs                   ← public entry point: Core.Run(args)
│   │   │   ├── FakeCore.cs               ← internal error logging (no IPC server)
│   │   │   ├── ArgumentsComparer.cs      ← delegate for parallel-execution check
│   │   │   ├── JAPClient.cs              ← internal; implements IJosynApplicationProtocol via JIP
│   │   │   ├── JobInvoker.cs             ← internal static; reflection-based job dispatch
│   │   │   ├── Contracts/
│   │   │   │   └── ICore.cs              ← static abstract interface contract
│   │   │   └── Attributes/
│   │   │       ├── JobEntryPointAttribute.cs
│   │   │       ├── BeforeJobEntryAttribute.cs
│   │   │       ├── JobArgumentsAtribute.cs
│   │   │       ├── JobResultAttribute.cs
│   │   │       └── ParallelExecutionAlwaysAllowedAttribute.cs
│   │   └── JOSYN.MyDemoJob/              ← demo exe; in solution, NOT packed
│   │       ├── MyDemoJob.csproj
│   │       ├── Program.cs
│   │       ├── Mandatory/MyFirstJob.cs
│   │       └── Optional/  (MyArguments, MyResult, MyJobConfig, MyExecutionReport)
│   └── JOSYN.System.JAPServer/           ⏳ JOSYN.System.Backend (PENDING)
│       └── JOSYN.System.JapServer.Server/
├── Local Packages/
│   ├── JOSYN.Foundation.ResultPattern.1.0.0-preview01.nupkg
│   ├── JOSYN.Foundation.PropertyBag.1.0.0-preview01.nupkg
│   ├── JOSYN.Foundation.JIP.1.0.0-preview01.nupkg
│   ├── JOSYN.System.Contract.1.0.0-preview01.nupkg
│   └── JOSYN.System.Frontend.1.0.0-preview01.nupkg
└── README.md
```

**Notes:**
- `JOSYN.Core/` no longer exists. All former Core packages live under `JOSYN.Foundation/`.
- `JOSYN.JobHost/` no longer exists. Replaced by `JOSYN.System.Frontend/`.
- `JOSYN.JAP/` no longer exists. Replaced by `JOSYN.System.Contract/`.

---

## 4. Implementation Order (Bottom-Up)

| # | Item | Status |
|---|---|---|
| 1 | `JOSYN.Foundation.ResultPattern` | ✅ Done + Packed |
| 2 | `JOSYN.Foundation.PropertyBag` | ✅ Done + Packed |
| 3 | `JOSYN.Foundation.JIP` | ✅ Done + Packed |
| 4 | `JOSYN.System.Contract` | ✅ Done + Packed |
| 5 | `JOSYN.System.Frontend` | ✅ Done + Packed |
| 6 | `JOSYN.System.Backend` (was `JOSYN.System.JAPServer`) | ⏳ **NEXT** |
| 7 | Final documentation + logical archive | ⏳ Pending |

---

## 5. IMMEDIATE NEXT STEP

**When user says "continue" — this is the answer:**

### Step 6 — `JOSYN.System.Backend`

The `JOSYN.System/JOSYN.System.JAPServer/` folder exists but is placeholder quality. Work:

1. **Assess** — read `JOSYN.System.JAPServer/JOSYN.System.JapServer.Server/` to understand what's there.
2. **Rename/restructure** → `JOSYN.System/JOSYN.System.Backend/`
   following the standard self-contained sub-repo pattern.
3. **Namespace** → `JOSYN.System.Backend`
4. **Wire dependencies** — `JOSYN.Foundation.JIP` + `JOSYN.System.Contract`
5. **XML docs + README** — to Foundation standard (German error messages, English docs).
6. **Pack** → `JOSYN.System.Backend.1.0.0-preview01.nupkg`.
7. **`JOSYN.System.JAPServer/`** — remove (no `.gitkeep` needed; folder ceases to exist).

After Backend is done, one final session remains: end-state doc + repo-wide README + freeze.

---

## 6. Key Architectural Facts

### The JAP Protocol (application layer)

```csharp
// In JOSYN.System.Contract, namespace JOSYN.System.Contract:
public interface IJosynApplicationProtocol
{
    Task<Result<string>> GetRawArguments();   // Backend calls: JobHost fetches job args
    Task<Result> PutRawResult(string result); // Backend calls: JobHost returns result
}
```

The contract is **transport-agnostic** — it says nothing about pipes.
The transport (JIP named pipes) is wired up in Frontend (`JAPClient`) and Backend (`JAPServer`).

### How Frontend Works

```
Job.exe (JOSYN.MyDemoJob pattern)
  └── Core.Run(args)
       ├── JAPClient.CreateConnectedClient(args)  — parse session key, connect via JIP
       └── JobInvoker.InvokeJob(japClient)
            ├── FindEntryPointAssembly             — Entry assembly
            ├── FindJobFunction                    — [JobEntryPoint] via reflection
            ├── CreateInvocationArguments          — GetRawArguments() → PropertyBag.Deserialize
            ├── [Your job method]                  — business logic
            └── ProcessJobResult                   — PropertyBag.Serialize → PutRawResult()
```

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

## 7. Frontend — JobInvoker Reflection Design (Important)

`JobInvoker` uses `Assembly.GetEntryAssembly()` and `[JobEntryPoint]` attribute to locate
the user's job method. **This reflection use is intentional and is the defined extension
point.** It is NOT "hidden magic" — it is documented design. Do not flag it as a violation of
the "no reflection-based wiring" principle.

Job authors:
- Reference `JOSYN.System.Frontend`
- Mark exactly one `public static` method with `[JobEntryPoint]`
- Call `Core.Run(args)` from `Program.cs`

---

## 8. Code Conventions (Non-Negotiable)

| Topic | Rule |
|---|---|
| Default type | `static class` — instance only if justified |
| Data types | `record` over `class`; `init`-only properties |
| Error handling | `Result`/`Result<T>` — never `throw` above catch boundary |
| Exception boundary | `catch (Exception ex) { return ex; }` |
| Propagation | `Result.Propagate(inner)` — never re-wrap |
| Interfaces | `static abstract` members in `Contracts/` folder; `/// <inheritdoc cref="IXxx"/>` on static impls |
| Explicitness | No DI containers, no reflection wiring (except `JobInvoker` — intentional) |
| Error messages | **German** (project-wide convention) |
| Culture | `de-DE` default (numbers, dates) |
| XML docs | On interfaces/contracts; implementations use `<inheritdoc>` |

---

## 9. Build & Pack Reference

Each self-contained sub-repo has `.local-build/` scripts at the **sub-repo root**:

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

## 10. Self-Contained Sub-Repo Scaffold (Template)

Each logical package follows this pattern (example based on `JOSYN.System.Frontend`):

```
JOSYN.System/
└── JOSYN.System.Frontend/          ← sub-repo root
    ├── .local-build/
    │   ├── build.cmd
    │   ├── build.debug.cmd
    │   ├── build.release.cmd
    │   ├── test.cmd
    │   └── pack.cmd                ← "dotnet pack JOSYN.X.Y --output ..\..\Local Packages"
    ├── Directory.Build.props       ← BuildRoot = C:\Temp\VS.OUT\JOSYN\
    ├── nuget.config                ← "..\\..\Local Packages"
    ├── JOSYN.System.Frontend.slnx
    └── JOSYN.System.Frontend/      ← project folder
        ├── JOSYN.System.Frontend.csproj
        ├── README.md
        └── [source files]
```

**nuget.config path depth:** `..\..\Local Packages` (from sub-repo root, two levels up to JOSYN root).

---

## 11. PropertyBag Quick Reference

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

## 12. Result Pattern Quick Reference

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

*This document was written after poc/session-0009 (JOSYN.System.Frontend).*  
*It supersedes session-0007 (Ariadne's Thread v2).*  
*Next session target: JOSYN.System.Backend.*
