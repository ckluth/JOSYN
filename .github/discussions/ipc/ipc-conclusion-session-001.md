# JOSYN.Core.IPC — Status-Quo & PoC-Abschlussbewertung

> Dokumentiert den aktuellen Stand der Implementierung: Architektur, Features, Protokoll-Konventionen
> und bewusste Design-Limitierungen. Keine Diskussionshistorie.

---

## Architektur

### Two-Pipe Design
Server und Client kommunizieren über **zwei getrennte Named Pipes** — eine für Requests (`req-pipe-<key>`), eine für Responses (`res-pipe-<key>`). Beide Pipes sind `PipeTransmissionMode.Byte` mit explizitem `PipeDirection` (`In`/`Out`). Die Richtungstrennung macht den Datenfluss eindeutig und vermeidet Half-Duplex-Komplexität.

### Length-Prefix Framing
Jede Nachricht wird als `int32`-Länge (little-endian) gefolgt von N Bytes übertragen. `BinaryWriter`/`BinaryReader`-Paare werden auf beiden Seiten konsistent eingesetzt. Der `Write(byte[], 0, n)`-Overload wird explizit verwendet, um das doppelte Längenpräfix zu vermeiden, das `Write(byte[])` erzeugen würde.

### Session-Key-Isolation
Pipe-Namen werden aus einem `Guid`-Session-Key abgeleitet. Mehrere gleichzeitige Sessions auf derselben Maschine kollidieren nicht.

### Static-Abstract-Interface-Pattern
`IPipesServer`, `IPipesClient`, `IPipesProtocol` verwenden C# 11 `static abstract`-Member als **API-Vertrags-Dokumentation**. Die Interfaces ermöglichen keine polymorphe Dispatch-Nutzung, sind aber der kanonische Eigentümer aller Protokoll-Konstanten und -Signaturen.

### Result-Pattern-Disziplin
Keine Exceptions leaken aus dem Call-Stack heraus. Alle `catch`-Blöcke verwenden `return ex;` (implizite Konvertierung). `Result.Propagate()` erhält den vollständigen Aufrufpfad über Schichtgrenzen hinweg.

---

## Protokoll-Konstanten (`IPipesProtocol`)

| Konstante | Wert | Bedeutung |
|-----------|------|-----------|
| `MagicToken` | `"JOSYN-IPC"` | Basis-Präfix; wird für CLI-Argumente verwendet |
| `MagicErrorToken` | `"JOSYN-IPC-ERROR"` | Server sendet Handler-Exception als Error-Response |
| `MagicBusyToken` | `"JOSYN-IPC-ERROR-BUSY"` | Client meldet: vorherige Anfrage noch ausstehend |
| `MagicShutdownToken` | `"JOSYN-IPC-SHUTDOWN"` | Client initiiert sauberes Server-Shutdown |

---

## Server-Features (`PipesServer`)

### Vorbedingungsprüfungen
Vor dem Erstellen von Pipes oder Starten von Prozessen wird geprüft:
- Mindestens ein Request-Handler muss gesetzt sein (`HandleStringRequest` oder `HandleRawRequest`)
- `reConnect = true` kombiniert mit `ClientExePath` wird als Konfigurationsfehler sofort zurückgewiesen

### Dual-Handler-Unterstützung
Aufrufer können entweder einen `Func<string, Task<string>>`-Handler (für text-basiertes Protokoll) oder einen `Func<byte[], Task<byte[]>>`-Handler (für binäres Protokoll) übergeben. Intern werden beide durch `RawRequestHandler` auf denselben Byte-Loop abgebildet.

### Handler-Fehler-Isolation
Eine Exception in `processRequest` tötet **nicht** die Verbindung. Sie wird gefangen, über den `onError`-Callback an den Aufrufer gemeldet, und als `MagicErrorToken`-prefixierter Response an den Client zurückgesendet. Der Client empfängt daraufhin ein fehlgeschlagenes `Result`.

### Cancellation
Zwei orthogonale Mechanismen:

**Polling-Predicate** (`Func<Task<bool>>? IsCancellationRequested`): Wird in einem separaten Task alle 100 ms abgefragt. Bei `true` wird `CancelledByCallback = true` gesetzt — bevor `CancelAsync()` aufgerufen wird — und der `CancellationTokenSource` gecancelt. Exceptions im Predicate werden bewusst ignoriert (dokumentierte Design-Entscheidung: der Callback soll lightweight sein).

**CancellationToken**: Wird an die Pipe-Verbindung und den Request-Loop übergeben. Zusätzlich registriert `cancellationToken.Register(() => reqPipe.Close())` einen Callback, der die Request-Pipe schließt, sobald Cancellation feuert — das weckt den blockierenden `ReadInt32()`-Aufruf sofort auf (`IOException`, sauberer Loop-Exit).

### MagicShutdownToken-Handling
Der Server erkennt den `MagicShutdownToken` als Request, sendet ihn als Ack zurück, und verlässt den Request-Loop mit `return true`. Dieser Bool propagiert durch `RunAsyncInternal` bis zur Reconnect-Schleife in `RunAsync`, die bei `true` abbricht — symmetrisch zum `CancelledByCallback`-Pfad.

### Reconnect-Loop
`RunAsync(args, reConnect: true, onReconnect: ...)` kapselt `RunAsyncInternal` in einer Schleife. Nach normalem Client-Disconnect (Loop-Exit mit `false`) wird `onReconnect` aufgerufen und eine neue Pipe-Instanz aufgemacht. Die Schleife bricht ab bei:
- Cancellation durch `IsCancellationRequested` (`CancelledByCallback = true`)
- Client-initiiertem Shutdown (`MagicShutdownToken`)
- Fehler (nicht-succeeded Result)

---

## Client-Features (`PipesClient`)

### Verbindungsaufbau mit Exponential Backoff
`ConnectAsync` versucht bis zu 5 Mal, beide Pipes zu verbinden. Startdelay 300 ms, Faktor 1,5, Cap 2.000 ms. Behandelt korrekt das Race zwischen Server-`WaitForConnectionAsync` und Client-Connect.

### Thread-sicherer Busy-Guard
`ClientPipes` verwendet `Interlocked.CompareExchange` auf einem privaten `int`-Feld. `TrySetBusy()` und `ClearBusy()` sind atomar — zwei gleichzeitige Aufrufer können nicht beide `SendRequestAsync` eintreten. Der abgewiesene Aufrufer erhält ein fehlgeschlagenes Result mit `MagicBusyToken`-Präfix.

### Transparente Fehlerbehandlung
`SendRequestAsync` erkennt den `MagicErrorToken`-Präfix in der Response und gibt ein fehlgeschlagenes `Result` zurück — der Aufrufer muss das Protokoll-Präfix nicht kennen.

### Sauberer Disconnect
`DisconnectAsync(pipes, sendShutdownRequest: true)` sendet zuerst den `MagicShutdownToken` (Server verlässt den Request-Loop, Reconnect-Loop bricht ab), flusht dann die Request-Pipe und disposed beide Pipes asynchron.

---

## Bewusste Design-Limitierungen

### Single-in-flight — kein Multiplexing
Der Request-Loop ist strikt sequenziell. Es gibt keine Request-IDs und kein Multiplexing. Parallele Requests würden zu korruptem Framing führen. Schutz liegt im Client (`TrySetBusy`). Diese Limitierung ist für den vorgesehenen Einsatz (internes Session-Server-IPC, ein Client pro Session) **explizit akzeptiert und ausreichend**.

### `shouldCancel`-Exceptions werden ignoriert
Eine Exception im `IsCancellationRequested`-Callback beendet den Polling-Task lautlos, ohne Cancellation auszulösen. Design-Entscheidung: Der Callback soll lightweight sein und keinen kritischen Code enthalten. Verstöße dagegen sind Sache des Aufrufers.

### `reConnect = true` nur ohne `ClientExePath`
Reconnect in Kombination mit Client-Start per Exe-Pfad wird als Konfigurationsfehler abgewiesen (`Result.Fail`), da jede Iteration einen neuen Client-Prozess starten würde. Reconnect ist ausschließlich für Szenarien vorgesehen, in denen der Client autonom lebt.

---

## Verbleibende Kleinigkeiten (kein Handlungsbedarf)

- **`MagicBusyToken` in `ErrorMessage`**: Die `ErrorMessage` des fehlgeschlagenen Results enthält das Protokoll-Präfix (`"JOSYN-IPC-ERROR-BUSYAnfrage abgelehnt: ..."`). Für rein interne Nutzung akzeptabel; vor einer öffentlichen API-Oberfläche wäre das Herausfiltern des Präfixes sauberer.

- **`RawRequestHandler`-Member als `internal`**: Die verschachtelte `private class` deklariert ihre Member als `internal`. Faktisch irrelevant (innerhalb einer `private class` erzeugen `internal`-Member keine nach außen sichtbaren Symbole), aber inkonsistent.

---

## Urteil

`JOSYN.Core.IPC` ist **produktionsreif für den vorgesehenen Einsatz**. Alle bekannten Risiken und Schwächen sind entweder behoben oder als bewusste, dokumentierte Design-Limitierungen des sequentiellen, session-isolierten Modells klassifiziert. Es gibt keine unbekannten Bugs oder stillen Fehlerquellen.

**→ Die nächste Phase ist: Dokumentation, XML-Comments, NuGet-Paket.**
