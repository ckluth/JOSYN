# IPC Discussions — Phase: stable-poc-v2

Diese Phase umfasst die Diskussionen zur Weiterentwicklung des IPC-PoC auf dem Branch
`evolution/ipc-convention-layer` (Commits `f5d109d` und `96b168a`).

## Enthaltene Sessions

| Session | Inhalt |
|---------|--------|
| [001](ipc-discussion-session-001.md) | Analyse von Commit `f5d109d`: Fehler-Isolation im Request-Handler, async Cancellation-Predicate (`Func<bool>` → `Func<Task<bool>>`), Magic-Error-Response-Protokoll, Reconnection-Pattern |
| [002](ipc-discussion-session-002.md) | Analyse von Commit `96b168a`: Vollständige Adressierung aller sechs Befunde aus Session 001 — Konstanten-Migration ins Interface, client-seitiges Error-Handling, Poll-Interval-Revert, Demo-Bereinigung |

## Ergebnis

**Alle sechs Befunde aus Session 001 wurden in Session 002 vollständig adressiert.**  
Zusätzlich wurden zwei bisher offene Design-Punkte aus der Vorphase umgesetzt:
`ClientPipes`/`ServerPipes` sind `sealed class`, und der Request-Handler ist async
(`Func<byte[], Task<byte[]>>`). Der PoC ist auf dem Stand von `evolution/ipc-convention-layer`
stabil und konsistent.

## Offene Punkte (übernommen in nächste Phase)

| # | Kategorie | Beschreibung | Priorität |
|---|-----------|--------------|-----------|
| 1 | Design | Single-in-flight ohne Multiplexing | 🟡 dokumentiert, für JOSYN-IPC akzeptiert |
| 8 | Technische Schuld | `/// <summary> TODO </summary>` auf Interface-Konstanten | 🟢 |
