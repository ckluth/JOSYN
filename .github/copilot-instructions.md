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

**Known PoC limitations** (see `.github\session-results\ipc\session-0003-poc-assessment-conclusion.md` for full analysis):
- Request handler is currently synchronous (`Func<byte[], byte[]>`) — async handlers needed before building on top of this.
- Protocol is single-in-flight (strictly sequential, no request IDs).
- `ClientPipes` / `ServerPipes` are typed as `record` but should be `sealed class`.

## Key Conventions

- **Static entry points** — `PipesServer`, `PipesClient`, `PipesProtocol`, and `PropertyBag` are all static classes. Interfaces (`IPipesServer`, etc.) exist as API-contract documentation using C# 11 `static abstract` members.
- **Namespace pragma** — files whose folder path doesn't match their namespace use `#pragma warning disable/restore IDE0130` around the `namespace` declaration.
- **Local NuGet feed** — inter-repo dependencies are resolved via `..\..\Local Packages\` (each `nuget.config` points here). Pack a dependency before referencing it from another logical repo.
- **Error messages are in German** — maintain this for consistency (`"Verbindung durch Aufrufer abgebrochen."`, `"kein Callstack"`, etc.).
- **Session results** are stored under `.github\session-results\` using a two-level structure: **story directory** → **session files**.

  **Story directory** = a named folder for a subject area, e.g. `result-pattern\` or `ipc\`. Session files accumulate here with no setup overhead — just start writing them.

  **Session file naming:** `session-NNNN-[short-description]-[type].md`
  - `NNNN` — zero-padded **4-digit** index, continuous per story (never resets after archiving)
  - `short-description` — 2–4 word kebab-case hint of the content
  - `type` — one of: `discussion` | `summary` | `conclusion` | `analysis` | `generation` | `opener`
  - The directory already carries story context — the filename must **not** repeat it.
  - Examples: `session-0001-make-or-buy-summary.md`, `session-0002-async-handler-analysis.md`
  - Each session appends a new file — never overwrites an old one.

  **Story index (`_index.md`):** each story folder has a `_index.md` maintained entirely by the AI.
  - **Read it first** at the start of any session in that story — it gives full context without opening individual session files
  - **Create it** on the first save in a story that doesn't have one yet
  - **Update it silently** on every subsequent save — no separate instruction from the user needed
  - Contains three sections:
    - `Key Decisions` — firm conclusions that future sessions must not contradict without knowing about them
    - `Open Questions` — unresolved threads a future session might pick up
    - `Sessions` — one-line-per-session table (sequence number, filename, one-sentence summary); archived sessions are listed with `[archived]` tag
  - `_index.md` is **never archived** — it stays in the story root and carries forward across chapters
  - `_index.md` has no `session-NNNN` prefix and is not a session file

  **Session opener:** an optional structured prompt the user places in the story folder before a session starts, to kick off a focused and prepared session:
  - Named `session-NNNN-opener[-short-description].md` (type is `opener`; short-description is optional)
  - The user is responsible for placing it in the correct story folder with the correct name
  - At session start, the AI reads it first, paraphrases it briefly, and asks for clarification if anything is unclear, then begins working
  - Openers are purely optional; sessions without them work exactly as before
  - The session result file produced from an opener is numbered **NNNN** (the opener's own number — the user is responsible for a correctly incremented session number); never re-derive the next number from the file listing when an opener is present

  **Archiving:** when the user says *"archive the current chapter"* (optionally *"as \<name\>"*):
  1. Move all session files currently in the story root into `archives\archive-NNN[-optional-name]\` (3-digit archive counter, optional suffix).
  2. Session numbering in the story **continues from where it left off** — no reset.
  3. If there are 3 or more sessions in the batch, offer (do not require): *"Want a brief conclusion file in the archive?"* — if yes, negotiate content on the fly, no fixed structure.
  4. Archives are sealed after creation — never modified.

  **Directory layout example:**
  ```
  .github\session-results\
    result-pattern\
      session-0004-new-story-discussion.md   ← active, continues after archive
      archives\
        archive-001-first-iteration\          ← sealed
          session-0001-make-or-buy-summary.md
          session-0002-...md
          session-0003-...md
          conclusion.md                       ← optional, only if requested
  ```

- **Session save trigger** — whenever the user says *"save this session"*, *"write a summary"*, *"log this"*, or similar: always propose a filename following the pattern above and ask for confirmation before writing. Example: *"Shall I save this as `.github\session-results\result-pattern\session-0001-make-or-buy-summary.md`?"*
