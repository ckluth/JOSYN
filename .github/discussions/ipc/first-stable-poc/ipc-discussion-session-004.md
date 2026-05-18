# IPC Discussion — Session 004: Issue-Status-Review & Code-Analyse

> Scope: Überprüfung der seit Session-003 behobenen Probleme sowie Analyse des aktuellen Code-Stands.  
> Kein Code wurde während dieser Diskussion verändert.

---

## Teil 1 — Was wurde seit Session-003 behoben?

### ✅ Issue #3 — Framing-Inkonsistenz (`BinaryWriter` vs. `BitConverter`)

Client und Server verwenden nun durchgehend `BinaryWriter`/`BinaryReader`. Die kritische Falle — `BinaryWriter.Write(byte[])` erzeugt ein doppeltes Längen-Präfix — ist sauber durch `Write(byte[], 0, n)` umgangen und im Code explizit kommentiert. Vollständig behoben.

---

### ✅ Issue #4 (Rest) — `.Close()` vs. `DisposeAsync()`

`DisconnectAsync` verwendet nun `await pipes.RequestPipe.DisposeAsync()` und `await pipes.ResponsePipe.DisposeAsync()`. Vollständig behoben.

---

### ✅ Issue #5 — `CancellationTokenSource` nie disposed

Der Dispose-Handler ruft nun `cts.Dispose()` auf:

```csharp
return (() => { cancel?.Invoke(); cts.Dispose(); }, cts.Token);
```

Vollständig behoben.

---

### ✅ `CreatePipesAsync` → `CreatePipes` (Umbenennung)

Die Methode ist synchron und heißt nun korrekt `CreatePipes`. Behoben.

---

### ✅ Issue #8 / Szenario 3 — kein Aufrufer

Das Demo-`ServerExe` wertet `ParseServerCLIArguments` nun vollständig aus und verzweigt auf `RunSzenario01` (kein Client-Start) bzw. `RunSzenario02` (Server startet Client). Die fehlende Code-Integration ist behoben.

---

### ✅ CLI-Arg-Reihenfolge (🔴 aus Session-003 Teil 3)

Das Protokoll-Draft wurde aktualisiert und beschreibt nun:

```
JOSYN-IPC <session-key> [<client-exe-path>]
```

Dies stimmt mit der Code-Implementierung überein (`args[1]` = Session-Key, `args[2]` = Exe-Pfad). Widerspruch beseitigt.

---

### ✅ Layer-1-Wire-Format im Draft nachtragen (🟡 aus Session-003 Teil 3)

Layer 1 in der Schichtentabelle enthält nun die fehlende zweite Zeile:

```
[ 4 Bytes: Länge (int32, little-endian) ][ N Bytes: Payload (UTF-8) ]
```

Dokumentationslücke geschlossen.

---

## Teil 2 — Was ist noch offen?

### Issue #7 — Single-in-flight-Constraint undokumentiert (🟢 aus Session-003, weiterhin offen)

Das Request-Loop verarbeitet Anfragen strikt sequenziell; parallele Requests führen zu undefiniertem Verhalten. Im Protokoll-Draft ist "Multiplexing: Nicht unterstützt" vermerkt, aber der Code selbst enthält keinen Kommentar, der Aufrufer von dieser Einschränkung warnt. Ein kurzer `// WARNUNG: Single-in-flight — kein Multiplexing.`-Hinweis im `RequestLoopAsync` wäre hilfreich.

---

## Teil 3 — Neue Befunde

### 🔴 Fehlender `Flush` nach Response in `RequestLoopAsync`

Nach dem Schreiben der Antwort wird weder `writer.Flush()` noch `await resPipe.FlushAsync()` aufgerufen:

```csharp
writer.Write(response.Length);
writer.Write(response, 0, response.Length);
// ← kein Flush
```

`BinaryWriter` puffert intern. Bei längerem Betrieb oder großen Payloads kann der Client blockieren, weil die Antwort im Puffer verbleibt. Fix: `writer.Flush()` (oder `await resPipe.FlushAsync()`) nach dem Schreiben der Antwort einfügen.

---

### 🟡 `OperationCanceledException`-Catch in `RequestLoopAsync` ist toter Code

```csharp
var messageLength = reader.ReadInt32();   // synchron, kein CancellationToken
requestBytes = reader.ReadBytes(messageLength);
// ...
catch (OperationCanceledException) { ... }  // ← nie erreichbar
```

`BinaryReader.ReadInt32()` und `ReadBytes()` akzeptieren kein `CancellationToken`. Eine Anforderung, die im synchronen Lesezugriff steckt, wird durch das Cancellation-Token nicht unterbrochen — stattdessen würde beim Trennen der Pipe eine `IOException` ausgelöst. Der `OperationCanceledException`-Catch ist faktisch unerreichbar und vermittelt ein falsches Sicherheitsgefühl. Konsequenz: Cancellation während einer laufenden Anfrage ist nicht möglich (bekannte PoC-Einschränkung), sollte aber dokumentiert oder der tote Catch-Block entfernt werden.

---

### 🟡 Namens-Inkonsistenz: Demo-Szenario-Nummern vs. Protokoll-Draft

Der Draft definiert drei Szenarien (1 = autonom, 2 = extern mit Key, 3 = extern mit Key + Exe-Pfad). Die Demo implementiert:

| Demo-Methode    | Tatsächlich abgedecktes Szenario |
|-----------------|----------------------------------|
| `RunSzenario01` | Draft-Szenario 2 (kein Client-Start, Dritte gibt Key) |
| `RunSzenario02` | Draft-Szenario 3 (Server startet Client per Exe-Pfad) |

Draft-Szenario 1 (autonomer Produktivbetrieb, Server kennt Client-Exe proprietär) hat keine Demo-Implementierung. Die abweichende Nummerierung ist beim Lesen beider Dokumente verwirrend. Empfehlung: Methoden in `RunSzenario02` und `RunSzenario03` umbenennen oder die Demo-Kommentare auf den Draft verweisen.

---

### 🟢 `catch (Exception ex) { return Result.Fail(ex); }` — Inkonsistenz

Am Ende von `RequestLoopAsync`:

```csharp
catch (Exception ex) { return Result.Fail(ex); }
```

Alle anderen `catch`-Blöcke in der Codebasis nutzen `return ex;` (implizite Konvertierung). `Result.Fail(ex)` vs. implizite Konvertierung kann ein unterschiedliches Verhalten bei der Fehlererfassung erzeugen. Sollte zur Konsistenz auf `return ex;` geändert werden.

---

### 🟢 `BinaryWriter`/`BinaryReader` in `SendRequestAsync` nicht disposed

In `PipesClient.SendRequestAsync(byte[], ClientPipes)` werden `BinaryWriter` und `BinaryReader` ohne `using` erstellt:

```csharp
var writer = new BinaryWriter(pipes.RequestPipe, Encoding.UTF8, leaveOpen: true);
var reader = new BinaryReader(pipes.ResponsePipe, Encoding.UTF8, leaveOpen: true);
```

Dank `leaveOpen: true` wird die Pipe nicht geschlossen, aber die Writer/Reader-Objekte selbst werden nicht freigegeben. Für ein PoC unkritisch, aber konsequenter wäre `using var writer = ...`.

---

### 🟢 `PipesProtocol` sollte `static class` sein

`PipesProtocol` hat keine Instanzmember und wird nirgends instantiiert. `PipesClient` und `PipesServer` sind ebenfalls statische Klassen. Die Klasse sollte als `static class PipesProtocol` deklariert sein, damit der Compiler die Instantiierung verhindert.

---

## Gesamtbewertung

Der Codestand hat seit Session-003 deutliche Fortschritte gemacht — alle 🔴- und 🟡-Issues aus Session-003 sind behoben. Die verbleibenden Punkte:

1. **Fehlender `Flush` nach Response** (🔴, funktional: blockierender Client möglich)
2. **Toter `OperationCanceledException`-Catch** (🟡, irreführend; gleichzeitig dokumentiert, dass Cancellation während laufender Anfrage nicht greift)
3. **Demo-Szenario-Nummern vs. Draft-Nummern** (🟡, Verwirrung bei parallelem Lesen von Code und Spec)
4. **`Result.Fail(ex)` Inkonsistenz** (🟢, Kosmetik)
5. **`BinaryWriter`/`BinaryReader` ohne `using`** (🟢, Kosmetik)
6. **`PipesProtocol` nicht `static`** (🟢, Kosmetik)
7. **Issue #7 Single-in-flight undokumentiert** (🟢, bereits seit Session-003 offen)
