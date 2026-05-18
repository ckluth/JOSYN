# IPC Discussion — Issue #1 Fix Review (Async Request Handler)

> Scope: Branch `evolution/ipc-issue-blocking-requesthandler` — review of the attempted fix for Issue #1 from session-001.  
> No code was changed during this discussion.

---

## What Was Done

The branch upgrades every `Func<byte[], byte[]>` / `Func<string, string>` handler signature to its async counterpart throughout the entire public surface and the internal pipeline:

| Location | Change |
|---|---|
| `IPipesServer.cs` | All 5 overloads: `Func<…, T>` → `Func<…, Task<T>>` |
| `PipesServer.cs` | All 5 public overloads updated to match |
| `PipesServer.RawRequestHandler` | `ProcessStrings` field and `ProcessRawRequest` made async |
| `RequestLoopAsync` | `var response = processRequest(…)` → `var response = await processRequest(…)` |
| `Demo.ServerExe/Program.cs` | `HandleRequest` returns `Task<string>` |

---

## What Is Correct

### Mechanically complete and consistent
Every overload was updated — no stale sync signature was left behind. The interface (`IPipesServer`) and the implementation (`PipesServer`) are in perfect sync. This is the most important property for an API-surface change: it must be all-or-nothing.

### `RawRequestHandler.ProcessRawRequest` properly propagates async
The internal adapter that bridges `Func<string, Task<string>>` to `Func<byte[], Task<byte[]>>` was correctly made `async Task<byte[]>` with a proper `await`. If it had been left as a sync wrapper calling `.Result` or `.GetAwaiter().GetResult()` on the inner task, the original blocking problem would simply have been moved one level deeper. It wasn't.

### The hot path is genuinely non-blocking now
The critical line in `RequestLoopAsync`:
```csharp
var response = await processRequest(requestBytes);
```
This is the goal. The loop now yields the thread to the runtime while the handler does its async work.

### Demo server updated correctly
`HandleRequest` returning `Task.FromResult(responseStr)` is the right idiom for a demo handler that is inherently synchronous — it satisfies the new signature without introducing unnecessary overhead.

---

## Issues and Observations

### 🔴 Issue #2 (record → sealed class) is still open
`ClientPipes` and `ServerPipes` remain `record` types holding mutable OS stream handles. Session-001 rated this 🔴 HIGH alongside Issue #1, with the explicit note that both should be fixed *before building on top of this*. The branch name scopes the change to issue #1, which is intentional, but the second red-flag item needs its own follow-up branch before this code is merged and used as a foundation.

### 🟡 Response writes are still synchronous after an async handler
In `RequestLoopAsync`, after `await processRequest(requestBytes)` completes, the response is written synchronously:
```csharp
writer.Write(response.Length);   // sync
writer.Write(response);          // sync
```
`BinaryWriter` has no `WriteAsync` overloads. For a named pipe created with `PipeOptions.Asynchronous`, writes go to the kernel buffer immediately and are rarely a practical bottleneck in the current PoC. However, this is a natural loose end that the async refactor exposes. If the write side ever needs to be fully non-blocking (e.g., large responses, slow readers), the `BinaryWriter` wrapper would need to be replaced with direct `stream.WriteAsync` calls — the same pattern already used on the read side. Worth keeping in mind but not urgent at this stage.

### 🟢 Spurious whitespace change in `Demo.ClientExe/Program.cs`
```diff
-    return LogError("Es wurde kein Pipes-SessionKey übergeben", 1);
+    return LogError("Es wurde kein Pipes-SessionKey übergeben ", 1);
```
A trailing space was added to the German error message. This is noise — unrelated to the fix and introduces a barely-visible inconsistency. Should be reverted before the PR is merged.

### 🟢 Remaining session-001 issues are intentionally out of scope
Issues #3–#8 from session-001 are not touched by this branch. That is the right decision — keeping the diff focused makes review and potential rollback straightforward. The open items are:

| # | Priority | Status |
|---|----------|--------|
| 2 | 🔴 High | `record` → `sealed class` — **still open, must precede any consumer** |
| 3 | 🟡 Medium | `BinaryWriter` vs `BitConverter` framing inconsistency — open |
| 4 | 🟡 Medium | `DisconnectAsync` — missing flush, fake async — open |
| 5 | 🟡 Medium | `CancellationTokenSource` never disposed in polling path — open |
| 6 | 🟡 Medium | `await Task.FromResult(…)` in `CreatePipesAsync` — open |
| 7 | 🟢 Low | Single-in-flight constraint undocumented — open |
| 8 | 🟢 Low | 3-argument CLI parse branch unused — open |

---

## Overall Assessment

The fix is **correct and complete for its stated scope**. Issue #1 — blocking handler on the pipe loop — is genuinely resolved. The approach is clean: `Task<>` was threaded all the way through every overload and the internal adapter, with no shortcuts like `.Result` or fire-and-forget wrapping. The interface and implementation are consistent.

**Before merging:** remove the trailing-space change in `Demo.ClientExe/Program.cs`.  
**Before building on top of this:** address Issue #2 (`record` → `sealed class` for `ClientPipes` / `ServerPipes`).  
**Everything else** can proceed in a post-PoC hardening pass as described in session-001.
