# IPC Discussion — Session 001: JIP Implementierungs-Helfer (`JipClient` / `JipServer`)

> **Scope:** Design und Implementierung einer Convenience-Schicht über dem JIP-Protokoll.  
> **Branch/Commit:** main, nach Abschluss der `JOSYN.Core.IPC` PoC-Stabilisierung.  
> **Ergebnis:** Zwei neue statische Klassen (`JipClient`, `JipServer`) + Demo-Refactoring.  
> Code wurde während dieser Diskussion implementiert.

---

## 1. Ausgangslage und Motivation

### Das Problem: Overhead in der Praxis

`JipProtocol` definiert den Protokoll-Vertrag sauber — Parsing, Result↔Response-Konvertierung, Fehlerklassen. Der Demo-Client zeigt jedoch, dass der Entwickler für **jeden einzigen Methodenaufruf** vier manuelle Schritte wiederholen muss:

```csharp
// Jede Methode in jedem X-JIP-Client braucht genau das:
var getRaw      = await PipesClient.SendRequestAsync(new Request { What = "GET-CONFIG" }.ToString(), pipes);
var parseResult = JipProtocol.ParseResponse(getRaw.Value);
var response    = parseResult.Value;
var result      = JipProtocol.ToResult<string>(response, d => d);
```

Diese vier Schritte sind vollständig generisch — kein einziger ist anwendungsspezifisch. Jedes X-JIP-Protokoll (J-JIP, XJIP, etc.) würde denselben Boilerplate enthalten. Analog auf der Server-Seite: `ParseRequest`, Parse-Fehler-Handling, Switch-Body, `response.ToString()` — alles mechanisch wiederholbar.

### Die Idee

JIP könnte eine **Implementierungs-Helfer-Schicht** anbieten: statische Klassen, die außerhalb des Protokoll-Vertrags (`IJipProtocol`) stehen, aber die gesamte wiederkehrende Pipeline verstecken. X-JIP-Entwickler sehen nur noch: Methodenname, Payload-Typ, Ergebnis-Typ.

---

## 2. Design-Diskussion

### 2.1 Kernprinzip: "Alles verstecken, was unnötig ist"

Das Leitprinzip: keine geschweiften Klammern, keine Zwischenvariablen, keine manuellen Transformationsschritte im X-JIP-Code. Der Entwickler sieht ausschließlich, was fachlich relevant ist.

### 2.2 Grenzziehung: Außerhalb des Vertrags

Die Helfer stehen explizit **außerhalb von `IJipProtocol`**. Der Protokoll-Vertrag bleibt minimal und stabil. Die Helfer sind eine optionale Komfort-Schicht — X-JIP kann sie verwenden oder direkt auf `JipProtocol` aufbauen.

### 2.3 Die JSON-Default-Frage (abgelehnt)

**Diskutiert:** Ob `JipClient.SendAsync<T>` ohne `Func<string, T>`-Parameter angeboten werden sollte, mit `JsonSerializer.Deserialize<T>` als Default.

**Entscheidung: Nein.**

Der Payload (das `Data`-Feld in `Request`/`Response`) ist für JIP ein opaker UTF-8-String (ein "dummer BLOB"). Die Hoheit über das Payload-Format liegt **ausschließlich bei der Applikation** — YAML, XML, INI, JSON, anything-goes. JIP macht keine Annahmen über den Inhalt. Daher:
- `Func<TIn, string> serialize` und `Func<string, TOut> deserialize` sind **immer Pflichtparameter** (wo ein Payload vorkommt)
- Keine JSON-Default-Overloads, weder im Kern noch in einer separaten Convenience-Klasse
- Diese Grenze gilt für `JipClient` und alle zukünftigen Helfer

*Klarstellung:* "intern JSON" meint das Wire-Format des JIP-Protokolls selbst (Request/Response-Serialisierung). Das ist fest eingebaut und von dieser Frage vollständig unberührt.

### 2.4 Server-Seite: `WrapHandler` statt Dispatcher

**Diskutiert:** Ob ein Dispatcher-Pattern (`dispatch.On("PING", handler).On(...)`) in `JipServer` gehört.

**Entscheidung:** Nur `WrapHandler`. Ein Dispatcher wäre eine X-JIP-Verantwortung, nicht eine JIP-Verantwortung. `JipServer.WrapHandler` kapselt ausschließlich die mechanischen Schritte (ParseRequest, Fehler-Wrapping, `.ToString()`). Die fachliche Dispatch-Logik (`req.What switch { ... }`) gehört zum Implementierungsprotokoll.

### 2.5 Naming: `JipClient`/`JipServer` vs. Alternativen

**Risiko identifiziert:** Die Namen klingen nach Ersatz für `PipesClient`/`PipesServer`.

**Entscheidung:** Beibehalten. Die Abstraktionsebenen sind deutlich verschieden:
- `PipesClient` / `PipesServer` = Transport-Schicht (rohe Pipes)
- `JipClient` / `JipServer` = JIP-Protokoll-Schicht (typisierte Methoden)

Die parallel laufenden Konventionen widerspiegeln eine klare Schichttrennung, keine Verwechslungsgefahr. Die XML-Dokumentation stellt den Zusammenhang klar.

### 2.6 Response-Shorthand-Aliases (abgelehnt)

**Diskutiert:** `JipServer.Ok(result)` als Alias für `JipProtocol.ToResponse(result)`, `JipServer.Unknown(what)` für `JipProtocol.ToLogicalFailureResponse(...)`.

**Entscheidung:** Nicht implementiert. `JipProtocol.ToResponse` ist bereits präzise und kurz. Den Dispatch-Body zusätzlich mit einem zweiten API-Pfad zu belasten wäre redundant. `WrapHandler` allein ist der echte Wert auf der Server-Seite.

### 2.7 Transport-Kopplung

`JipClient` referenziert `PipesClient` und `ClientPipes` direkt — d.h. JIP ist nicht transport-agnostisch. Akzeptierte Einschränkung für den PoC-Stand, solange Named-Pipe-IPC der einzige Transport ist. Wenn ein zweiter Transport (TCP, Unix-Socket) folgt, müssen die Helfer entweder dupliziert oder hinter ein Delegate abstrahiert werden. **Bewusst dokumentiert, kein Handlungsbedarf jetzt.**

---

## 3. Implementierung

### 3.1 `JipClient` — Overload-Matrix

| Signatur | Eingabe | Ausgabe |
|----------|---------|---------|
| `SendAsync(pipes, what)` | — | `Task<Result>` |
| `SendAsync<TIn>(pipes, what, data, serialize)` | Typed | `Task<Result>` |
| `SendAsync<TOut>(pipes, what, deserialize)` | — | `Task<Result<TOut>>` |
| `SendAsync<TIn, TOut>(pipes, what, data, serialize, deserialize)` | Typed | `Task<Result<TOut>>` |
| `SendDictAsync(pipes, what)` | — | `Task<Result<Dictionary<string,string>>>` |

`SendDictAsync` adressiert den `Response.Dict`-Pfad, der von `ToResult<T>` (Data-only) nicht abgedeckt wird.

**Interner Kern:** `SendCoreAsync` führt `PipesClient.SendRequestAsync` + `JipProtocol.ParseResponse` durch. Alle öffentlichen Overloads delegieren hierher und mappen das `Result<Response>` auf den erwarteten Return-Typ via `.ToResult()` / `.ToResult<T>()`.

**Exception-Handling für `serialize`:** Ein `try/catch` um den `serialize(data)`-Call mit `return ex` (implizite Konvertierung) — konsistent mit dem JOSYN Result-Pattern.

### 3.2 `JipServer` — WrapHandler

Zwei Overloads (sync und async Handler):

```csharp
Func<string, Task<string>> WrapHandler(Func<Request, Response> handler)
Func<string, Task<string>> WrapHandler(Func<Request, Task<Response>> handler)
```

Bei Parse-Fehler: automatisch `JipProtocol.ToLogicalFailureResponse(...)` — der Handler wird gar nicht erst aufgerufen.

### 3.3 Demo-Refactoring

**Client:** `SendAndPrint<T>` vollständig entfernt. Direkte `JipClient`-Calls. Neues `PrintResult(label, result, data?)` nur für Console-Ausgabe zuständig.

**Server:** `HandleRequest` auf eine Logging-Shell reduziert. Dispatch-Logik in statischem `_dispatch`-Feld mit `JipServer.WrapHandler` + Switch-Expression. Trennung von Logging-Concern und Dispatch-Concern.

---

## 4. Ergebnis-Vergleich

### Client — vorher / nachher

```csharp
// VORHER (4 Schritte pro Methode, neue Zwischenvariablen, geschliffene Klammern)
if (await SendAndPrint(pipes, new Request { What = "GET-CONFIG" }, r => JipProtocol.ToResult<string>(r, d => d)) is not 0 and var e) return e;

// NACHHER (1 Zeile)
var config = await JipClient.SendAsync<string>(pipes, "GET-CONFIG", d => d);
```

### Server — vorher / nachher

```csharp
// VORHER (~15 Zeilen mit ParseRequest, if(!Succeeded){...}, switch, ToString())
private static Task<string> HandleRequest(string requestStr) { /* ... */ }

// NACHHER (1 Expression + Logging-Shell)
private static readonly Func<string, Task<string>> _dispatch = JipServer.WrapHandler(req => req.What switch
{
    "PING"       => JipProtocol.ToResponse(Result.Success),
    "GET-CONFIG" => JipProtocol.ToResponse(Result<string>.Success("..."), d => d),
    "GET-DICT"   => new Response { Status = ResponseStatus.Success, Dict = ... },
    _            => JipProtocol.ToLogicalFailureResponse($"Unbekannte Funktion: '{req.What}'"),
});
```

---

## 5. Offene Punkte und Zukunft

### 5.1 Async-Handler ist vorbereitet, aber nicht genutzt

`JipServer.WrapHandler(Func<Request, Task<Response>>)` existiert für zukünftige async Handler. Der aktuelle Demo-Server nutzt noch den sync Overload. Wenn die async-Migration von `HandleStringRequest` (aktuell `Func<string, Task<string>>`) zu einem echten async Dispatch-Modell folgt, ist die Server-Seite bereit.

### 5.2 `Dict`-Eingabe fehlt

`JipClient` unterstützt `Response.Dict` (via `SendDictAsync`), aber keinen `Request.Dict` als Eingabe. Bisher kein Bedarf — wenn Dict-basierte Requests kommen, wäre ein `SendAsync(..., Dictionary<string,string> dict)` Overload der natürliche Ergänzungspunkt.

### 5.3 Kein Interface für `JipClient`/`JipServer`

Bewusst weggelassen — die Helfer stehen außerhalb des Vertrags. Kein `IJipClient`. Wenn sich die API stabilisiert hat, kann eine Interface-Dokumentation à la `IJipProtocol` nachgeliefert werden, ist aber nicht dringend.

### 5.4 Transport-Agnostizität (langfristig)

Wenn ein zweiter Transport folgt, müsste `JipClient.SendCoreAsync` einen abstrahierten `Func<string, Task<Result<string>>>` statt `ClientPipes` + `PipesClient` direkt verwenden. Die Änderung wäre lokal und nicht breaking für X-JIP-Aufrufer.

---

## 6. Status

| Komponente | Status |
|------------|--------|
| `JipClient.cs` | ✅ Implementiert, gebaut, warning-frei |
| `JipServer.cs` | ✅ Implementiert, gebaut, warning-frei |
| Demo-Client refactored | ✅ `SendAndPrint` entfernt |
| Demo-Server refactored | ✅ Dispatch per `WrapHandler` |
| Build | ✅ 0 Errors, 0 Warnings (Release) |
| Tests | — (keine Tests für Helfer vorgesehen; Integration über Demo) |
