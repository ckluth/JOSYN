# Session 0016 — csproj Standardization + LocalLog Finalization

## What Was Done

### LocalLog — final shape (continued from session-0015)

- **Log path changed**: from `%TEMP%\JOSYN\<ProcessName>` to `AppContext.BaseDirectory\logs`
  - Reason: AD technical users running in impersonated context have no local user profile → `Path.GetTempPath()` is unreliable.
  - Decision: exe-adjacent `logs\` folder. Logs stay near the failing job = natural error-research context.
  - `LogDirectory` is now a plain `public static string` (settable); default = `Path.Combine(AppContext.BaseDirectory, "logs")`.
  - `ProcessName` static property removed; `LogDirectory` is the single configurable root.

- **`sender` renamed to `causer`**: all overloads `Error(string causer, ...)` / `Info(string causer, ...)` write to `Path.Combine(LogDirectory, causer)`.

- **`Causer` field added to `ErrorReport` record** (Shared.Contract); it is the first parameter.

- **`JAPServer.PutError` wired up**: deserializes `ErrorReport` via `PropertyBag.Deserialize<ErrorReport>` (JSON auto-detected), calls `LocalLog.Error(report.Causer, ...)`.

- **JSON serialization in `JAPClient.PutError`**: `PropertyBag.Serialize(report, JsonDictionarySerializer.Serialize)` — required because `CallStack` and `ExceptionDetails` contain embedded newlines which INI format (default) truncates at the first `\n`. `PropertyBag.Deserialize` auto-detects JSON by leading `{`; no server-side change needed.

- **Dirty cast eliminated in `Core.cs`**: `ReportErrorToServer(IJosynApplicationProtocol client, ...)` → `ReportErrorToServer(JAPClient client, ...)` — concrete type, no cast, no TODO.

- **`PutError` return value fixed**: `return !put.Succeeded ? Result.Propagate(put) : Result.Success` (was returning `put` — a `Result<string>` — on the success path).

- **`JOSYN.System.Contract` folder removed**: superseded by `JOSYN.System.Shared.Contract`; `git rm` workaround: delete on disk first, then `git add -A`.

### csproj Standardization — all 13 projects

Three canonical templates established and documented in `copilot-instructions/C# Project Files/AGENT.md`:

| Template | Projects |
|---|---|
| NuGet Library | ResultPattern, PropertyBag, JIP, Shared.Contract, Shared.Log, JobHost |
| Exe | JAPServer, MyDemoJob, JIP.Demo.ServerExe, JIP.Demo.ClientExe |
| Test | ResultPattern.Test, PropertyBag.Test, JIP.Demo.ServerExe.Test |

**Changes applied across all 13 files:**
- Single `<PropertyGroup>` (no more split groups)
- Consistent tab indentation (Notepad++ XML Pretty Print standard)
- `LangVersion`, `PlatformTarget`, `IncludeSourceRevisionInInformationalVersion`, `AppendTargetFrameworkToOutputPath`, `AssemblyName`, `RootNamespace` — present everywhere
- NuGet libraries: full metadata, `GenerateDocumentationFile`, `PackageIcon`, `PackageReadmeFile`
- Exes and tests: no metadata, no doc generation
- `PackageReleaseNotes` omitted everywhere (private feed, nobody reads it)
- Fixed: `JOSYN3` → `JOSYN` in repo URLs (ResultPattern)
- Removed duplicate `PackageReadmeFile` entries (ResultPattern, PropertyBag, JIP)
- `icon.png` added to Shared.Contract, Shared.Log, JobHost (file copied + csproj wired)
- `MyDemoJob`: `AssemblyName=JOSYN.MyDemoJob`, `RootNamespace=MyDemoJob`
- `JIP.Demo.ClientExe`: `RootNamespace=JOSYN.Foundation.JIP.Demo`

All 6 solutions built clean after changes. Committed and pushed to `poc/evolution`.

## Key Technical Notes

- **`#if DEBUG` in NuGet libraries** is evaluated at pack time (Release), not at consumer build time. Use runtime flags instead.
- **INI truncates multi-line strings**: any record with `\n` in field values must use `JsonDictionarySerializer.Serialize`; `PropertyBag.Deserialize` auto-detects format.
- **`AppContext.BaseDirectory`** is more reliable than `Assembly.GetEntryAssembly()?.Location` directory for finding the exe folder.
- **`git rm -r` interactive prompt**: when untracked files exist in a directory, git asks "try again?". Workaround: `Remove-Item -Recurse -Force` first, then `git add -A`.

## Files Modified

- `LocalLog.cs` — `LogDirectory` (public static, AppContext.BaseDirectory-based), causer overloads, `EnableConsoleOutput`
- `ErrorReport.cs` — `Causer` as first parameter
- `JAPServer.cs` — `PutError` with full deserialization and `LocalLog.Error(report.Causer, ...)`
- `JAPClient.cs` — JSON serialization for ErrorReport; `Result.Success` on success path
- `Core.cs` — `ReportErrorToServer(JAPClient client, ...)` concrete type
- `JOSYN.System.Backend.JAPServer.csproj` — `JOSYN.Foundation.PropertyBag` reference added
- All 13 `.csproj` files — standardized per templates
- `copilot-instructions/C# Project Files/AGENT.md` — created with 3 templates + decision log
- `icon.png` — copied to Shared.Contract, Shared.Log, JobHost
