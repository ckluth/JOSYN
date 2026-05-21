# IPC Discussion — Session 005: Issue-Status-Review & Code-Analyse

> Scope: Überprüfung der seit Session-004 behobenen Probleme sowie Analyse des aktuellen Code-Stands.  
> Kein Code wurde während dieser Diskussion verändert.

---

## Teil 1 — Was wurde seit Session-004 behoben?

### ✅ 🔴 Fehlender `Flush` nach Response in `RequestLoopAsync`

`writer.Flush()` wird nun unmittelbar nach dem Schreiben der Antwort aufgerufen:

```csharp
writer.Write(response.Length);
writer.Write(response, 0, response.Length); // raw bytes only — no extra length prefix
writer.Flush();
```

Vollständig behoben.

---

### ✅ 🟡 Toter `OperationCanceledException`-Catch

Der Catch-Block wurde nicht entfernt, aber er erhielt einen erklärenden Kommentar:

```csharp
catch (OperationCanceledException)
{
    // can't happen in the current design - but, paranoia...
    return Result.Error("Request-Loop durch Aufrufer abgebrochen.");
}
```

Die Entscheidung, den Block zu behalten, ist dokumentiert und als explizite "Paranoia-Guard" markiert. Akzeptable Lösung.

---

### ✅ 🟡 Demo-Szenario-Nummern vs. Protokoll-Draft

Die Demo-Methoden wurden umbenannt und dokumentiert:

| Vorher           | Nachher          | Abgedecktes Draft-Szenario                        |
|------------------|------------------|---------------------------------------------------|
| `RunSzenario01`  | `RunSzenario02`  | Kein Client-Start; externer Dritter kennt den Key |
| `RunSzenario02`  | `RunSzenario03`  | Server startet Client per Exe-Pfad                |

`Main` enthält außerdem den Hinweis `// Specified Szenario 1 not implemented in this Demo so far...`. Die Nummerierung entspricht nun dem Protokoll-Draft. Vollständig behoben.

---

### ✅ 🟢 Issue #7 — Single-in-flight undokumentiert

Ein ausführlicher `DISCLAIMER`-Kommentarblock wurde direkt vor `RequestLoopAsync` eingefügt:

```csharp
//----------------------------------------------------------------------------------------
// DISCLAIMER: Single-in-flight — kein Multiplexing im zentralen Request-Loop!
//
// Wie in der Spec des JOSYN-IPC-Protocols als design-basierte Limitierung beschrieben:
// Der Request-Loop verarbeitet Anfragen strikt sequenziell.
// Parallele Requests könne zu undefiniertem Verhalten führen.
//
// CALM DOWN: "Will never happen" in der internen JOSYN-Implementierung. ;)
//----------------------------------------------------------------------------------------
```

Vollständig behoben.

---

### ✅ Bonus: Asynchrone Request-Handler (PoC-Einschränkung aus Session-001)

Diese Verbesserung war in Session 004 nicht explizit als Issue gelistet, aber in Session 001 als bekannte PoC-Einschränkung dokumentiert: *"Request handler is currently synchronous (`Func<byte[], byte[]>`) — async handlers needed before building on top of this."*

`PipesServer.RunAsync` und die Interfaces akzeptieren nun durchgehend asynchrone Handler:

```csharp
Func<byte[], Task<byte[]>> processRequest
Func<string, Task<string>> processRequest
```

Die PoC-Einschränkung ist damit beseitigt. Vollständig behoben.

---

## Teil 2 — Was ist noch offen?

### 🟢 `Result.Fail(ex)` Inkonsistenz (aus Session-004, weiterhin offen)

Am Ende von `RequestLoopAsync` (PipesServer.cs):

```csharp
catch (Exception ex) { return Result.Fail(ex); }
```

Alle anderen `catch`-Blöcke in der Codebasis nutzen `return ex;` (implizite Konvertierung). Sollte zur Konsistenz geändert werden.

---

### 🟢 `BinaryWriter`/`BinaryReader` ohne `using` in `SendRequestAsync` (aus Session-004, weiterhin offen)

In `PipesClient.SendRequestAsync`:

```csharp
var writer = new BinaryWriter(pipes.RequestPipe, Encoding.UTF8, leaveOpen: true);
var reader = new BinaryReader(pipes.ResponsePipe, Encoding.UTF8, leaveOpen: true);
```

Dank `leaveOpen: true` werden die Pipes nicht geschlossen, aber die Writer-/Reader-Objekte selbst werden nicht freigegeben. Konsequenter wäre `using var writer = ...` / `using var reader = ...`.

---

### ~~🟢 `PipesProtocol` sollte `static class` sein~~ — Befund zurückgezogen

Der Befund aus Session 004 muss korrigiert werden: In C# kann eine `static class` kein Interface implementieren. `PipesProtocol`, `PipesServer` und `PipesClient` implementieren alle jeweils `IPipesProtocol`, `IPipesServer` und `IPipesClient` mit `static abstract`-Membern (C# 11). Die Deklaration als `static class` wäre ein Compiler-Fehler. Der Befund war technisch falsch.

Die Klassen können nur dann zu `static class` werden, wenn die Interfaces aufgegeben würden — das wäre jedoch eine bewusste Designentscheidung, die über eine rein kosmetische Korrektur hinausgeht.

---

## Teil 3 — Neue Befunde

### 🟡 Flush-Asymmetrie: Client nutzt `pipes.RequestPipe.FlushAsync()`, Server `writer.Flush()`

Server (`RequestLoopAsync`) und Client (`SendRequestAsync`) spülen unterschiedlich:

```csharp
// Server (korrekt, synchron)
writer.Flush();

// Client (direkt auf Pipe, BinaryWriter wird übersprungen)
await pipes.RequestPipe.FlushAsync();
```

Da `BinaryWriter` in .NET keinen eigenen internen Puffer unterhält und direkt in den zugrunde liegenden Stream schreibt, sind beide Varianten **funktional äquivalent** — `pipes.RequestPipe.FlushAsync()` flusht denselben Stream, den `writer.Flush()` aufrufen würde. Es entsteht kein Datenverlust.

Stilistisch und zur Absicherung gegen künftige Refactorings (z. B. wenn ein gepufferter `StreamWriter` eingesetzt würde) wäre jedoch ein explizites `writer.Flush()` vor dem `FlushAsync()` konsistenter. Alternativ könnte der Writer auf `using var` umgestellt und `writer.Flush()` explizit aufgerufen werden.

---

### 🟢 `RawRequestHandler` ist `internal` statt `private`

Die verschachtelte Hilfsklasse in `PipesServer` ist als `internal class RawRequestHandler` deklariert:

```csharp
internal class RawRequestHandler
{
    internal required Func<string, Task<string>> ProcessStrings { get; set; }
    internal async Task<byte[]> ProcessRawRequest(byte[] requestBytes) { ... }
}
```

Als verschachtelte Klasse innerhalb von `PipesServer` wäre `private class` ausreichend — sie wird ausschließlich innerhalb von `PipesServer` verwendet. `internal` hebt die Sichtbarkeit unnötig auf Assembly-Ebene. Fix: `private class RawRequestHandler` (Member können dann auch `private` sein).

---

## Gesamtbewertung

Der Codestand hat seit Session-004 weitere Fortschritte gemacht: Alle 🔴- und 🟡-Issues sowie die bedeutendste PoC-Einschränkung (synchroner Request-Handler) sind behoben. Die verbleibenden Punkte:

1. **`Result.Fail(ex)` Inkonsistenz** (🟢, Kosmetik — `return ex;` bevorzugt)
2. **`BinaryWriter`/`BinaryReader` ohne `using`** (🟢, Kosmetik)
3. **Flush-Asymmetrie Client ↔ Server** (🟡, funktional korrekt, stilistisch inkonsistent)
4. **`RawRequestHandler` als `internal` statt `private`** (🟢, Kosmetik)
