# IPC Discussion — JOSYN.Core.IPC (PoC Review)

> Scope: `JOSYN.Core.IPC` including its dependency on `JOSYN.Core.ResultPattern`.  
> This is an early-stage PoC analysis. No code was changed.

---

## What's Solid

### Two-pipe design (request + response)
Clean directional separation. Each pipe has an explicit `PipeDirection` (`In` / `Out`), making the data flow obvious and avoiding half-duplex complexity. The server owns both `NamedPipeServerStream` instances; the client owns both `NamedPipeClientStream` instances.

### Length-prefix framing over byte-mode pipes
The right call. `PipeTransmissionMode.Message` has practical size limits (~64 KB reliable on some Windows versions); manual `int32 + bytes` framing on `PipeTransmissionMode.Byte` is robust and portable.

### Dual cancellation model
Offering both `Func<bool>?` (polling predicate, e.g. for an ESC-key check) and `CancellationToken` is pragmatic. The polling adapter (`CreatePollingCancellationToken`) converts the predicate into a `CancellationToken` internally so all real async work stays token-based. Good design.

### Retry with exponential backoff in `ConnectAsync`
5 attempts, starting at 300 ms, 1.5× multiplier capped at 2 000 ms. Correctly handles the client racing the server's `WaitForConnectionAsync`. This is one of the trickier parts of named-pipe IPC done right.

### Result-pattern discipline
No exceptions leak up the call stack. `catch (Exception ex) { return ex; }` is used consistently. `Result.Propagate()` is applied wherever a failure from a called method bubbles up — the call chain is preserved. This is exactly what the pattern is for.

### Session-key isolation
Each session gets unique pipe names derived from the key (`req-pipe-<key>`, `res-pipe-<key>`). Multiple concurrent sessions on the same machine won't collide.

### Static abstract interface pattern
Using C# 11 `static abstract` members on `IPipesServer`, `IPipesClient`, `IPipesProtocol` as documentation/contract anchors for purely static classes is a deliberate, modern choice. It won't enable polymorphic dispatch without a type constraint, but as API documentation it is valid and readable.

### Demo projects
Having `Demo.ServerExe` and `Demo.ClientExe` in the same solution is very helpful for a PoC. Both scenarios (server-starts-client and server-waits-for-external-client) are covered.

---

## Issues

### 🔴 HIGH — Synchronous request handler will block the pipe loop

```csharp
// PipesServer.cs — RequestLoopAsync
var response = processRequest(requestBytes);   // synchronous call, blocks the loop
```

The handler signature is `Func<byte[], byte[]>` (and `Func<string, string>`). In a JobSystem, handlers will perform async work — database calls, subprocess execution, file I/O. A blocking handler on this hot path stalls the entire request loop.

**Should be:** `Func<byte[], Task<byte[]>>` (and `Func<string, Task<string>>`).  
This change touches the public API surface and all overloads, so it is better addressed early.

---

### 🔴 HIGH — `record` for `ClientPipes` / `ServerPipes` is semantically wrong

```csharp
public record ClientPipes
{
    public required NamedPipeClientStream RequestPipe { get; init; }
    public required NamedPipeClientStream ResponsePipe { get; init; }
}
```

`record` implies value equality and supports `with`-expressions. But `NamedPipeClientStream` is a mutable OS resource — equality would compare stream references, not identity or state. A `with`-copy of a `ClientPipes` would alias the same underlying pipe handles, which is dangerous.

**Should be:** `sealed class`. There is no benefit to `record` here and real risk of confusing future readers or tooling.

---

### 🟡 MEDIUM — Inconsistent framing: `BinaryWriter` on one side, `BitConverter` on the other

- **Server writing a response** → uses `BinaryWriter.Write(response.Length)` + `BinaryWriter.Write(response)`
- **Client writing a request** → uses `BitConverter.GetBytes(requestBytes.Length)` + raw `WriteAsync`
- **Server reading a request** → uses raw `ReadExactlyAsync` + `BitConverter.ToInt32`
- **Client reading a response** → uses raw `ReadExactlyAsync` + `BitConverter.ToInt32`

Both approaches happen to be little-endian on .NET/x86, so it currently works. But the inconsistency will cause confusion when the protocol needs to change. A uniform approach — e.g. `BinaryWriter`/`BinaryReader` pairs on both sides — would be cleaner and self-documenting.

---

### 🟡 MEDIUM — `DisconnectAsync` is fake-async and is missing a flush

```csharp
public static async Task<Result> DisconnectAsync(ClientPipes pipes)
{
    pipes.RequestPipe.Close();          // synchronous
    pipes.ResponsePipe.Close();         // synchronous
    return await Task.FromResult(Result.Success);   // pointless await
}
```

Two problems:
1. No `FlushAsync` before closing the request pipe. The server may be mid-read when the client closes, potentially leaving it in an undefined state rather than a clean `EndOfStreamException`.
2. `return await Task.FromResult(...)` is the classic fake-async antipattern. The method should either be `Task`-returning with a direct `return Result.Success` or (if close ever becomes genuinely async) use `await DisposeAsync()`.

---

### 🟡 MEDIUM — `CancellationTokenSource` is never disposed in the polling path

```csharp
// CreatePollingCancellationToken
var cts = new CancellationTokenSource();
// ...
return (() => { cancel?.Invoke(); }, cts.Token);
// cts.Dispose() is never called
```

The returned dispose action calls `cts.Cancel()` but never `cts.Dispose()`. `CancellationTokenSource` holds a `WaitHandle` and a timer (if constructed with a delay); not disposing it is a resource leak. The returned action should call both `Cancel()` and `Dispose()`.

---

### 🟡 MEDIUM — `await Task.FromResult(...)` antipattern in `CreatePipesAsync`

```csharp
return await Task.FromResult(new ServerPipes { ... });
```

This method is declared with an async signature but does not perform any async work. It allocates a `Task` wrapper for no reason. Should return `Result<ServerPipes>` directly (non-async) or `ValueTask<Result<ServerPipes>>` if the async shape must be preserved.

---

### 🟢 LOW — No request correlation / single-in-flight constraint is implicit

The protocol is strictly sequential: one request must complete before the next can be sent. There is no request ID in the frame header, no timeout per request, and no way to detect a lost response versus a slow handler.

This is acceptable for the PoC and for a controlled server↔runner boundary. However it should be **documented explicitly** — especially `maxNumberOfServerInstances: 1` — so that any future evolution (concurrent jobs, multiplexing) knows what it needs to change. The constraint is invisible today.

---

### 🟢 LOW — `ParseServerCLIArguments` 3-argument form is unused and undocumented

```csharp
// Accepts: JOSYN-IPC <sessionKey>              (2 args)
//      or: JOSYN-IPC <clientExePath> <sessionKey>   (3 args)
return args.Length == 2 ? (args[1], string.Empty) : (args[2], args[1]);
```

`CreateClientStartCLIArguments` only produces the 2-argument form. The 3-argument form is parsed but never generated anywhere in this codebase. If it belongs to a future server-to-server scenario, it should be a named factory method and documented. If it is dead code, remove it.

---

## Summary Table

| # | Priority | Issue |
|---|----------|-------|
| 1 | 🔴 High | Async handler — `Func<byte[], byte[]>` must become `Func<byte[], Task<byte[]>>` |
| 2 | 🔴 High | `record` → `sealed class` for `ClientPipes` / `ServerPipes` |
| 3 | 🟡 Medium | `BinaryWriter` vs `BitConverter` framing inconsistency |
| 4 | 🟡 Medium | `DisconnectAsync` — missing flush, fake async |
| 5 | 🟡 Medium | `CancellationTokenSource` never disposed in polling path |
| 6 | 🟡 Medium | `await Task.FromResult(...)` antipattern in `CreatePipesAsync` |
| 7 | 🟢 Low | Single-in-flight constraint is implicit — should be documented |
| 8 | 🟢 Low | 3-argument CLI parse branch is unused — document or remove |

---

## Overall Assessment

The **architecture** is sound for the PoC purpose: session-key-isolated sessions, two-pipe directional design, proper byte-mode framing, retry logic, and Result-pattern discipline throughout. There is nothing structurally wrong that would require a redesign.

The high-priority items (#1 and #2) should be fixed **before building on top of this**, because both affect the public API surface. Everything else can wait for a post-PoC hardening pass.
