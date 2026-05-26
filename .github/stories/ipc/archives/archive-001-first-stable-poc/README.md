# IPC Discussions — Phase: first-stable-poc

Diese Phase umfasst die Diskussionen zur Entwicklung und Stabilisierung des IPC-PoC
(`JOSYN.Core.IPC`).

## Enthaltene Sessions

| Session | Inhalt |
|---------|--------|
| [001](ipc-discussion-session-001.md) | Initiale PoC-Analyse, kritische Einschränkungen (sync Handler, Single-in-flight, `record`-Typen) |
| [002](ipc-discussion-session-002.md) | … |
| [003](ipc-discussion-session-003.md) | … |
| [004](ipc-discussion-session-004.md) | … |
| [005](ipc-discussion-session-005.md) | … |
| [006](ipc-discussion-session-006.md) | PoC-Abschluss & Gesamtbewertung — alle 🔴-Issues beseitigt, PoC stabil |

## Ergebnis

**Der IPC-PoC ist abgeschlossen.**  
Alle kritischen Issues (synchroner Handler, fehlende `using`-Blöcke, `Result.Fail(ex)`-Inkonsistenz)
sind behoben. Die verbleibenden offenen Punkte (Single-in-flight, `record`-Typen, Stil-Kleinigkeiten)
sind dokumentiert und für den PoC bewusst akzeptiert.

Die Implementierung dient als Grundlage für den nächsten Schritt:
**Integration in `JOSYN.SessionServer`**.

## Offene Punkte (übernommen in nächste Phase)

| # | Kategorie | Beschreibung | Priorität |
|---|-----------|--------------|-----------|
| 1 | Design | Single-in-flight ohne Multiplexing | 🟡 dokumentiert, für JOSYN-IPC akzeptiert |
| 2 | Design | `ClientPipes`/`ServerPipes` als `record` statt `sealed class` | 🟡 kein Blocking-Issue |
| 3 | Stil | Flush-Asymmetrie Client: `pipes.RequestPipe.FlushAsync()` statt `writer.Flush()` | 🟢 |
| 4 | Stil | `await using var writer` statt `using var writer` in `RequestLoopAsync` | 🟢 |
| 5 | Stil | `RawRequestHandler`-Member als `internal` statt `private` | 🟢 |
