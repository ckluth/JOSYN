# Story: ipc

## Key Decisions

- **Build custom IPC** — existing .NET solutions (gRPC, SignalR, named-pipe wrappers) don't fit the session-isolated, lightweight, no-dependency model required by JOSYN
- **Two-pipe design** — one pipe for requests (`req-pipe-<key>`), one for responses (`res-pipe-<key>`); direction separation avoids half-duplex complexity
- **Length-prefix framing** — `int32` (little-endian) + N bytes; `BinaryWriter`/`BinaryReader` on both sides; `Write(byte[], 0, n)` overload used explicitly to avoid double-length-prefix
- **`shouldCancel: Func<bool>?` predicate** — callers pass a simple predicate; converted internally to a polling `CancellationToken`; no `CancellationToken` management required from callers
- **JipClient / JipServer convenience layer** — built over the raw protocol to eliminate 4-step boilerplate per method call

## Open Questions

- **Async request handler** — current handler is `Func<byte[], byte[]>` (synchronous); must be made async before building anything meaningful on top of this layer
- **Single-in-flight protocol** — strictly sequential, no request IDs; multi-in-flight support would require protocol redesign
- **`ClientPipes` / `ServerPipes` as `record`** — should be `sealed class`; currently typed as record which is semantically wrong

## Sessions

| # | File | Summary |
|---|---|---|
| [archived] | archive-001-first-stable-poc/ipc-discussion-session-001.md | Early PoC review of JOSYN.Core.IPC; no code changed |
| [archived] | archive-001-first-stable-poc/ipc-discussion-session-002.md | Review of async request handler fix attempt (branch evolution/ipc-issue-blocking-requesthandler) |
| [archived] | archive-001-first-stable-poc/ipc-discussion-session-003.md | Issue status review + analysis of josyn-ipc-protocol-draft-01.md |
| [archived] | archive-001-first-stable-poc/ipc-discussion-session-004.md | Issue status review + code analysis |
| [archived] | archive-001-first-stable-poc/ipc-discussion-session-005.md | Issue status review + code analysis |
| [archived] | archive-001-first-stable-poc/ipc-discussion-session-006.md | PoC completion and overall assessment |
| [archived] | archive-002-stable-poc-v2/ipc-discussion-session-001.md | Stabilization analysis: improved error behavior, async cancellation, reconnection pattern (branch evolution/ipc-convention-layer) |
| [archived] | archive-002-stable-poc-v2/ipc-discussion-session-002.md | Applied all six findings from stable-poc-v2/session-001 |
| 0001 | session-0001-jip-client-server-discussion.md | Design and implementation of JipClient/JipServer convenience layer over JIP protocol |
| 0002 | session-0002-why-custom-ipc-discussion.md | Technical rationale for custom IPC: alternatives analysis and differentiating features |
| 0003 | session-0003-poc-assessment-conclusion.md | Status-quo documentation: architecture, two-pipe design, length-prefix framing, known PoC limitations |
