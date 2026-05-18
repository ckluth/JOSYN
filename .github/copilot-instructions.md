# JOSYN – Copilot Instructions

JOSYN ("JobSystem Next") is a **physical multi-repo monorepo** in its genesis/PoC phase. All code targets **.NET 10**, C# `latest`, `Nullable` enabled throughout.

## Repository Layout

```
JOSYN/
├── JOSYN.Core/
│   ├── JOSYN.Core.ResultPattern/   ← foundation; referenced by everything
│   ├── JOSYN.Core.PropertyBag/     ← record serializer; depends on ResultPattern
│   └── JOSYN.Core.IPC/             ← named-pipe IPC; depends on ResultPattern
├── JOSYN.JobRunner/                ← placeholder
└── JOSYN.System/
    └── JOSYN.SessionServer/        ← placeholder
```

Each logical repo under `JOSYN.Core/` is self-contained with its own `.slnx` solution, `nuget.config`, and `zzz.*` scripts.

## Build, Test & Pack

Run from the **logical repo root** (e.g. `JOSYN.Core\JOSYN.Core.ResultPattern\`):

| Task | Command |
|------|---------|
| Build (Release) | `zzz.build.cmd` or `zzz.build.cmd Release` |
| Build (Debug) | `zzz.build.cmd Debug` |
| Run all tests | `zzz.test.cmd` → `dotnet test` |
| Single test by name | `dotnet test --filter "TestName"` |
| Pack NuGet | `zzz.pack.cmd` → outputs to `..\..\Local Packages\` |
| Build all solutions | `zzz.build.all.cmd` from repo root |

Build outputs go to `C:\Temp\VS.OUT\JOSYN\<ProjectName>\` (set in `Directory.Build.props`).  
**Test framework:** NUnit 4.x — `[TestFixture]` / `[Test]`.  
**Solution format:** `.slnx` (not `.sln`).

## The Result Pattern — used everywhere

`JOSYN.Core.ResultPattern` is the single most important convention. **No exceptions are thrown up the call stack.** Every operation returns `Result` (void) or `Result<T>`.

```csharp
// Success
return Result.Success;
return Result<MyType>.Success(value);  // or implicit: return value;

// Failure
return Result.Fail("error message");
return Result.Error("message");        // creates Error struct, implicitly converts to Result

// Failure from exception — always in catch blocks
catch (Exception ex) { return ex; }   // implicit conversion, captures caller info

// Propagate up a call chain (appends CallerInfo to the call stack)
var inner = SomeOperation();
if (!inner.Succeeded) return Result.Propagate(inner);

// Consume
if (!result.Succeeded)
{
    Console.WriteLine(result.ErrorMessage);
    Console.WriteLine(result.CallStackAsString);
}
var value = result.Value;  // only access after Succeeded == true
```

Always use `Propagate()` instead of re-wrapping a failure — it accumulates the call chain.

## PropertyBag

Serializes/deserializes C# **`record`** types (not plain classes — checked via `<Clone>$` method) to/from `Dictionary<string, string>`, then to **sectionless INI** or **JSON** (auto-detected by checking if input starts with `{`).

```csharp
var result = PropertyBag.Serialize(myRecord, IniDictionarySerializer.Serialize);
var result = PropertyBag.Serialize(myRecord, JsonDictionarySerializer.Serialize);
var result = PropertyBag.Deserialize<MyRecord>(rawString);  // format auto-detected
```

- Default culture is **`de-DE`** — affects number/date formatting.
- Only types listed in `SupportedPropertyTypes.cs` are valid record property types.
- Property name matching is case-insensitive on the first character when deserializing parameters.

## IPC (Named Pipes)

Session-isolated named-pipe communication between processes. Each session uses **two pipes**: one for requests (`req-pipe-<key>`), one for responses (`res-pipe-<key>`). Messages are **length-prefixed** (`int32` + bytes, little-endian).

```csharp
// Server — starts a client exe, then processes requests
await PipesServer.RunAsync(clientExePath, HandleRequest, connectTimeout, sessionKey, cancelPredicate);

// Server — waits for an externally-started client
await PipesServer.RunAsync(HandleRequest, connectTimeout, sessionKey, cancelPredicate);

// Client
var pipes = (await PipesClient.ConnectAsync(sessionKey)).Value;
var response = (await PipesClient.SendRequestAsync("request", pipes)).Value;
await PipesClient.DisconnectAsync(pipes);

// CLI protocol: server passes session key to client process as:
// "JOSYN-IPC <sessionKey>"
```

The `shouldCancel: Func<bool>?` parameter is converted internally to a polling `CancellationToken`. Callers pass a simple predicate; no `CancellationToken` management required.

**Known PoC limitations** (see `.github\discussions\ipc\ipc-discussion-session-001.md` for full analysis):
- Request handler is currently synchronous (`Func<byte[], byte[]>`) — async handlers needed before building on top of this.
- Protocol is single-in-flight (strictly sequential, no request IDs).
- `ClientPipes` / `ServerPipes` are typed as `record` but should be `sealed class`.

## Key Conventions

- **Static entry points** — `PipesServer`, `PipesClient`, `PipesProtocol`, and `PropertyBag` are all static classes. Interfaces (`IPipesServer`, etc.) exist as API-contract documentation using C# 11 `static abstract` members.
- **Namespace pragma** — files whose folder path doesn't match their namespace use `#pragma warning disable/restore IDE0130` around the `namespace` declaration.
- **Local NuGet feed** — inter-repo dependencies are resolved via `..\..\Local Packages\` (each `nuget.config` points here). Pack a dependency before referencing it from another logical repo.
- **Error messages are in German** — maintain this for consistency (`"Verbindung durch Aufrufer abgebrochen."`, `"kein Callstack"`, etc.).
- **Discussion files** live under `.github\discussions\<topic>\` and are named `<topic>-discussion-session-NNN.md` (e.g. `ipc-discussion-session-001.md`). Each session that produces a discussion appends a new file — never overwrites an old one. This preserves the full discussion history as explicit files in addition to git history.
