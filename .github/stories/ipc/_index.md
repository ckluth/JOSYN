# Story: ipc

## Key Decisions

- **Build custom IPC** — existing .NET solutions (gRPC, SignalR, named-pipe wrappers) don't fit the session-isolated, lightweight, no-dependency model required by JOSYN
- **Two-pipe design** — one pipe for requests (`req-pipe-<key>`), one for responses (`res-pipe-<key>`); direction separation avoids half-duplex complexity
- **Length-prefix framing** — `int32` (little-endian) + N bytes; `BinaryWriter`/`BinaryReader` on both sides; `Write(byte[], 0, n)` overload used explicitly to avoid double-length-prefix
- **`shouldCancel: Func<bool>?` predicate** — callers pass a simple predicate; converted internally to a polling `CancellationToken`; no `CancellationToken` management required from callers
- **JipClient / JipServer convenience layer** — built over the raw protocol to eliminate 4-step boilerplate per method call
- **`Result<string?>` as canonical JIP return type** — `Dict` and serialize/deserialize delegates removed from the JIP layer; app layer owns payload interpretation; `bool Succeeded` replaces `ResponseStatus` enum; `JipClient` has one method: `SendAsync(pipes, what, data?)`
- **`JipDispatcher` replaces manual switch** — fluent registration, `RegisterAll<TProtocol>` via reflection; What-strings derived from method names; client uses `nameof` for same-symbol reference
- **`JOSYN.Core.IPC` → `JOSYN.Foundation.JIP`** — final step of Core→Foundation migration; both old namespaces (`JOSYN.Core.IPC` for transport, `JOSYN.Core.IPC.JIP` for protocol) collapsed to flat `JOSYN.Foundation.JIP`; `JOSYN.Core` folder deleted

## Open Questions

- **Single-in-flight protocol** — strictly sequential, no request IDs; multi-in-flight support would require protocol redesign
- ~~**`ClientPipes`/`ServerPipes` as `record`**~~ — ✅ **already fixed**; both are `sealed class` in current code
- **NuGet pack** — `JOSYN.Foundation.JIP` not yet packed; consumers currently reference via `PackageReference` without a local package in place

## Resolved

- **Async request handler** — ✅ fixed; `Func<byte[], byte[]>` → `Func<byte[], Task<byte[]>>` applied throughout `PipesServer`, `JipServer`, `JipDispatcher`, and demo handlers; confirmed in `archive-001/ipc-discussion-session-002.md`

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
| 0004 | session-0004-protocol-simplification-generation.md | Radical JIP simplification: Dict removed, Result<string?> as canonical return type, JipClient to single method |
| 0005 | session-0005-echo-demo-generation.md | ECHO demo case added: client sends string, server returns "ECHO " + string |
| 0007 | session-0007-foundation-jip-migration-generation.md | JOSYN.Core.IPC → JOSYN.Foundation.JIP: Core→Foundation migration complete, JOSYN.Core deleted |
