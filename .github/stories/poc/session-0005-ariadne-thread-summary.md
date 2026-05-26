# JOSYN PoC — AI Continuity Document (Ariadne's Thread)

**Story:** poc  
**Session:** 0005  
**Type:** summary  
**Purpose:** Internal AI context document. Enables seamless continuation of the PoC story  
without re-gathering context. Read this FIRST when the user says "continue poc" or similar.

---

## 1. What Is JOSYN (One Paragraph)

JOSYN ("JobSystem Next") is a physical multi-repo monorepo in its genesis/PoC phase.  
It is a job execution system: a **JobHost** (frontend/client side) dispatches jobs via  
**JIP** (JOSYN Interprocess Protocol, named-pipe IPC) to a **JAPServer** (backend/server side).  
The protocol between them is **JAP** (JOSYN Application Protocol — currently  
`IJosynApplicationProtocol` with `GetRawArguments()` and `PutRawResult()`).  
All code is **.NET 10**, C# `latest`, `Nullable` enabled, **functional-first C#**  
(static classes by default, Result pattern everywhere, no exceptions up the call stack,  
errors as values via `Result` / `Result<T>` from `JOSYN.Foundation.ResultPattern`).

---

## 2. The PoC Goal

- Reach a **show-and-tell maturity** suitable for colleagues.
- Produce a **comprehensive final documentation** with all next steps clearly defined.
- Reach a **logically archivable / frozen state** for the entire meta-repo.
- Not throw-away code: **mature building-block implementations** with XML docs, README, NuGet packages.

---

## 3. The Four Layers + Current Status

### Layer 1 — JOSYN.Foundation ✅ Structurally Complete (Pack pending for JIP)

No dependencies on other JOSYN layers. Reusable outside JOSYN. Stable at 1.0.0 forever.

| Package | Folder | Namespace | NuGet Packed | Maturity |
|---|---|---|---|---|
| `JOSYN.Foundation.ResultPattern` | `JOSYN.Foundation/JOSYN.Foundation.ResultPattern/` | `JOSYN.Foundation.ResultPattern` | ✅ `1.0.0-preview01` | Tests: 113 green |
| `JOSYN.Foundation.PropertyBag` | `JOSYN.Foundation/JOSYN.Foundation.PropertyBag/` | `JOSYN.Foundation.PropertyBag` | ✅ `1.0.0-preview01` | Tests: 47 green |
| `JOSYN.Foundation.JIP` | `JOSYN.Foundation/JOSYN.Foundation.JIP/` | `JOSYN.Foundation.JIP` | ❌ **NOT YET PACKED** | Tests: 1 green (dispatcher test) |

**`JOSYN.Foundation.JIP` is the blocker** for the next layer — it must be packed before  
Contract/Frontend/Backend can reference it.

### Layer 2 — JOSYN.Contract ⏳ Pending (naming TBD)

- Current code lives in `JOSYN.JAP/JOSYN.JAP.Protocol/` — one file: `IJosynApplicationProtocol.cs`
- Namespace: `JOSYN.JAP` (via `#pragma warning disable IDE0130`)
- **The name `JOSYN.JAP` / `JOSYN.JAP.Protocol` is explicitly unsatisfactory** and must be  
  resolved before any implementation work starts on this layer.
- Content is essentially the right thing: `GetRawArguments()` and `PutRawResult(string)`.
- All XML doc is `// TODO` — not production-ready.
- Naming decision tracked as Open Question in poc story (since resolved: `JOSYN.System.Contract`  
  was proposed — see poc _index.md decision #5).

**Resolved naming (from poc _index.md Key Decision #5):**  
`JOSYN.System` is the accepted grouping layer.  
The sub-names are: `JOSYN.System.Frontend`, `JOSYN.System.Backend`, `JOSYN.System.Contract`.

> **So: `JOSYN.JAP.Protocol` → `JOSYN.System.Contract`** (rename pending, not yet done).

### Layer 3 — JOSYN.Frontend ⏳ Pending

- Current code lives in `JOSYN.JobHost/`
- Contains: `JOSYN.JobHost/` (the NuGet package) + `JOSYN.MyDemoJob/`
- `JOSYN.JobHost` is the central package for job development (decoupled from backend via JIP).
- References: Foundation packages + Contract package.
- Current state: **placeholder / demo quality**, not yet mature.

### Layer 4 — JOSYN.Backend ⏳ Pending

- Current code lives in `JOSYN.System/JOSYN.System.JAPServer/`
- `JOSYN.System.JapServer.Server` — largely a copy of the JIP demo server.
- References: Foundation packages + Contract package.
- Current state: **demo quality**, not yet mature.

---

## 4. Repository Map (Current Physical State)

```
JOSYN/
├── .github/
│   └── session-results/
│       ├── poc/           ← PoC story (this story): sessions 0001–0005
│       ├── ipc/           ← JIP story: sessions 0001–0007
│       ├── result-pattern/
│       └── property-bag/
├── JOSYN.Foundation/      ← ALL three Foundation packages live here
│   ├── JOSYN.Foundation.ResultPattern/   ✅ packed
│   ├── JOSYN.Foundation.PropertyBag/     ✅ packed
│   └── JOSYN.Foundation.JIP/             ❌ not yet packed
├── JOSYN.JAP/             ← JOSYN.System.Contract (rename pending)
│   └── JOSYN.JAP.Protocol/
│       └── IJosynApplicationProtocol.cs
├── JOSYN.JobHost/         ← JOSYN.System.Frontend (pending)
│   ├── JOSYN.JobHost/
│   └── JOSYN.MyDemoJob/
├── JOSYN.System/          ← JOSYN.System.Backend (pending)
│   └── JOSYN.System.JAPServer/
│       └── JOSYN.System.JapServer.Server/
├── Local Packages/        ← Local NuGet feed
│   ├── JOSYN.Foundation.ResultPattern.1.0.0-preview01.nupkg
│   └── JOSYN.Foundation.PropertyBag.1.0.0-preview01.nupkg
│   (JOSYN.Foundation.JIP missing here!)
└── README.md
```

**Note:** `JOSYN.Core/` **no longer exists** — deleted in ipc/session-0007.  
All three formerly-Core packages live under `JOSYN.Foundation/`.

---

## 5. Agreed Implementation Order (Bottom-Up)

| # | Item | Status |
|---|------|--------|
| 1 | `JOSYN.Foundation.ResultPattern` | ✅ Done + Packed |
| 2 | `JOSYN.Foundation.PropertyBag` | ✅ Done + Packed |
| 3 | `JOSYN.Foundation.JIP` | ✅ Renamed/migrated — **pack + XML docs still needed** |
| 4 | `JOSYN.System.Contract` (was `JOSYN.JAP.Protocol`) | ⏳ Rename + XML docs + pack |
| 5 | `JOSYN.System.Frontend` (was `JOSYN.JobHost`) | ⏳ Maturity work |
| 6 | `JOSYN.System.Backend` (was `JOSYN.System.JAPServer`) | ⏳ Maturity work |
| 7 | Final documentation + logical archive | ⏳ |

---

## 6. IMMEDIATE NEXT STEP

**When user says "continue" or "what's next" — this is the answer:**

### Step 3b — Finish `JOSYN.Foundation.JIP` to maturity

The physical rename (ipc/session-0007) is done. The package is **not yet at Foundation maturity**.  
The remaining work before moving to layer 4:

1. **XML documentation** — all public interfaces (`IPipesServer`, `IPipesClient`, `IPipesProtocol`,  
   `IServerStartArguments`, `IJipProtocol`, `IRequest`, `IResponse`) get full XML docs;  
   implementations get `/// <inheritdoc/>` (or `/// <inheritdoc cref="IXxx.Member"/>` for static  
   classes that can't formally implement the interface).
2. **README update** — the project `README.md` is placeholder-level; bring it to the standard  
   of `JOSYN.Foundation.ResultPattern` (purpose, artifact name, referenced by, references).
3. **NuGet pack** — run `.local-build\pack.cmd`, verify  
   `Local Packages\JOSYN.Foundation.JIP.1.0.0-preview01.nupkg` exists.
4. **Consumers re-resolve** — after pack, the consumers (`JOSYN.JobHost`, `JOSYN.System.JAPServer`)  
   that reference `PackageReference Include="JOSYN.Foundation.JIP"` will resolve correctly.

Then:

### Step 4 — `JOSYN.System.Contract`

- Rename `JOSYN.JAP/JOSYN.JAP.Protocol/` → `JOSYN.System.Contract/` (TBD exact structure)
- Namespace: `JOSYN.System.Contract` (or `JOSYN.System.JAP`? — needs brief decision)
- `IJosynApplicationProtocol` → proper XML docs, proper name?
- Pack as NuGet.

---

## 7. Known JIP Dirt Spots (Documented, NOT blocking PoC)

From `ipc/_index.md` Open Questions — these are KNOWN and ACCEPTED limitations:

- **Async request handler** — `Func<byte[], byte[]>` is sync; must go async before production use
- **Single-in-flight** — no request IDs, strictly sequential; not multi-plex capable
- **`ClientPipes`/`ServerPipes` as `record`** — semantically wrong, should be `sealed class`

Do NOT attempt to fix these unless the user explicitly asks. They are documented dirt spots.

---

## 8. Key Conventions (Non-Negotiable)

### Code Style
- **Static classes by default** — `static class` + `static` methods; instance only if justified
- **Immutability** — `record` over `class`; `readonly` and `init`-only properties
- **Result pattern everywhere** — `Result` / `Result<T>` from `JOSYN.Foundation.ResultPattern`;  
  never `throw` above the lowest layer; catch blocks use `return ex;`; propagate with `Result.Propagate(inner)`
- **Interfaces as contracts** — public static types get an `interface` with `static abstract` members  
  in a `Contracts/` folder; implementations use `/// <inheritdoc cref="IXxx.Member"/>` (static)  
  or `/// <inheritdoc/>` (non-static)
- **Explicit over magic** — no reflection-based wiring, no DI containers
- **Error messages in German** — this is the project convention
- **`de-DE` culture** — default thread culture; affects number/date formatting

### Build & Test
- `.local-build\build.cmd` — builds the solution (Release by default)
- `.local-build\test.cmd` — `dotnet test`
- `.local-build\pack.cmd` — packs to `..\..\Local Packages\`
- Solution format: `.slnx` (NOT `.sln`)
- Test framework: NUnit 4.x (`[TestFixture]` / `[Test]`)

### NuGet Feed
- `nuget.config` in each logical repo root points to `..\..\Local Packages\`
- After packing a dependency, the downstream project can restore it

### Namespace Pragma Pattern
Files whose folder path doesn't match their namespace use:
```csharp
#pragma warning disable IDE0130
namespace JOSYN.Whatever;
#pragma warning restore IDE0130
```

---

## 9. JIP Architecture (Quick Reference)

### Two-Pipe Design
- Request pipe: `req-pipe-<sessionKey>` (client → server)
- Response pipe: `res-pipe-<sessionKey>` (server → client)
- Session-isolated via `Guid`-based session keys

### Transport Layer (namespace: `JOSYN.Foundation.JIP`)
- `PipesServer` — static, manages server lifecycle, cancellation, reconnect loop
- `PipesClient` — static, exponential backoff connect, busy-guard
- `PipesProtocol` — static, protocol constants (`MagicToken`, `MagicErrorToken`, etc.)
- `ServerStartArguments` — record, configuration for server startup
- `ClientPipes` / `ServerPipes` — currently typed as `record` (known dirt spot)

### JIP Convention Layer (namespace: `JOSYN.Foundation.JIP`, same flat namespace)
- `JipClient` — static, single method: `SendAsync(pipes, what, data?)`
- `JipServer` — static, `WrapHandler(...)` for server-side boilerplate
- `JipProtocol` — static, implements `IJipProtocol` (parse, serialize)
- `JipDispatcher` — `sealed class`, fluent registration, `RegisterAll<TProtocol>` via reflection
- Wire types: `Request` (What + Data?), `Response` (Succeeded + Data? + Error?)

### CLI Session Key Convention
Server passes session key to externally-started client as: `"JOSYN-IPC <sessionKey>"`

### Canonical Handler Return Type
`Func<string, Task<Result<string?>>>` — `bool Succeeded` replaces old `ResponseStatus` enum.  
`null` value means "void success"; non-null means "success with payload".

---

## 10. The JOSYN.Foundation.PropertyBag Pattern (Quick Reference)

Serializes/deserializes C# `record` types to/from `Dictionary<string, string>`, then to  
sectionless INI or JSON (auto-detected by checking if input starts with `{`).

```csharp
var result = PropertyBag.Serialize(myRecord, IniDictionarySerializer.Serialize);
var result = PropertyBag.Deserialize<MyRecord>(rawString); // format auto-detected
```

- **Records only** — checks for `<Clone>$` method
- **`de-DE` culture** — number/date formatting
- **Supported types** in `SupportedPropertyTypes.cs`

---

## 11. What JIP Is NOT (to avoid wrong proposals)

- Not a general-purpose IPC library
- Not transport-agnostic (Named Pipes only, by design)
- Not exception-based (everything is `Result`)
- Not multi-threaded / multiplexed (single-in-flight, by design for session-isolated use)

---

## 12. Quick State Verification Commands

```powershell
# Verify Foundation state
Get-ChildItem "JOSYN.Foundation" -Directory  # should show 3 folders

# Verify Local Packages
Get-ChildItem "Local Packages" -Filter "*.nupkg"  # should show ResultPattern + PropertyBag (JIP missing!)

# Build JIP
cd JOSYN.Foundation\JOSYN.Foundation.JIP
.local-build\build.cmd

# Pack JIP
.local-build\pack.cmd
```

---

*This document was written after session ipc/0007 (JOSYN.Foundation.JIP migration complete)  
and poc/session-0004 (JOSYN.Foundation.PropertyBag rename complete).*  
*Next session target: Finish JOSYN.Foundation.JIP to full maturity → pack it.*
