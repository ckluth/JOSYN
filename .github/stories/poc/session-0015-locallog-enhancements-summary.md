# Session 0015 — LocalLog Enhancements

## Context

Fine-tuning session on `JOSYN.System.Shared.Log` (`LocalLog`). Triggered by observing that
`WriteToConsole()` was silently absent when running `MyDemoJob` in Visual Studio DEBUG while the
JAPServer was spawned manually outside VS.

---

## Problem Diagnosed

`#if DEBUG` in a NuGet library is evaluated at **pack time** (Release build), not at consumer
compile time. The `WriteToConsole()` calls were compiled out of the DLL before it was ever used.

---

## Changes Made

### 1. `LocalLog.cs` — Console output controlled by runtime flag

- Removed `#if DEBUG` guards around `WriteToConsole()` calls.
- Added `public static bool EnableConsoleOutput { get; set; } = false;`.
- All `WriteToFile` calls now pass `directory` as an explicit parameter (refactored for sender
  subfolder support, see below).

### 2. `Host.cs` (JAPServer) + `Core.cs` (JobHost) — Activate flag in consumer's own DEBUG block

```csharp
#if DEBUG
    LocalLog.EnableConsoleOutput = true;
#endif
```

These `#if DEBUG` guards compile correctly because they live in the consumer's own exe projects,
not in the shared NuGet DLL.

### 3. `LocalLog.cs` — Entry-assembly name instead of `Process.GetCurrentProcess().ProcessName`

```csharp
public static readonly string ProcessName =
    Path.GetFileNameWithoutExtension(Assembly.GetEntryAssembly()?.Location ?? "unknown");
```

`ProcessName` is exposed publicly so callers can reference it without repeating the expression.

### 4. `LocalLog.cs` — Sender subfolder overloads

New overloads for `Info` and `Error` accept a leading `sender` string. If provided, log entries
are written to `%TEMP%\JOSYN\<ProcessName>\<sender>\<date>.log` instead of the root process
folder. This allows the JAPServer to segregate log entries per calling job:

```csharp
LocalLog.Info(LocalLog.ProcessName, "message from this process as sender");
LocalLog.Error("MyDemoJob", result);   // in JAPServer: per-job subfolder
```

Resulting folder structure:
```
%TEMP%\JOSYN\
  JOSYN.System.Backend.JAPServer\
    2026-05-25.log              ← server's own entries
    JOSYN.MyDemoJob\
      2026-05-25.log            ← entries logged with sender = LocalLog.ProcessName of job
```

### 5. `.local-build/*.cmd` — Removed blocking `pause` calls

10 `pause` statements across `pack.cmd`, `test.cmd`, and `build.debug.cmd` files were `REM`'d
out so scripts can be called non-interactively (e.g. from `build-all.cmd` or Copilot CLI).

Files changed:
- `JOSYN.System.Shared\.local-build\pack.cmd`
- `JOSYN.System.Shared\.local-build\test.cmd`
- `JOSYN.System.Backend\.local-build\test.cmd`
- `JOSYN.System.Backend\.local-build\build.debug.cmd`
- `JOSYN.System.Contract\.local-build\test.cmd`
- `JOSYN.System.Contract\.local-build\pack.cmd`
- `JOSYN.System.Frontend\.local-build\test.cmd`
- `JOSYN.System.Frontend\.local-build\pack.cmd`
- `JOSYN.Foundation.JIP\.local-build\test.cmd`
- `JOSYN.Foundation.JIP\.local-build\pack.cmd`

`demo.cmd` / `demo.debug.cmd` pauses on error-exit paths were left intentionally.

---

## NuGet

`JOSYN.System.Shared.Log` re-packed (still `1.0.0-preview01`, NuGet cache was cleaned manually
before re-pack). Consumers (`JAPServer`, `JobHost`) rebuilt against the new DLL.
