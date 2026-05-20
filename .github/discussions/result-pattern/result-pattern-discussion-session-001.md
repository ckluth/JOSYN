# ResultPattern Discussion — Session 001: Pre-Finalization Analysis

> Scope: Vollständige Analyse der aktuellen Implementierung auf Basis von `Result.cs`, `Result.generic.cs`,
> den Support-Typen (`CallerInfo`, `Error`, `ResultHelper`, `ResultSuccess`), den Interfaces (`IResult<TSelf>`,
> `IResult<TSelf, TValue>`, `IFailure`) sowie allen drei Testklassen.
> Kein Code wurde während dieser Diskussion verändert.

---

## Überblick

Die Implementierung ist konzeptuell solide und alle 56 Tests sind grün. Das Fundament — keine Exceptions im
Call-Stack, implizite Konvertierungen, Propagate-Kette, Nullable-Absicherung — funktioniert korrekt. Die
Analyse zeigt jedoch eine Handvoll Probleme unterschiedlicher Schwere, die vor der Finalisierung adressiert
werden sollten.

---

## Befund 1 — `using System.ComponentModel` ist toter Import

### Problem

`using System.ComponentModel` ist in `Result.cs`, `Result.generic.cs`, `IResult.cs` und `IResult.generic.cs`
importiert, wird aber nirgendwo verwendet. Der Import war offensichtlich als Vorbereitung für
`[EditorBrowsable(EditorBrowsableState.Never)]` auf den `internal_ignore_*`-Parametern gedacht.

Das Attribut hätte jedoch keinerlei Wirkung: `[EditorBrowsable]` wird von IDEs (Visual Studio, Rider)
nur auf **Typen, Methoden, Properties, Felder und Events** ausgewertet — nicht auf Parameter. Das
Unterdrücken von Parametern aus IntelliSense ist mit .NET-Boardmitteln nicht möglich.

Die gewählte Benennung `internal_ignore_*` ist die korrekte und idiomatische Lösung für dieses Problem —
sie signalisiert dem Entwickler eindeutig "nicht anfassen", ohne eine (wirkungslose) Attributdeklaration.

### Lösung

Den toten `using System.ComponentModel`-Import aus allen vier Dateien entfernen.

---

## Befund 2 — Inkonsistente Fehlermeldung beim Weg `Exception → Error → Result`

### Problem

Wenn eine Exception direkt in ein Result umgewandelt wird, läuft sie durch
`ResultHelper.FormatExceptionMessage(exception)`:

```csharp
// Ergibt: "Ausnahmefehler: Datenbankverbindung fehlgeschlagen."
return ex;
```

Wenn dieselbe Exception erst in ein `Error`-Struct umgewandelt wird, verwendet `Error`'s impliziter Operator
schlicht `exception.Message`:

```csharp
public static implicit operator Error(Exception exception) => new(exception.Message, exception);
// Ergibt: "Datenbankverbindung fehlgeschlagen."   ← kein Prefix
```

Der Unterschied:

| Weg | `ErrorMessage` |
|-----|---------------|
| `return ex;` | `"Ausnahmefehler: Datenbankverbindung fehlgeschlagen."` |
| `Error err = ex; return err;` | `"Datenbankverbindung fehlgeschlagen."` |

Beide Wege erzeugen strukturell identische Results, aber mit unterschiedlicher `ErrorMessage`. Das ist
ein echter Verhaltens-Bug: die Wahl des Konvertierungswegs ändert die sichtbare Fehlermeldung.

### Lösung

`Error`'s impliziten Operator von `Exception` durch `ResultHelper.FormatExceptionMessage` führen:

```csharp
public static implicit operator Error(Exception exception)
    => new(ResultHelper.FormatExceptionMessage(exception), exception);
```

---

## Befund 3 — `new StackFrame(1)` ohne `needFileInfo: true`

### Problem

In den impliziten Operatoren `operator(Exception)` und `operator(Error)` in beiden Result-Typen:

```csharp
var frame = new StackFrame(1);  // needFileInfo fehlt
```

Ohne `needFileInfo: true` liefert `StackFrame` niemals Dateiname oder Zeilennummer — auch dann nicht, wenn
PDB-Dateien vorhanden sind. Das Ergebnis: `CallerInfo.FilePath` ist immer leer, `CallerInfo.LineNumber` ist
immer 0, sobald eine Exception oder ein Error über den impliziten Operator konvertiert wird.

Im Gegensatz dazu nutzen `Fail()` und `Propagate()` die `[Caller*]`-Attribute des Compilers, die Datei und
Zeile korrekt liefern. Implizite Operatoren können diese Attribute nicht verwenden — aber sie könnten
wenigstens `StackFrame(1, needFileInfo: true)` einsetzen, um die Information bereitzustellen, wenn PDBs
vorhanden sind.

### Lösung

```csharp
var frame = new StackFrame(1, needFileInfo: true);
```

in allen vier impliziten Operator-Implementierungen (`operator(Exception)` und `operator(Error)` jeweils
in `Result.cs` und `Result.generic.cs`).

---

## Befund 4 — `CallStackAsString`: erstes Element ohne führendes `"  at "`

### Problem

`ResultHelper.CallStackToString` verwendet `string.Join("\n  at ", ...)`:

```csharp
return string.Join("\n  at ", callers.Select(c => c.ToString()));
```

`Join` setzt den Separator *zwischen* die Elemente, nicht vor das erste. Das Ergebnis bei 4 Einträgen:

```
ResultTestsPropagate.LoadUserRecord()          ← kein "  at "
  at ResultTestsPropagate.ReadUserName() in ResultTestsPropagate.cs:49
  at ResultTestsPropagate.ParseUserAge() in ResultTestsPropagate.cs:42
  at ResultTestsPropagate.LoadUserDisplayName() in ResultTestsPropagate.cs:35
```

Der erste Eintrag hat kein `"  at "`, alle anderen schon. Das sieht wie ein Formatierungsfehler aus,
insbesondere wenn der Output in Logs oder Konsole erscheint.

### Lösung

Entweder alle Einträge mit `"  at "` prefixen:

```csharp
return string.Join("\n", callers.Select(c => $"  at {c}"));
```

Oder den ersten Eintrag anders hervorheben (z. B. als Ursprung). Konsistenz ist das Ziel.

---

## Befund 5 — `CallerInfo` ist nicht `sealed`

### Problem

```csharp
public record CallerInfo  // ← kein sealed
```

`CallerInfo` ist ein reiner Datenträger ohne Vererbungsabsicht. Das Fehlen von `sealed` ist ein Versehen —
es öffnet die Klasse für ungewollte Subtypen. In Kombination mit dem `record`-Mechanismus könnte eine
Subklasse das `ToString()`-Verhalten und damit die Callstack-Ausgabe verändern.

### Lösung

```csharp
public sealed record CallerInfo
```

---

## Befund 6 — `Result<T>.ToResult()` — zwei Statements auf einer Zeile

### Problem

`Result.generic.cs`, Zeile 100:

```csharp
public Result ToResult() 
{ 
    if (Succeeded) return Result.Success; return Result.FailSilent(ErrorMessage!, Exception) with { Callers = Callers };
}
```

Zwei `return`-Statements auf einer Zeile — einzige Stelle im gesamten Codebase, die dieses Muster
verwendet. Inkonsistent mit dem restlichen Stil und schwerer lesbar.

### Lösung

Auf zwei Zeilen aufteilen:

```csharp
public Result ToResult()
{
    if (Succeeded) return Result.Success;
    return Result.FailSilent(ErrorMessage!, Exception) with { Callers = Callers };
}
```

---

## Befund 7 — `Propagate()` auf einem succeeded Result ist lautlos ein No-Op

### Problem

```csharp
public static Result Propagate(Result result, ...)
{
    if (result.Succeeded) return result;  // ← stiller No-Op
    ...
}
```

Der Konvention entsprechend wird `Propagate()` **immer** hinter `if (!result.Succeeded)` aufgerufen. Ein
Aufruf auf einem succeeded Result ist ein logischer Fehler im aufrufenden Code. Die aktuelle Implementierung
gibt den succeeded Result unverändert zurück, ohne irgendein Signal.

Das verdeckt potenzielle Aufrufer-Fehler im Entwicklungszyklus. Ein `Debug.Assert` würde das Problem
in Debug-Builds sofort sichtbar machen, ohne das Produktionsverhalten zu ändern.

### Empfehlung

```csharp
if (result.Succeeded)
{
    Debug.Assert(false, "Propagate() wurde auf einem succeeded Result aufgerufen.");
    return result;
}
```

---

## Befund 8 — `IResult<TSelf>.Propagate` hat hardkodierte Rückgabe- und Parametertypen

### Problem

In `IResult<TSelf>` (die void-Variante):

```csharp
static abstract Result Propagate(Result result, ...);  // ← nicht TSelf, sondern Result
```

In `IResult<TSelf, TValue>` (die generische Variante) ist dasselbe korrekt typisiert:

```csharp
static abstract TSelf Propagate(TSelf result, ...);    // ← korrekt: TSelf
```

Die Asymmetrie ist unproblematisch solange `Result` die einzige `IResult<TSelf>`-Implementierung ist
(was durch `sealed record` garantiert wird). Aber als Interface-Kontrakt ist es architektonisch
inkonsistent: das Interface beschreibt kein abstraktes Muster, sondern einen konkreten Typ.

### Einschätzung

Kein akuter Handlungsbedarf — `sealed record Result` macht eine Fremdimplementierung unmöglich.
Die Inkonsistenz ist dokumentierungswürdig.

---

## Befund 9 — `ToResult<T>()` auf einem succeeded `Result` erzeugt einen stillen Fehlschlag

### Problem

```csharp
public Result<TValue> ToResult<TValue>()
{
    if (Succeeded) return Result<TValue>.FailSilent("Kein Wert vorhanden");  // ← stiller Fehler
    return Result<TValue>.FailSilent(ErrorMessage!, Exception) with { Callers = Callers };
}
```

`ToResult<T>()` existiert ausschließlich für den Propagation-Weg (`if (!result.Succeeded) return ...`).
Aufgerufen auf einem succeeded Result gibt es ein fehlgeschlagenes `Result<T>` mit der Meldung
`"Kein Wert vorhanden"` zurück — ohne jede Warnung oder Ausnahme.

Das ist korrekt nach Konvention, aber das Muster bietet keine Schutzmaßnahme gegen den falschen Aufruf.
Analog zu Befund 7: ein `Debug.Assert(!Succeeded, ...)` würde den Missbrauch im Entwicklungszyklus
auffangen.

### Empfehlung

```csharp
public Result<TValue> ToResult<TValue>()
{
    Debug.Assert(!Succeeded, "ToResult<T>() wurde auf einem succeeded Result aufgerufen.");
    if (Succeeded) return Result<TValue>.FailSilent("Kein Wert vorhanden");
    return Result<TValue>.FailSilent(ErrorMessage!, Exception) with { Callers = Callers };
}
```

---

## Befund 10 — Auskommentierter Code in den Tests

### Problem

`ResultTestsPropagate.cs` enthält an drei Stellen auskommentierten Entwicklungs-Alternativcode:

```csharp
// Zeile 61:  //return Result<Guid>.Fail(ex);
// Zeile 121: // Result.Fail(ex);
// Zeile 179: //Result<Guid>.Fail(ex);
```

Das sind Überbleibsel aus der Entwicklungsphase, in der `return ex;` gegen `Result.Fail(ex)` evaluiert
wurde. Kein Schaden, aber kein Mehrwert — sollte vor Finalisierung entfernt werden.

---

## Gesamtbewertung

| # | Datei | Befund | Schwere |
|---|-------|--------|---------|
| 1 | `Result.cs`, `Result.generic.cs`, `IResult.cs`, `IResult.generic.cs` | `using System.ComponentModel` ist toter Import — war für wirkungsloses `[EditorBrowsable]` auf Parametern vorgesehen | 🟢 |
| 2 | `Error.cs` | `implicit operator Error(Exception)` nutzt `exception.Message` statt `FormatExceptionMessage` — inkonsistente Fehlermeldungen | 🔴 |
| 3 | `Result.cs`, `Result.generic.cs` | `new StackFrame(1)` ohne `needFileInfo: true` — Datei/Zeilennummer nie verfügbar in impliziten Operatoren | 🟡 |
| 4 | `ResultHelper.cs` | `CallStackAsString`: erstes Element hat kein führendes `"  at "` — visuell inkonsistente Ausgabe | 🟡 |
| 5 | `CallerInfo.cs` | `CallerInfo` ist nicht `sealed` — ungewollte Subtypen möglich | 🟡 |
| 6 | `Result.generic.cs` | `ToResult()`: zwei Statements auf einer Zeile — Stil-Inkonsistenz | 🟢 |
| 7 | `Result.cs`, `Result.generic.cs` | `Propagate()` auf succeeded Result ist stiller No-Op — `Debug.Assert` würde Missbrauch sichtbar machen | 🟡 |
| 8 | `IResult.cs` | `IResult<TSelf>.Propagate` hat hardkodierte `Result`-Typen statt `TSelf` — architektonische Inkonsistenz zum generischen Pendant | 🟡 |
| 9 | `Result.cs`, `Result.generic.cs` | `ToResult<T>()` auf succeeded Result: stiller `"Kein Wert vorhanden"`-Fehlschlag — `Debug.Assert` empfohlen | 🟡 |
| 10 | `ResultTestsPropagate.cs` | Drei auskommentierte Code-Zeilen — Entwicklungs-Artefakte entfernen | 🟢 |

---

## Fazit

Die Implementierung ist funktional korrekt und vollständig durch Tests abgedeckt. Es gibt keine
unentdeckten Laufzeitfehler.

Die zwei kritischen Punkte (🔴) sind keine Correctness-Bugs im engeren Sinne, aber beide betreffen
die **Konsistenz des API-Vertrags**: Die inkonsistente Fehlermeldung zwischen `Exception → Result`
und `Exception → Error → Result` verletzt das Prinzip der geringsten Überraschung. Das fehlende
`needFileInfo: true` in impliziten Operatoren führt zu stiller Informationsarmut im Callstack.

**→ Empfohlene nächste Schritte: Befunde 2–5 beheben, dann XML-Kommentare vervollständigen und NuGet-Paket erstellen.**
