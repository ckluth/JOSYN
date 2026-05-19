# IPC Discussion — Session 001: Stabilisierung der Basis-Implementierung

> Scope: Analyse der Änderungen auf Branch `evolution/ipc-convention-layer`, Commit `f5d109d` ("improvements").  
> Thema: Verbessertes Fehlerverhalten, async Cancellation, Reconnection-Pattern.  
> Kein Code wurde während dieser Diskussion verändert.

---

## Überblick der Änderungen

Der Commit umfasst vier inhaltliche Bereiche:

1. **Fehler-Isolation im Request-Handler** — Exception in `processRequest` tötet nicht mehr die Verbindung
2. **Async Cancellation-Predicate** — `Func<bool>` → `Func<Task<bool>>`
3. **Magic Error Response Protokoll** — `JOSYN-IPC-ERROR` Präfix in `PipesProtocol`
4. **Reconnection-Pattern im Demo** — Server-Loop nach Client-Disconnect

---

## Teil 1 — Fehler-Isolation im Request-Handler

### Was wurde geändert

`RequestLoopAsync` isoliert nun Exceptions aus `processRequest` in einem eigenen `try/catch`:

```csharp
byte[] response;
try
{
    response = await processRequest(requestBytes);
}
catch (Exception ex)
{
    await onError(Encoding.UTF8.GetString(requestBytes), ex);
    var responseStr = $"{PipesProtocol.MagicErrorResponsePrefix}{ex}";
    response = Encoding.UTF8.GetBytes(responseStr);
}
```

Alle `RunAsync`-Überladungen — inkl. Interface — bekommen einen neuen Pflicht-Parameter:

```csharp
Func<string, Exception, Task> onError
```

### Bewertung

#### ✅ Kernverbesserung: Verbindung überlebt Handler-Fehler

Bisher hätte eine Exception in `processRequest` den `catch (Exception ex) { return ex; }` von `RequestLoopAsync` erreicht und die Verbindung abgebrochen. Jetzt ist der Handler sauber isoliert: eine schlechte Request-Verarbeitung zerstört nicht die Session. Das ist der richtige Ansatz für einen stabilen IPC-Server.

#### ✅ `onError`-Callback-Design ist korrekt

Der Server weiß nicht, was mit dem Fehler passieren soll (loggen? ignorieren? eskalieren?). Die Entscheidung an den Aufrufer zu delegieren ist das richtige Muster — leichtgewichtig, nicht bevormundend.

#### 🟡 Client-seitige Asymmetrie: kein Handling für `MagicErrorResponsePrefix`

`PipesClient.SendRequestAsync` liefert die Antwort unverändert zurück:

```csharp
// PipesClient.SendRequestAsync (string-Variante)
return Encoding.UTF8.GetString(result.Value);
// → gibt "JOSYN-IPC-ERRORSystem.Exception: Oh No!..." als "Succeeded" zurück
```

Der Client-Aufrufer bekommt ein **erfolgreiches** `Result<string>`, dessen Inhalt jedoch ein Fehler ist. Er muss selbst prüfen:

```csharp
if (response.StartsWith(PipesProtocol.MagicErrorResponsePrefix)) { ... }
```

Das ist undokumentiert und kontraintuitiv. Zwei mögliche Lösungen:

**Option A (empfohlen):** `PipesClient.SendRequestAsync` erkennt das Präfix und gibt ein **fehlgeschlagenes** `Result<string>` zurück — der Fehler wird in-band transportiert und transparent ins Result-Pattern überführt. Der Aufrufer merkt davon nichts Ungewöhnliches.

**Option B:** Präfix bleibt Sache des Aufrufers, aber `PipesProtocol` bekommt eine Hilfsmethode `TryParseErrorResponse(string response, out string errorText)`, und es wird dokumentiert.

Option A ist ergonomischer und schließt die Lücke vollständig. Der Fehler-Text (`ex.ToString()`) kann dabei in der `ErrorMessage` des fehlgeschlagenen `Result` landen.

#### 🟡 `MagicErrorResponsePrefix` als `static readonly` statt `const`

`MagicToken` ist `const`. `MagicErrorResponsePrefix` ist davon abgeleitet und könnte ebenfalls `const` sein:

```csharp
// Aktuell:
public static readonly string MagicErrorResponsePrefix = $"{MagicToken}-ERROR";

// Besser (konsistent mit MagicToken):
public const string MagicErrorResponsePrefix = MagicToken + "-ERROR";
```

`static readonly` verhindert Compile-Time-Inlining (z. B. in `switch`-Ausdrücken oder Attributen) und ist ohne Grund inkonsistent zum `const MagicToken`.

#### 🟡 `MagicErrorResponsePrefix` fehlt in `IPipesProtocol`

Das neue Protokoll-Element ist in `PipesProtocol` definiert, aber nicht in `IPipesProtocol` deklariert. Da das Interface als API-Kontrakt-Dokumentation dient (C# 11 `static abstract`), sollte es dort als `const` oder `static abstract`-Property auftauchen. Aktuell könnte eine alternative Implementierung das Error-Präfix völlig ignorieren oder anders benennen, ohne Compiler-Warnung.

---

## Teil 2 — Async Cancellation-Predicate

### Was wurde geändert

```csharp
// Vorher
Func<bool>? shouldCancel
Thread.Sleep(pollIntervalMs)         // pollIntervalMs = 100
cts.Cancel()

// Nachher
Func<Task<bool>>? shouldCancel
await Task.Delay(pollIntervalMs)     // pollIntervalMs = 500
await cts.CancelAsync()
```

### Bewertung

#### ✅ Async polling ist korrekt

`Thread.Sleep` in einem `Task.Run`-Block blockiert einen Thread-Pool-Thread für die gesamte Sleep-Dauer. `await Task.Delay` gibt den Thread zurück — das ist die idiomatische async-Variante und konsistent mit dem Rest der Implementierung.

#### ✅ `cts.CancelAsync()` statt `cts.Cancel()`

Seit .NET 8 ist `CancelAsync()` die bevorzugte Variante. `Cancel()` kann synchron registrierte Callbacks blockierend ausführen; `CancelAsync()` verhindert das.

#### ✅ `Func<Task<bool>>` öffnet den Callback für async-Nutzung

Ein `shouldCancel`-Callback, der z. B. einen Channel abfragt oder einen Datenbankstatus prüft, ist nun ohne `Task.Result`-Blocking möglich. Sinnvolle Erweiterung.

#### 🟡 Poll-Interval 100ms → 500ms: TODO offen

Der Code enthält:

```csharp
// TODO: fixes Poll-Interval 500ms dokumentieren
```

Die Erhöhung bedeutet bis zu **500ms Latenz** zwischen Cancellation-Request und tatsächlichem Abbruch. Für ESC-Tasten-Abbruch eines interaktiven Demos: irrelevant. Für programmatischen Abbruch in automatisierten Szenarien oder Tests: relevant. 

Die Begründung für die Erhöhung fehlt noch. Möglicher Grund: bei 100ms-Intervall und vielen kurzen Verbindungen entsteht unnötiger CPU-Churn. Bei 500ms ist der Overhead vernachlässigbar. Das sollte im Kommentar festgehalten werden, sobald der TODO bearbeitet wird.

#### 🟢 Design-Entscheidung: Exceptions in `shouldCancel()` werden ignoriert

Der Kommentar dokumentiert bewusst:

> "Explizite Design-Entscheidung hier: Der IsCancellationRequested-Callback soll lightweight und schlank implementiert sein. Bei Verstoß: 'Pech gehabt'."

Das ist eine klare, dokumentierte Einschränkung. Für ein internes System, wo der Aufrufer die Kontrolle über den Callback hat, ist das akzeptabel. Der Kommentar-Block sollte langfristig ins offizielle Protokoll-Dokument übernommen werden.

---

## Teil 3 — Reconnection-Pattern im Demo

### Was wurde geändert

`RunSzenario02` umhüllt `RunAsync` nun in einer Schleife:

```csharp
while (true)
{
    res = await PipesServer.RunAsync(HandleRequest, ..., sessionKey, WasEscapePressed);
    if (res.Succeeded)
    {
        if (wasEscaped) break;
        Console.WriteLine("Reestablishing Connection");
        // → nächste Iteration: wartet auf neuen Client
    }
    else
        break;
}
```

### Bewertung

#### 🟡 Konzept gehört langfristig in die `PipesServer`-API

Der aktuelle `RunAsync` ist ein "einmaliger Verbindungszyklus": verbinden → Request-Loop → Client trennt sich → return. Das Reconnect-Muster muss der Aufrufer selbst schreiben. Das ist für erfahrene Entwickler transparent, aber unintuitive API-Ergonomie.

Langfristig wäre ein `RunUntilCancelledAsync(...)` sinnvoll, das intern die Reconnect-Schleife kapselt. Für das aktuelle PoC-Stadium: kein Blocking-Issue.

#### 🟢 `wasEscaped`-Flag als statisches Feld

Funktioniert im Demo-Kontext (Single-Instance). Wäre in einem echten System ein Problem (shared state, nicht thread-safe). Für die Demo: völlig akzeptabel.

---

## Teil 4 — Demo-Testcode

Zwei absichtliche Exceptions im Demo wurden commitet:

```csharp
private static Task<bool> WasEscapePressed()
{
    throw new Exception("aaa");  // ← testet: Exception in shouldCancel() wird ignoriert
    ...
}

private static Task<string> HandleRequest(string requestStr)
{
    ...
    throw new Exception("Oh No!");  // ← testet: Exception in Handler → ErrorResponse
    return Task.FromResult(responseStr);
}
```

Das sind bewusste Test-Artefakte, um die neuen Mechanismen zu verifizieren. Es ist sauber, dass sie auf dem Feature-Branch commitet wurden — zeigt, dass die Funktionalität tatsächlich durchgetestet wurde.

**Erwartung:** Diese `throw`-Zeilen werden vor einem Merge auf `main` oder vor Integration in produktiven Code entfernt.

---

## Teil 5 — Tooling-Änderungen (ClientExe)

- **Pre-Build-Target auskommentiert:** Verhindert, dass beim Client-Build ungewollt ein Server-Prozess gestartet wird. Saubere Entscheidung.
- **Session-Key auf GUID umgestellt:** `"my-session-key"` → echter GUID. Realistischeres Szenario, verhindert Key-Kollisionen bei mehreren laufenden Instanzen.
- **`start-server.cmd` hinzugefügt:** Hilfreich für manuelles Testen, ohne durch die IDE navigieren zu müssen.

Keine inhaltlichen Anmerkungen — reine Ergonomie-Verbesserungen.

---

## Gesamtbewertung

| # | Bereich | Befund | Priorität |
|---|---------|--------|-----------|
| 1 | Design | Client erkennt `MagicErrorResponsePrefix` nicht; `SendRequestAsync` gibt Erfolg zurück obwohl Server-seitig Fehler | 🟡 |
| 2 | Konsistenz | `MagicErrorResponsePrefix` sollte `const` sein (wie `MagicToken`) | 🟢 |
| 3 | Kontrakt | `MagicErrorResponsePrefix` fehlt in `IPipesProtocol` | 🟢 |
| 4 | Dokumentation | Poll-Interval 500ms: TODO offen, Begründung fehlt | 🟢 |
| 5 | Demo | `throw new Exception("aaa")` / `throw new Exception("Oh No!")` — Test-Artefakte, vor Merge entfernen | 🟢 |
| 6 | API-Design | Reconnect-Pattern nur im Demo; langfristig `RunUntilCancelledAsync` erwägenswert | 🟡 |

### Fazit

Die Kernverbesserung — Fehler-Isolation im Request-Handler — ist solide und gut durchdacht. Der `onError`-Callback-Ansatz ist clean. Das async Cancellation-Predicate ist die korrekte Konsequenz aus der bereits async ausgerichteten Architektur.

Der einzige offene Punkt mit echtem Gewicht ist das fehlende Client-seitige Handling des `MagicErrorResponsePrefix` (Befund #1): Solange `PipesClient` das Präfix nicht auswertet, ist das Error-Protokoll nur halb implementiert — der Server sendet strukturierte Fehler, der Client sieht sie als Erfolg. Das sollte vor der nächsten Ausbaustufe geschlossen werden.
