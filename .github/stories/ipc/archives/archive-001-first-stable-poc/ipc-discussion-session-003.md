# IPC Discussion — Session 003: Issue-Status-Review & Protocol-Draft-Analyse

> Scope: Überprüfung der seit Session-002 behobenen Probleme sowie Analyse von `josyn-ipc-protocol-draft-01.md`.  
> Kein Code wurde während dieser Diskussion verändert.

---

## Teil 1 — Was wurde seit Session-002 behoben?

### ✅ Issue #2 — `record` → `sealed class`

`ClientPipes` und `ServerPipes` sind nun `sealed class` mit `required … { get; init; }`. Korrekt und vollständig.

---

### ✅ Spurious Trailing Space in `Demo.ClientExe/Program.cs`

`"Es wurde kein Pipes-SessionKey übergeben"` — kein abschließendes Leerzeichen mehr. Bereinigt.

---

### ✅ Issue #4 (teilweise) — `DisconnectAsync`: fehlender Flush & Fake-Async

`await pipes.RequestPipe.FlushAsync()` ist nun vor dem `Close()` vorhanden. Die Methode ist damit auch wirklich async (echter `await`). 

**Offener Rest:** Beide Pipes werden nach dem Flush noch mit dem synchronen `.Close()` geschlossen. Da `NamedPipeClientStream` `IAsyncDisposable` implementiert, wäre `await pipe.DisposeAsync()` konsequenter — aber für ein PoC nicht kritisch.

---

### ✅ Issue #6 — `await Task.FromResult(…)` in `CreatePipesAsync`

`CreatePipesAsync` ist jetzt eine **synchrone** Methode mit Rückgabetyp `Result<ServerPipes>`. Das unnötige async-Wrapping ist beseitigt.

**Neue Kleinigkeit:** Der Methodenname trägt weiterhin das `Async`-Suffix, obwohl die Methode synchron ist. Kein funktionaler Effekt, aber ein Namens-Widerspruch, der bei Gelegenheit bereinigt werden sollte (Umbenennung in `CreatePipes`).

---

## Teil 2 — Was ist noch offen? (Fortschreibung aus Session-002)

| # | Priorität | Status | Beschreibung |
|---|-----------|--------|--------------|
| 3 | 🟡 Medium | Offen  | `BinaryWriter` vs. `BitConverter` Framing-Inkonsistenz |
| 4 | 🟡 Medium | Teilweise | Flush hinzugefügt ✅; `.Close()` vs. `DisposeAsync()` noch offen |
| 5 | 🟡 Medium | Offen  | `CancellationTokenSource` wird nie disposed |
| 6 | 🟢 Low    | Behoben ✅ | `await Task.FromResult` entfernt |
| 7 | 🟢 Low    | Offen  | Single-in-flight-Constraint undokumentiert (im Code) |
| 8 | 🟢 Low    | Offen  | 3-Argumente-Branch in `ParseServerCLIArguments` hat keinen Aufrufer |

### Detail: Issue #5 — `CancellationTokenSource` nie disposed

`CreatePollingCancellationToken` gibt als Dispose-Handler zurück:

```csharp
return (() => { cancel?.Invoke(); }, cts.Token);
```

Die Action ruft nur `Cancel()` auf, aber nie `Dispose()` auf dem `CancellationTokenSource`. Fix:

```csharp
return (() => { cts.Cancel(); cts.Dispose(); }, cts.Token);
```

### Detail: Issue #3 — Framing-Inkonsistenz

Der Server schreibt die Response-Länge via `BinaryWriter.Write(int)`, der Client liest sie via `BitConverter.ToInt32(...)`. Auf x86/x64 (little-endian) funktioniert das identisch, ist aber konzeptionell gemischt. Konsistenter wäre es, auf einer Seite zu vereinheitlichen — entweder durchgehend `BinaryWriter`/`BinaryReader`, oder durchgehend `BitConverter` + direktes `stream.WriteAsync`.

---

## Teil 3 — Analyse: `josyn-ipc-protocol-draft-01.md`

Das Dokument ist gut strukturiert, vollständig in seiner Abdeckung der implementierten Schichten und korrekt in der Beschreibung der drei Szenarien. Es gibt vier Punkte, die Aufmerksamkeit verdienen:

---

### 🔴 CLI-Argumentreihenfolge: Draft vs. Code widersprechen sich

Der Draft definiert:

```
JOSYN-IPC  [<client-exe-path>]  <session-key>
```

Der Code implementiert die **umgekehrte** Reihenfolge:

```csharp
// PipesProtocol.ParseServerCLIArguments
return args.Length == 2 ? (args[1], string.Empty) : (args[1], args[2]);
// → effektiv: JOSYN-IPC <session-key> [<client-exe-path>]
```

Da der Draft als Spezifikation gilt, sollte der Code angepasst werden. Konkret betrifft das `ParseServerCLIArguments` (Reihenfolge der Indizes) und ggf. jede Stelle, die die 3-Argumente-Variante zusammenstellt.

---

### 🟡 Layer 1: Wire-Format nicht dokumentiert

Die Tabelle beschreibt Layer 1 als *"Rohe Nutzdaten als `byte[]`-Arrays"*. Das eigentliche Framing-Format — **`int32` (little-endian) als Längen-Präfix, gefolgt vom Payload** — wird nicht erwähnt. Für eine Implementierung durch Dritte (oder spätere Protokoll-Revisionen) ist das der entscheidende Vertrag. Empfehlung: einen Abschnitt "Wire-Format (Layer 1)" ergänzen:

```
[ 4 Bytes: Länge (int32, little-endian) ][ N Bytes: Payload (UTF-8) ]
```

---

### 🟡 Szenario 3: dokumentiert, aber kein Code-Pfad vorhanden

Der Draft formalisiert Szenario 3 (Server erhält Session-Key + Exe-Pfad per CLI und startet den Client selbst). Das Parsing existiert (`ParseServerCLIArguments` 3-Argumente-Branch). Jedoch gibt es keine Stelle im Server-Startup, die das Parsing-Ergebnis auswertet und beide Werte kombiniert in einen `RunAsync`-Aufruf übergibt. Das ist im Kern dasselbe wie Issue #8 ("3-Argumente-Branch hat keinen Aufrufer") — der Draft gibt dem Szenario nun einen Namen, behebt aber nicht die fehlende Code-Integration.

---

### 🟢 "Unidirektional: Client → Server" ist missverständlich

Unter *Kommunikationsrichtung* steht `Unidirektional: Client → Server`. Gemeint ist, dass ausschließlich der Client Anfragen initiiert (kein Server-Push). Da das Protokoll aber Response-Daten in Gegenrichtung sendet, könnte die Formulierung als "nur ein Weg" missverstanden werden. Klarere Formulierung wäre z. B.:

> *Anfragen-Initiierung: ausschließlich Client (kein Server-Push)*

---

## Gesamtbewertung

Der Codestand ist solide. Die kritischsten noch offenen Punkte sind:

1. **CLI-Arg-Reihenfolge** — Draft und Code sind inkonsistent (🔴, sollte vor einem weiteren Konsumenten behoben werden)
2. **Issue #5** — `CancellationTokenSource` Leak (🟡, trivialer Fix)
3. **Layer-1-Wire-Format** im Draft nachtragen (🟡, Dokumentationslücke)
4. **`CreatePipesAsync` umbenennen** in `CreatePipes` (🟢, Kosmetik)
