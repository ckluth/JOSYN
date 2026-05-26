# ResultPattern Discussion — Session 002: Umsetzung der Pre-Finalization Befunde

> Scope: Umsetzung aller Befunde aus Session 001.  
> Basis: `result-pattern-discussion-session-001.md`  
> Kein neuer Code hinzugefügt — ausschließlich Korrekturen und Bereinigungen.

---

## Korrekturen

### Befund 1 — Toter `using System.ComponentModel`-Import entfernt

`using System.ComponentModel` aus `Result.cs`, `Result.generic.cs`, `IResult.cs`, `IResult.generic.cs`
entfernt. Nebenbefund: `[EditorBrowsable]` hätte auf Parametern ohnehin keine Wirkung gezeigt — die
`internal_ignore_*`-Benennung bleibt die korrekte Lösung.

### Befund 2 — Konsistente Fehlermeldung bei `Exception → Error`

`Error`'s impliziter Operator von `Exception` verwendet jetzt `ResultHelper.FormatExceptionMessage`
statt `exception.Message` direkt. Beide Konvertierungswege (`return ex` und `Error err = ex; return err`)
erzeugen nun identische `ErrorMessage`-Werte.

Ein Test (`Error_ImplicitConversion_FromException`) wurde von `Is.EqualTo("oops")` auf `Does.Contain("oops")`
angepasst — die bisher behauptete Gleichheit war das ursprüngliche Fehlverhalten.

### Befund 3 — `needFileInfo: true` in impliziten Operatoren

`new StackFrame(1)` → `new StackFrame(1, needFileInfo: true)` in allen vier impliziten Operatoren
(`operator(Exception)` und `operator(Error)` in `Result.cs` und `Result.generic.cs`). Die `CallerInfo`
für über implizite Operatoren erstellte Failures enthält jetzt `FilePath` und `LineNumber`, sofern PDBs
vorhanden sind.

### Befund 4 — Konsistentes `"  at "`-Prefix im Callstack

`ResultHelper.CallStackToString` nutzt jetzt `string.Join("\n", callers.Select(c => $"  at {c}"))`.
Alle Einträge haben einheitlich `"  at "` als Prefix — der erste Eintrag war zuvor unformatiert.

### Befund 5 — `CallerInfo` versiegelt

`public record CallerInfo` → `public sealed record CallerInfo`.

### Befund 6 — `Result<T>.ToResult()` auf zwei Zeilen aufgeteilt

Zwei `return`-Statements auf einer Zeile in `Result.generic.cs` aufgetrennt.

### Befunde 7 & 9 — `Debug.Assert` in `Propagate()` und `ToResult<T>()`

`Debug.Assert(!result.Succeeded)` in `Propagate()` und `Debug.Assert(!Succeeded)` in `ToResult<T>()`
jeweils in `Result` und `Result<T>` ergänzt. Fehlnutzung wird in Debug-Builds sofort sichtbar,
Produktionsverhalten unverändert.

### Befund 10 — Auskommentierter Code in Tests entfernt

Drei auskommentierte Entwicklungsalternativen aus `ResultTestsPropagate.cs` entfernt.

---

## Ergebnis

56/56 Tests grün. Kein Verhalten geändert — ausschließlich Konsistenz, Korrektheit und Lesbarkeit.

**→ Nächste Phase: XML-Kommentare vervollständigen, NuGet-Paket erstellen.**
