# JOSYN PoC — Milestone Checkpoint & Ariadne's Thread v4

**Story:** poc  
**Session:** 0012  
**Type:** summary  
**Date:** 2026-05-24  
**Purpose:** Two-in-one document.
- **Part A** — Human milestone report: the full PoC as it stands today — concepts first.
- **Part B** — Ariadne's Thread v4: complete AI continuation context. Supersedes session-0010.
  Read Part B first when starting a new session with "continue poc" or similar.

---

# PART A — HUMAN MILESTONE REPORT

> *For the Pilot-in-Charge.*

---

## What We Built — The Big Picture

JOSYN is a **job execution system**. A Job is a unit of work — a method annotated with
`[JobEntryPoint]` inside a standalone `.exe`. That exe does not run the job directly.
Instead it delegates: it connects to a **JAPServer** (a background process), hands off its work,
and receives the result back. The link is a named-pipe channel.

The ambition of this PoC is not just to make it work — it is to build it properly from the
ground up: typed, documented, composable NuGet packages that a second team member could
pick up without explanation.

---

## The Three Conceptual Layers

### Layer 1 — Foundation

Three self-contained utility packages. No JOSYN-specific logic. Reusable across any project.

| Package | What it is |
|---|---|
| `JOSYN.Foundation.ResultPattern` | The error-handling foundation. No exceptions above the catch boundary. Every operation returns `Result` (void) or `Result<T>`. Failures carry message + call chain. |
| `JOSYN.Foundation.PropertyBag` | Serializes C# `record` types to/from sectionless INI or JSON (auto-detected). Used to pass job arguments and results across the pipe. |
| `JOSYN.Foundation.JIP` | JOSYN Interprocess Protocol. Named-pipe transport in a two-layer design: raw byte transport (PipesServer/Client) + JSON request/response convention (JipServer/Client). |

All three are packed NuGet libraries. All have XML-documented public API, README, and tests.

### Layer 2 — System.Contract

A single interface — `IJosynApplicationProtocol` — that defines **what the Backend offers
the Frontend** at the application level:

```csharp
Task<Result<string>> GetRawArguments();   // Frontend calls: give me the job's arguments
Task<Result>         PutRawResult(string);// Frontend calls: here is the job's result
```

This contract is **transport-agnostic**. It says nothing about pipes. Both Frontend and
Backend depend on it independently. This is the seam between the two sub-domains.

### Layer 3 — System (Frontend + Backend)

**Frontend = `JOSYN.System.Frontend.JobHost`** (a NuGet library)

The Job Developer's world. A job author:
1. References this package.
2. Marks one `public static` method with `[JobEntryPoint]`.
3. Calls `Core.Run(args)` from `Program.cs`.

The library handles everything else: parses the session key from CLI args, connects via JIP,
fetches arguments from the Backend (`GetRawArguments`), deserializes them via PropertyBag,
invokes the job method via reflection (`JobInvoker`), serializes the result, and sends it
back (`PutRawResult`). The reflection here is **intentional and documented design** — it is
the defined extension point, not hidden magic.

**Backend = `JOSYN.System.Backend.JAPServer`** (an exe, not a library)

The Backend is the process that bridges the two sides. It:
1. Starts. Listens on its named pipe.
2. Launches a job exe, passing `"JOSYN-IPC <sessionKey>"` as CLI args.
3. Handles the two JAP calls from the job (GetRawArguments, PutRawResult).
4. Shuts down cleanly (ESC key detected via `WasEscapePressed`, graceful drain).

It is an exe because it has only one consumer (itself). No NuGet packaging needed or appropriate.

---

## The Namespace Grouping Pattern

`JOSYN.System.Frontend` and `JOSYN.System.Backend` are **pure namespace containers** —
folder groupings with no code of their own. The concrete projects live one level deeper:

```
JOSYN.System/
  JOSYN.System.Frontend/          ← namespace grouping layer (folder + slnx + scaffold)
    JOSYN.System.Frontend.JobHost/ ← THE library: namespace JOSYN.System.Frontend.JobHost
    JOSYN.MyDemoJob/               ← demo job exe (in solution, not packed)
  JOSYN.System.Backend/           ← namespace grouping layer
    JOSYN.System.Backend.JAPServer/ ← THE exe: namespace JOSYN.System.Backend.JAPServer
```

This mirrors the Foundation pattern (e.g., `JOSYN.Foundation/JOSYN.Foundation.JIP/JOSYN.Foundation.JIP/`).
The grouping folder = namespace root. Concrete project = full qualified name. No ambiguity.

---

## The Build System

### Root-level `.local-build/`

Four scripts at the repo root orchestrate everything:

| Script | What it does |
|---|---|
| `build-all.cmd` | Crystal-clean full build: (1) deletes all `.nupkg` from Local Packages, (2) clears NuGet cache for all JOSYN packages, (3) builds + packs all 6 sub-repos in dependency order |
| `test-all.cmd` | Runs all tests across the 3 Foundation packages (161 tests) |
| `all.cmd` | build-all + test-all in sequence |
| `demo.cmd` | Launches JAPServer + MyDemoJob in separate console windows (Release) |
| `demo.debug.cmd` | Builds both in Debug, then launches them — activates `#if DEBUG` pause in JobHost |

### Dependency Order (Critical for NuGet)

Each package is built and **immediately packed** before the next one is built, so NuGet
restore for the next step finds the fresh package in Local Packages:

```
1. JOSYN.Foundation.ResultPattern  → pack
2. JOSYN.Foundation.PropertyBag    → pack  (depends on 1)
3. JOSYN.Foundation.JIP            → pack  (depends on 1)
4. JOSYN.System.Contract           → pack  (depends on 1)
5. JOSYN.System.Frontend.JobHost   → pack  (depends on 1,2,3,4)
6. JOSYN.System.Backend.JAPServer  → build only  (depends on 1,3,4; exe, not packed)
```

### Crystal-Clean Convention

`Local Packages/cleanup-nuget-cache.cmd` clears all JOSYN entries from the global NuGet
package cache (`~/.nuget/packages`). Called automatically by `build-all.cmd` (with `NOPAUSE`
argument to suppress interactive pause). This ensures zero stale-package risk.

---

## Package Status

| Layer | Package / Artefact | NuGet | Tests | Status |
|---|---|---|---|---|
| Foundation | `JOSYN.Foundation.ResultPattern` | ✅ | 113 ✅ | Done |
| Foundation | `JOSYN.Foundation.PropertyBag` | ✅ | 47 ✅ | Done |
| Foundation | `JOSYN.Foundation.JIP` | ✅ | 1 ✅ | Done |
| System | `JOSYN.System.Contract` | ✅ | — | Done |
| System | `JOSYN.System.Frontend.JobHost` | ✅ | — | Done |
| System | `JOSYN.System.Backend.JAPServer` | exe | — | Done |
| Root | `.local-build/` build scripts | — | — | Done |
| Root | Demo launcher (Release + Debug) | — | — | Done |

**All 6 sub-repos done. PoC is functionally complete and runnable.**

---

## What Is Not Yet Done (Logical Freeze)

The PoC is functionally complete. What remains is optional polish before freezing:
- Repo-wide `README.md` update to reflect the final structure.
- "Logical freeze" commit — a clean tagged commit marking end-of-PoC.
- Any architectural notes for the next phase (real multi-repo split, production concerns).

---

---

# PART B — ARIADNE'S THREAD v4

> *For the AI. Read this first when the user says "continue poc" or similar.*  
> *Supersedes session-0010 (Ariadne v3). Written after session-0012.*

---

## 1. What Is JOSYN (One Paragraph)

JOSYN ("JobSystem Next") is a physical multi-repo monorepo in its genesis/PoC phase.
It is a **job execution system**: a job is a standalone exe whose single method (marked
`[JobEntryPoint]`) is dispatched by the **JobHost** (Frontend library). The Frontend
connects to a **JAPServer** (Backend exe) via **JIP** (JOSYN Interprocess Protocol,
named pipes). The application-level protocol between them is **JAP** (`IJosynApplicationProtocol`
in `JOSYN.System.Contract`): two calls — `GetRawArguments()` and `PutRawResult(string)`.
All code targets **.NET 10**, C# `latest`, `Nullable` enabled throughout.
Code style: **functional-first C#** — static classes by default, `Result`/`Result<T>`
for all error handling, no exceptions above the catch boundary, no DI containers,
errors as values.

---

## 2. PoC Goal (Achieved)

- Mature, NuGet-packaged building blocks suitable for showing to colleagues.
- Full end-to-end runnable: JAPServer launches, MyDemoJob connects, job executes, result returned.
- Logical freeze state: next phase could start in clean, separate repos.
- Comprehensive documentation (this document) for both human and AI continuity.

---

## 3. Repository Map (Final Physical State)

```
JOSYN/
├── .github/
│   └── session-results/
│       └── poc/                     ← sessions 0001–0012
│           ├── _index.md
│           └── session-0012-milestone-ariadne-v4.md   ← THIS FILE
├── .local-build/                    ← ROOT build orchestration
│   ├── all.cmd                      ← build-all + test-all
│   ├── build-all.cmd                ← crystal-clean build + pack all 6 in order
│   ├── test-all.cmd                 ← dotnet test all Foundation packages (161 tests)
│   ├── demo.cmd                     ← launch JAPServer + MyDemoJob [Release]
│   └── demo.debug.cmd               ← build Debug + launch both (activates #if DEBUG pauses)
├── JOSYN.Foundation/
│   ├── JOSYN.Foundation.ResultPattern/   ✅ NuGet 1.0.0-preview01 | 113 tests
│   ├── JOSYN.Foundation.PropertyBag/     ✅ NuGet 1.0.0-preview01 | 47 tests
│   └── JOSYN.Foundation.JIP/             ✅ NuGet 1.0.0-preview01 | 1 test
├── JOSYN.System/
│   ├── JOSYN.System.Contract/            ✅ NuGet 1.0.0-preview01
│   │   └── JOSYN.System.Contract/
│   │       └── IJosynApplicationProtocol.cs
│   ├── JOSYN.System.Frontend/            ← namespace grouping layer
│   │   ├── JOSYN.System.Frontend.slnx
│   │   ├── JOSYN.System.Frontend.JobHost/  ✅ NuGet 1.0.0-preview01
│   │   │   ├── Core.cs                   — public entry point: Core.Run(args)
│   │   │   ├── FakeCore.cs               — internal; graceful failure when no server
│   │   │   ├── JAPClient.cs              — internal; IJosynApplicationProtocol via JIP
│   │   │   ├── JobInvoker.cs             — internal static; reflection-based dispatch
│   │   │   ├── ArgumentsComparer.cs      — delegate type for parallel-execution check
│   │   │   ├── Contracts/ICore.cs        — static abstract interface
│   │   │   └── Attributes/               — 5 attributes (all sealed)
│   │   └── JOSYN.MyDemoJob/              — demo job exe (in solution, NOT packed)
│   │       ├── Program.cs                — one line: Core.Run(args)
│   │       └── Mandatory/MyFirstJob.cs   — [JobEntryPoint] method
│   └── JOSYN.System.Backend/             ← namespace grouping layer
│       ├── JOSYN.System.Backend.slnx
│       └── JOSYN.System.Backend.JAPServer/  ← exe, NOT packed
│           ├── Program.cs                — entry point: Backend.Run(args)
│           ├── Backend.cs                — internal static; server lifecycle
│           ├── JAPServer.cs              — internal; IJosynApplicationProtocol impl
│           └── Properties/launchSettings.json
├── Local Packages/
│   ├── cleanup-nuget-cache.cmd           ← clears all JOSYN NuGet cache entries
│   ├── JOSYN.Foundation.ResultPattern.1.0.0-preview01.nupkg
│   ├── JOSYN.Foundation.PropertyBag.1.0.0-preview01.nupkg
│   ├── JOSYN.Foundation.JIP.1.0.0-preview01.nupkg
│   ├── JOSYN.System.Contract.1.0.0-preview01.nupkg
│   └── JOSYN.System.Frontend.JobHost.1.0.0-preview01.nupkg
└── README.md
```

**Tombstoned (removed entirely, no .gitkeep):**
- `JOSYN.Core/` → replaced by `JOSYN.Foundation/`
- `JOSYN.JobHost/` → replaced by `JOSYN.System.Frontend/`
- `JOSYN.JAP/` → replaced by `JOSYN.System.Contract/`
- `JOSYN.System.JAPServer/` → replaced by `JOSYN.System.Backend/`

---

## 4. Key Concepts — Read Before Touching Anything

### 4.1 The JAP Protocol (application layer)

```csharp
// JOSYN.System.Contract, namespace JOSYN.System.Contract
public interface IJosynApplicationProtocol
{
    Task<Result<string>> GetRawArguments();   // Backend calls: Frontend fetches job args
    Task<Result>         PutRawResult(string);// Backend calls: Frontend sends back result
}
```

Transport-agnostic. Both sides implement it independently. The transport (JIP named pipes)
is wired in `JAPClient` (Frontend) and `JAPServer` (Backend).

### 4.2 The JIP Transport (two-layer design)

**Layer 1 — Transport** (`PipesServer`, `PipesClient`, `PipesProtocol`):
- Understands bytes only. Length-prefix framing: `int32` (LE) + raw bytes.
- Two pipes per session: `req-pipe-<sessionKey>` and `res-pipe-<sessionKey>`.
- Magic tokens: `JOSYN-IPC`, `JOSYN-IPC-ERROR`, `JOSYN-IPC-ERROR-BUSY`, `JOSYN-IPC-SHUTDOWN`.
- CLI convention: server passes `"JOSYN-IPC <sessionKey>"` as args to the client exe.

**Layer 2 — Convention** (`JipServer`, `JipClient`, `JipProtocol`, `JipDispatcher`):
- JSON-based: `Request { What, Data? }` / `Response { Succeeded, Data?, Error? }`.
- Handler: `Func<Request, Task<Result<string?>>>`.

**Known PoC limitations (documented; do NOT fix unless asked):**
- Single-in-flight: no request multiplexing; strictly sequential.
- `ClientPipes`/`ServerPipes` typed as `record` — should be `sealed class`.

### 4.3 How Frontend Dispatches a Job

```
Job.exe  →  Core.Run(args)
              ├── JAPClient.CreateConnectedClient(args)   parse sessionKey + JIP connect
              └── JobInvoker.InvokeJob(japClient)
                   ├── FindEntryPointAssembly             Assembly.GetEntryAssembly()
                   ├── FindJobFunction                    [JobEntryPoint] via reflection
                   ├── CreateInvocationArguments          GetRawArguments() → PropertyBag.Deserialize
                   ├── [User's job method]                pure business logic
                   └── ProcessJobResult                   PropertyBag.Serialize → PutRawResult()
```

Reflection in `JobInvoker` is **intentional design** — it is the extension point for job
authors. Do not flag it as a violation of the "no hidden magic" principle.

### 4.4 The Namespace Grouping Pattern

Grouping layers (`JOSYN.System.Frontend`, `JOSYN.System.Backend`) are pure folders —
no code, just `.slnx` + scaffold. Concrete projects live one level deeper with the
fully-qualified name. This is consistent across the whole repo:
- `JOSYN.Foundation/JOSYN.Foundation.JIP/JOSYN.Foundation.JIP/`
- `JOSYN.System/JOSYN.System.Frontend/JOSYN.System.Frontend.JobHost/`
- `JOSYN.System/JOSYN.System.Backend/JOSYN.System.Backend.JAPServer/`

### 4.5 Backend is an Exe, Not a Library

`JOSYN.System.Backend.JAPServer` has `OutputType=Exe`. It is never packed as NuGet.
It depends on `JOSYN.Foundation.JIP` + `JOSYN.System.Contract`. It has hardcoded
demo data (`FakeReadArgumentsFromFile`). Graceful shutdown via ESC key (`WasEscapePressed`).

---

## 5. Build System Reference

### Root `.local-build/` scripts

| Script | Description |
|---|---|
| `build-all.cmd` | Clean → build → pack all 6 in dependency order (stops on first error) |
| `test-all.cmd` | `dotnet test` all 3 Foundation solutions (161 total) |
| `all.cmd` | `build-all.cmd` + `test-all.cmd` |
| `demo.cmd` | Start JAPServer (new window) → wait 1s → start MyDemoJob (new window), Release builds |
| `demo.debug.cmd` | Build both in Debug first → launch both, activates `#if DEBUG` Console.ReadKey pauses |

### Sub-repo `.local-build/` scripts (each self-contained sub-repo)

| Script | Description |
|---|---|
| `build.cmd [Debug\|Release]` | Build (Release default) |
| `build.debug.cmd` | Debug shortcut |
| `build.release.cmd` | Release shortcut |
| `test.cmd` | `dotnet test` |
| `pack.cmd` | Pack → `..\..\Local Packages\` |

### Crystal-clean build flow (build-all.cmd)

1. `del /q "Local Packages\*.nupkg"` — empty the feed
2. `cleanup-nuget-cache.cmd NOPAUSE` — clear global NuGet cache for all JOSYN packages
3. Build + pack each in dependency order (pack immediately after each build)

### Demo session key

Both `JAPServer` and `MyDemoJob` are hardcoded to:
`dea5611d-d740-437f-ad93-7a5dc5ae4299`

Both `.cmd` launchers use this key. It is in `launchSettings.json` of both projects.

---

## 6. Self-Contained Sub-Repo Scaffold (Template)

```
JOSYN.System/
└── JOSYN.System.Frontend/          ← sub-repo root
    ├── .local-build/               ← build/test/pack scripts
    ├── Directory.Build.props       ← BuildRoot = C:\Temp\VS.OUT\JOSYN\
    ├── nuget.config                ← local feed: "..\..\Local Packages"
    ├── JOSYN.System.Frontend.slnx
    └── JOSYN.System.Frontend.JobHost/   ← project folder (fully-qualified name)
        ├── JOSYN.System.Frontend.JobHost.csproj
        ├── README.md
        ├── Contracts/              ← static abstract interfaces (API docs)
        └── [source files]
```

- Solution format: `.slnx` (not `.sln`)
- Build output: `C:\Temp\VS.OUT\JOSYN\<ProjectName>\bin\<Config>\net10.0\`
- NuGet feed depth: 2 levels up from sub-repo root → JOSYN root

---

## 7. Code Conventions (Non-Negotiable)

| Topic | Rule |
|---|---|
| Default type | `static class` — instance only if justified (state, polymorphism) |
| Data types | `record` over `class`; `init`-only properties |
| Error handling | `Result`/`Result<T>` — never `throw` above catch boundary |
| Exception boundary | `catch (Exception ex) { return ex; }` — implicit conversion |
| Propagation | `Result.Propagate(inner)` — never re-wrap a failure manually |
| Interfaces | `static abstract` members in `Contracts/` folder; `/// <inheritdoc cref="IXxx"/>` on impls |
| Explicitness | No DI containers, no reflection wiring (except `JobInvoker` — intentional) |
| Error messages | **German** (project-wide convention: `"Verbindung getrennt."`, etc.) |
| Culture | `de-DE` default (numbers, dates) |
| XML docs | On interfaces/contracts; implementations use `<inheritdoc>` |
| Nullability | `Nullable` enabled everywhere; `?` is deliberate, never defensive |

---

## 8. Result Pattern Quick Reference

```csharp
// Success
return Result.Success;
return Result<T>.Success(value);   // or implicit: return value;

// Failure
return Result.Error("Fehlermeldung");

// Exception boundary (lowest layer only)
catch (Exception ex) { return ex; }

// Propagate up the chain
var inner = SomeOperation();
if (!inner.Succeeded) return Result.Propagate(inner);

// Consume
if (!result.Succeeded) { Console.Error.WriteLine(result.ErrorMessage); }
var value = result.Value; // only access after Succeeded == true
```

---

## 9. PropertyBag Quick Reference

```csharp
var r = PropertyBag.Serialize(myRecord, IniDictionarySerializer.Serialize);
var r = PropertyBag.Serialize(myRecord, JsonDictionarySerializer.Serialize);
var r = PropertyBag.Deserialize<MyRecord>(rawString); // auto-detects INI vs JSON
```

- **Records only** — checks for `<Clone>$` method
- **`de-DE` culture** — number/date formatting
- **Auto-detection** — JSON if input starts with `{`, INI otherwise
- Supported types in `SupportedPropertyTypes.cs`

---

## 10. What Remains (If Anything)

The PoC is **functionally complete and runnable**. Possible next steps:

1. **Repo-wide README.md** — update to reflect final structure (currently stale).
2. **Tagged release commit** — `git tag v0.1.0-poc` to mark the freeze point.
3. **Phase 2 planning** — production concerns: async JIP handler, multi-job scheduling,
   real argument source (not fake), proper process management in Backend.
4. **Multi-repo split** — each `JOSYN.Foundation.*` and `JOSYN.System.*` sub-repo
   becomes its own actual git repo (the grouping pattern was designed for this).

---

*This document was written after poc/session-0012.*  
*It supersedes session-0010 (Ariadne's Thread v3).*  
*PoC is functionally complete. All 6 sub-repos built, 5 NuGet packages packed, demo runnable.*
