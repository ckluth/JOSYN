# JOSYN PoC — Milestone Checkpoint & Ariadne's Thread v5

**Story:** poc  
**Session:** 0014  
**Type:** summary  
**Date:** 2026-05-25  
**Purpose:** Two-in-one document.
- **Part A** — Human milestone report: the full PoC as it stands today — concepts first.
- **Part B** — Ariadne's Thread v5: complete AI continuation context. Supersedes session-0012.
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

### Layer 2 — System.Shared

Two packages shared by both EXE processes. This layer replaced the former `JOSYN.System.Contract`
layer (renamed and extended in session 0013).

| Package | What it is |
|---|---|
| `JOSYN.System.Shared.Contract` | The JAP protocol contract: `IJosynApplicationProtocol` (3 methods) + `ErrorReport` record. Transport-agnostic. |
| `JOSYN.System.Shared.Log` | `LocalLog` — process-local file logger. Writes to `%TEMP%\JOSYN\<ProcessName>\<date>.log`. |

The `JOSYN.System.Shared` grouping layer follows the same structural rule as Frontend and Backend:
pure namespace container folder — concrete projects live one level deeper with fully-qualified names.

### Layer 3 — System (Frontend + Backend)

**Frontend = `JOSYN.System.Frontend.JobHost`** (a NuGet library)

The Job Developer's world. A job author:
1. References this package.
2. Marks one `public static` method with `[JobEntryPoint]`.
3. Calls `Core.Run(args)` from `Program.cs`.

The library handles everything else: parses the session key from CLI args, connects via JIP,
fetches arguments from the Backend (`GetRawArguments`), deserializes them via PropertyBag,
invokes the job method via reflection (`JobInvoker`), serializes the result, and sends it
back (`PutRawResult`). Error routing: pipe failure → `LocalLog` only; job failure →
`LocalLog` + `PutError` to server (with `LocalLog` fallback if PutError itself fails).

**Backend = `JOSYN.System.Backend.JAPServer`** (an exe, not a library)

The Backend is the process that bridges the two sides. It:
1. Starts. Listens on its named pipe.
2. Launches a job exe, passing `"JOSYN-IPC <sessionKey>"` as CLI args.
3. Handles the three JAP calls from the job (`GetRawArguments`, `PutRawResult`, `PutError`).
4. Shuts down cleanly (ESC key detected via `WasEscapePressed`, graceful drain).
5. All error logging via `LocalLog` — no more console-color hacks.

---

## The Namespace Grouping Pattern

All three `JOSYN.System.*` sub-layers are **pure namespace containers**:

```
JOSYN.System/
  JOSYN.System.Frontend/            ← grouping layer (folder + slnx + scaffold)
    JOSYN.System.Frontend.JobHost/  ← THE library
    JOSYN.MyDemoJob/                ← demo job exe
  JOSYN.System.Backend/             ← grouping layer
    JOSYN.System.Backend.JAPServer/ ← THE exe
  JOSYN.System.Shared/              ← grouping layer (NEW in session 0013)
    JOSYN.System.Shared.Contract/   ← JAP contract + ErrorReport
    JOSYN.System.Shared.Log/        ← LocalLog
```

---

## Package Status

| Layer | Package / Artefact | NuGet | Tests | Status |
|---|---|---|---|---|
| Foundation | `JOSYN.Foundation.ResultPattern` | ✅ | 113 ✅ | Done |
| Foundation | `JOSYN.Foundation.PropertyBag` | ✅ | 47 ✅ | Done |
| Foundation | `JOSYN.Foundation.JIP` | ✅ | 1 ✅ | Done |
| Shared | `JOSYN.System.Shared.Contract` | ✅ | — | Done |
| Shared | `JOSYN.System.Shared.Log` | ✅ | — | Done |
| System | `JOSYN.System.Frontend.JobHost` | ✅ | — | Done |
| System | `JOSYN.System.Backend.JAPServer` | exe | — | Done |
| Root | `.local-build/` build scripts | — | — | Done |
| Root | Demo launcher (Release + Debug) | — | — | Done |

**All sub-repos done. PoC is functionally complete and runnable.**

---

## What Is Not Yet Done (Logical Freeze)

- Repo-wide `README.md` update to reflect the final structure.
- "Logical freeze" commit — a clean tagged commit marking end-of-PoC.
- Remove or tombstone old `JOSYN.System.Contract/` folder (superseded by `Shared`).
- `FakeReadArgumentsFromFile` in `JAPServer` is intentionally fake (PoC scope).
- `LocalLog`: no rotation/cleanup yet (intentionally deferred post-freeze).

---

---

# PART B — ARIADNE'S THREAD v5

> *For the AI. Read this first when the user says "continue poc" or similar.*  
> *Supersedes session-0012 (Ariadne v4). Written after session-0013/0014.*

---

## 1. What Is JOSYN (One Paragraph)

JOSYN ("JobSystem Next") is a physical multi-repo monorepo in its genesis/PoC phase.
It is a **job execution system**: a job is a standalone exe whose single method (marked
`[JobEntryPoint]`) is dispatched by the **JobHost** (Frontend library). The Frontend
connects to a **JAPServer** (Backend exe) via **JIP** (JOSYN Interprocess Protocol,
named pipes). The application-level protocol between them is **JAP** (`IJosynApplicationProtocol`
in `JOSYN.System.Shared.Contract`): three calls — `GetRawArguments()`, `PutRawResult(string)`,
and `PutError(string)`. All code targets **.NET 10**, C# `latest`, `Nullable` enabled throughout.
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

## 3. Repository Map (Current Physical State)

```
JOSYN/
├── .github/
│   └── session-results/
│       └── poc/
│           ├── _index.md
│           ├── session-0013-shared-layer-discussion-summary.md
│           └── session-0014-milestone-ariadne-v5.md    ← THIS FILE
├── .local-build/                    ← ROOT build orchestration
│   ├── all.cmd                      ← build-all + test-all
│   ├── build-all.cmd                ← crystal-clean build + pack all 6 sub-repos in order
│   ├── test-all.cmd                 ← dotnet test all Foundation packages (161 tests)
│   ├── demo.cmd                     ← launch JAPServer + MyDemoJob [Release]
│   └── demo.debug.cmd               ← build Debug + launch both
├── JOSYN.Foundation/
│   ├── JOSYN.Foundation.ResultPattern/   ✅ NuGet 1.0.0-preview01 | 113 tests
│   ├── JOSYN.Foundation.PropertyBag/     ✅ NuGet 1.0.0-preview01 | 47 tests
│   └── JOSYN.Foundation.JIP/             ✅ NuGet 1.0.0-preview01 | 1 test
├── JOSYN.System/
│   ├── JOSYN.System.Contract/            ⚠️  SUPERSEDED — still on disk, no longer referenced
│   │                                          Delete or tombstone before final freeze
│   ├── JOSYN.System.Shared/              ← NEW grouping layer (session 0013)
│   │   ├── JOSYN.System.Shared.slnx
│   │   ├── JOSYN.System.Shared.Contract/ ✅ NuGet 1.0.0-preview01
│   │   │   ├── IJosynApplicationProtocol.cs  — 3 methods: GetRawArguments, PutRawResult, PutError
│   │   │   └── ErrorReport.cs                — record: Message, CallStack, ExceptionDetails, OccurredAt
│   │   └── JOSYN.System.Shared.Log/      ✅ NuGet 1.0.0-preview01
│   │       └── LocalLog.cs               — static; file logger to %TEMP%\JOSYN\<proc>\<date>.log
│   ├── JOSYN.System.Frontend/            ← namespace grouping layer
│   │   ├── JOSYN.System.Frontend.slnx
│   │   ├── JOSYN.System.Frontend.JobHost/  ✅ NuGet 1.0.0-preview01
│   │   │   ├── Core.cs                   — public entry: Core.Run(args); error routing
│   │   │   ├── JAPClient.cs              — internal; IJosynApplicationProtocol via JIP (3 methods)
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
│           ├── Program.cs                — entry point: Host.Run(args)
│           ├── Host.cs                   — internal static; server lifecycle + LocalLog
│           └── JAPServer.cs              — internal; IJosynApplicationProtocol impl (3 methods)
├── Local Packages/
│   ├── cleanup-nuget-cache.cmd
│   ├── JOSYN.Foundation.ResultPattern.1.0.0-preview01.nupkg
│   ├── JOSYN.Foundation.PropertyBag.1.0.0-preview01.nupkg
│   ├── JOSYN.Foundation.JIP.1.0.0-preview01.nupkg
│   ├── JOSYN.System.Shared.Contract.1.0.0-preview01.nupkg
│   ├── JOSYN.System.Shared.Log.1.0.0-preview01.nupkg
│   └── JOSYN.System.Frontend.JobHost.1.0.0-preview01.nupkg
└── README.md
```

**Tombstoned (removed from build order; old `JOSYN.System.Contract` folder still on disk):**
- `JOSYN.Core/` → replaced by `JOSYN.Foundation/`
- `JOSYN.JobHost/` → replaced by `JOSYN.System.Frontend/`
- `JOSYN.JAP/` → replaced by `JOSYN.System.Contract/` → replaced by `JOSYN.System.Shared/`
- `JOSYN.System.JAPServer/` → replaced by `JOSYN.System.Backend/`

---

## 4. Key Concepts — Read Before Touching Anything

### 4.1 The JAP Protocol (application layer)

```csharp
// JOSYN.System.Shared.Contract, namespace JOSYN.System.Shared.Contract
public interface IJosynApplicationProtocol
{
    Task<Result<string>> GetRawArguments();    // Frontend calls: give me the job args
    Task<Result>         PutRawResult(string); // Frontend calls: here is the job result
    Task<Result>         PutError(string);     // Frontend calls: job failed, here is the error
}

// ErrorReport serialized as INI via PropertyBag, then passed to PutError:
record ErrorReport(string Message, string? CallStack, string? ExceptionDetails, DateTimeOffset OccurredAt);
```

Transport-agnostic. Both sides implement it independently. The transport (JIP named pipes)
is wired in `JAPClient` (Frontend) and `JAPServer` (Backend).

### 4.2 Error Routing (Frontend `Core.cs`)

```
Pipe connection failed       → LocalLog.Error(result)                       exit -1
Job error, pipe still alive  → LocalLog.Error(result) + PutError to server  exit -2
PutError itself fails        → LocalLog.Error(fallback)                     still exit -2
```

`LocalLog` always runs first — it is the safety net. `PutError` is best-effort.

### 4.3 `LocalLog` (Shared.Log)

```csharp
LocalLog.Error("Nachricht", callStack: "...", exceptionDetails: "...");
LocalLog.Error(result);   // Result overload
LocalLog.Info("Nachricht");
// → %TEMP%\JOSYN\<ProcessName>\<yyyy-MM-dd>.log (flush-on-write, never throws)
// → + Console output in #if DEBUG
```

### 4.4 The JIP Transport (two-layer design)

**Layer 1 — Transport** (`PipesServer`, `PipesClient`, `PipesProtocol`):
- Understands bytes only. Length-prefix framing: `int32` (LE) + raw bytes.
- Two pipes per session: `req-pipe-<sessionKey>` and `res-pipe-<sessionKey>`.
- Magic tokens: `JOSYN-IPC`, `JOSYN-IPC-ERROR`, `JOSYN-IPC-ERROR-BUSY`, `JOSYN-IPC-SHUTDOWN`.
- CLI convention: server passes `"JOSYN-IPC <sessionKey>"` as args to the client exe.

**Layer 2 — Convention** (`JipServer`, `JipClient`, `JipProtocol`, `JipDispatcher`):
- JSON-based: `Request { What, Data? }` / `Response { Succeeded, Data?, Error? }`.

**Known PoC limitations (documented; do NOT fix unless asked):**
- Single-in-flight: no request multiplexing; strictly sequential.
- `ClientPipes`/`ServerPipes` typed as `record` — should be `sealed class`.

### 4.5 How Frontend Dispatches a Job

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

### 4.6 The Namespace Grouping Pattern

Grouping layers are pure folders — no code, just `.slnx` + scaffold. Three grouping layers:
- `JOSYN.System/JOSYN.System.Frontend/JOSYN.System.Frontend.JobHost/`
- `JOSYN.System/JOSYN.System.Backend/JOSYN.System.Backend.JAPServer/`
- `JOSYN.System/JOSYN.System.Shared/JOSYN.System.Shared.Contract/` + `.../JOSYN.System.Shared.Log/`

### 4.7 Backend is an Exe, Not a Library

`JOSYN.System.Backend.JAPServer` has `OutputType=Exe`. It is never packed as NuGet.
It has hardcoded demo data (`FakeReadArgumentsFromFile`). Graceful shutdown via ESC key.
All lifecycle logging via `LocalLog` (no more Console Helpers region).

---

## 5. Build System Reference

### Dependency Order (Critical for NuGet)

```
1. JOSYN.Foundation.ResultPattern  → pack
2. JOSYN.Foundation.PropertyBag    → pack  (depends on 1)
3. JOSYN.Foundation.JIP            → pack  (depends on 1)
4. JOSYN.System.Shared             → build → pack Contract + pack Log  (depends on 1)
5. JOSYN.System.Frontend.JobHost   → pack  (depends on 1,2,3,4)
6. JOSYN.System.Backend.JAPServer  → build only  (exe; depends on 1,3,4)
```

6 sub-repos, 7 NuGet packages total (Shared produces two).

### Root `.local-build/` scripts

| Script | Description |
|---|---|
| `build-all.cmd` | Crystal-clean: clear nupkg + clear NuGet cache → build+pack all 6 in order |
| `test-all.cmd` | `dotnet test` all 3 Foundation solutions (161 tests) |
| `all.cmd` | `build-all.cmd` + `test-all.cmd` |
| `demo.cmd` | Start JAPServer + MyDemoJob [Release] in separate console windows |
| `demo.debug.cmd` | Build Debug + launch both |

### Demo Session Key

Hardcoded in both `launchSettings.json`:  
`dea5611d-d740-437f-ad93-7a5dc5ae4299`

---

## 6. Code Conventions (Non-Negotiable)

| Topic | Rule |
|---|---|
| Default type | `static class` — instance only if justified (state, polymorphism) |
| Data types | `record` over `class`; `init`-only properties |
| Error handling | `Result`/`Result<T>` — never `throw` above catch boundary |
| Exception boundary | `catch (Exception ex) { return ex; }` — implicit conversion |
| Propagation | `Result.Propagate(inner)` — never re-wrap a failure manually |
| Interfaces | `static abstract` members in `Contracts/` folder; `/// <inheritdoc cref="IXxx"/>` on impls |
| Explicitness | No DI containers, no reflection wiring (except `JobInvoker` — intentional) |
| Error messages | **German** (project-wide convention) |
| Culture | `de-DE` default (numbers, dates) |
| XML docs | On interfaces/contracts; implementations use `<inheritdoc>` |
| Nullability | `Nullable` enabled everywhere; `?` is deliberate, never defensive |

---

## 7. Result Pattern Quick Reference

```csharp
return Result.Success;
return Result<T>.Success(value);       // or implicit: return value;
return Result.Error("Fehlermeldung");
catch (Exception ex) { return ex; }   // lowest layer only
var inner = SomeOperation();
if (!inner.Succeeded) return Result.Propagate(inner);
if (!result.Succeeded) { LocalLog.Error(result); }
var value = result.Value;              // only after Succeeded == true
```

---

## 8. PropertyBag Quick Reference

```csharp
var r = PropertyBag.Serialize(myRecord, IniDictionarySerializer.Serialize);
var r = PropertyBag.Serialize(myRecord, JsonDictionarySerializer.Serialize);
var r = PropertyBag.Deserialize<MyRecord>(rawString); // auto-detects INI vs JSON
```

- **Records only** — checks for `<Clone>$` method
- **`de-DE` culture** — number/date formatting
- **Auto-detection** — JSON if input starts with `{`, INI otherwise

---

## 9. What Remains (Logical Freeze)

1. **Remove `JOSYN.System.Contract/`** — superseded, still on disk. Tombstone or delete.
2. **Repo-wide `README.md`** — update to reflect `JOSYN.System.Shared` and final structure.
3. **Tagged release commit** — `git tag v0.1.0-poc` to mark the freeze point.
4. **Phase 2 planning** — production concerns: async JIP handler, multi-job scheduling,
   real argument source, proper process management.

---

*This document was written after poc/session-0013 and session-0014.*  
*It supersedes session-0012 (Ariadne's Thread v4).*  
*PoC is functionally complete. All sub-repos built, 7 NuGet packages packed, demo runnable.*
