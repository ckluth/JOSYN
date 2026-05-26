# Session 0006 — JipDispatcher: typsicheres Server-Dispatch

**Date:** 2026-05-24  
**Type:** summary

---

## Status Quo (Ziel dieser Dokumentation)

### Das Problem vorher

Der Server (`Demo.ServerExe/Program.cs`) enthielt einen manuellen `switch` über den `What`-String
jeder eingehenden Anfrage. Konkret:

```csharp
// vorher — fragil, keine Compile-Zeit-Sicherheit:
private static readonly Func<string, Task<string>> _dispatch = JipServer.WrapHandler(async req =>
{
    switch (req.What)
    {
        case "GET-ARGUMENTS": ...
        case "PUT-JOBRESULT": ...
        case "ECHO": ...
        default: return Result<string?>.Fail($"Unbekannte Funktion: '{req.What}'");
    }
});
```

Probleme dabei:
- Literal-Strings: Tippfehler werden erst zur Laufzeit sichtbar
- Client (`JAPClient`) und Server benutzten inkonsistente What-Strings (`"PUT-JOBRESULT"` vs. `"PUT-RESULT"`)
- Jeder neue Protokoll-Eintrag erforderte Änderungen an mehreren Stellen
- Das Ergebnis des `Build()`-Calls (eine `Func`) war anonym und lebte in einem versteckten Closure

---

## Aktuelle Lösung

### `JipDispatcher` — `JOSYN.Core.IPC/Jip/JipDispatcher.cs`

Eine `sealed class` im JIP-Layer, die als **explizit benanntes `static readonly`-Feld** auf der
Server-Seite lebt. Kein Builder-Pattern, kein `Build()`-Aufruf — der Dispatcher *ist* die
fertige Einheit.

**Kernprinzip:** Die Closure über das interne `_handlers`-Dictionary wird einmalig im Konstruktor
gebaut (`_builtDispatch`). `Register`-Aufrufe danach befüllen das Dictionary — die Closure sieht
die Einträge beim Dispatch, weil sie die Referenz hält, nicht eine Kopie.

**Fluent API — typisierte `Register`-Überladungen:**

| Überladung | Passt zu |
|---|---|
| `Func<Task<Result<string>>>` | `Task<Result<string>> GetXxx()` |
| `Func<string, Task<Result>>` | `Task<Result> PutXxx(string data)` |
| `Func<string?, Task<Result<string?>>>` | allgemeiner async Handler |
| `Func<string?, Result<string?>>` | synchroner Handler mit optionalem Data |
| `Result<string?>` (Konstante) | PING, feste Config-Antworten |

**`RegisterAll<TProtocol>(TProtocol impl)`:**  
Iteriert per Reflection alle Methoden von `TProtocol` (dem Interface, nicht der Klasse) und
registriert jeden Eintrag nach Signatur-Muster. `JipDispatcher` selbst kennt kein
Applikationsprotokoll — `TProtocol` ist zur Compile-Zeit offen. Der Aufrufer nennt den konkreten
Interface-Typ explizit:

```csharp
.RegisterAll<IJosynApplicationProtocol>(japServer)
```

Nicht unterstützte Signaturen werfen `InvalidOperationException` beim Start (fail-fast,
nicht zur Laufzeit eines Requests).

**`Dispatch(string requestStr)`:**  
Delegiert an `_builtDispatch`. Unbekannte `What`-Werte → Fehler-Response (kein throw).  
Übergabe an `ServerStartArguments.HandleStringRequest = jipDispatcher.Dispatch`.

---

### Server-Seite `Demo.ServerExe/Program.cs` — aktueller Stand

```csharp
private static readonly JAPServer japServer = new();

private static readonly JipDispatcher jipDispatcher = new JipDispatcher()
    .RegisterAll<IJosynApplicationProtocol>(japServer)
    .Register("PING",       Result<string?>.Success(null))
    .Register("GET-CONFIG", Result<string?>.Success("{ \"version\": \"1.0\", \"mode\": \"demo\" }"))
    .Register("ECHO",       (string? data) => Result<string?>.Success("ECHO " + data));
```

`HandleStringRequest = jipDispatcher.Dispatch` in `ServerStartArguments`.

---

### Client-Seite `JOSYN.JobHost/JAPClient.cs` — What-Strings per `nameof`

```csharp
await JipClient.SendAsync(Pipes, nameof(IJosynApplicationProtocol.GetRawArguments));
await JipClient.SendAsync(Pipes, nameof(IJosynApplicationProtocol.PutRawResult), result);
```

Client und Server referenzieren dasselbe Symbol. Umbenennen der Interface-Methode → Compiler-Fehler
an beiden Stellen.

---

### Sicherheitsnetz — `Demo.ServerExe.Test`

Neues Test-Projekt `JOSYN.Core.IPC.Demo.ServerExe.Test`. Ein Test genügt:

```csharp
[Test]
public void RegisterAll_CoversAllMethodsOf_IJosynApplicationProtocol()
{
    var expectedKeys = typeof(IJosynApplicationProtocol).GetMethods()
        .Select(m => m.Name).ToHashSet();

    var dispatcher = new JipDispatcher()
        .RegisterAll<IJosynApplicationProtocol>(new FakeJosynApplicationProtocol());

    Assert.That(dispatcher.RegisteredKeys, Is.SupersetOf(expectedKeys), ...);
}
```

Neue Methode im Interface mit nicht unterstützter Signatur → Test rot (via `InvalidOperationException`
aus `RegisterAll`). Neue Methode mit unterstützter Signatur, aber vergessen in `RegisterAll` →
kann nicht vergessen werden, da `RegisterAll` per Reflection alle Methoden selbst findet.

---

## Bekannte Dirt-Spots (bewusst liegen gelassen)

- `IJosynApplicationProtocol` ist in `Demo.ServerExe` **und** `JOSYN.JobHost` dupliziert —
  wartet auf das geplante Shared-NuGet-Paket (`// TODO: mit Server sharen`)
- `JAPServer` und `JAPClient` sind Demo-Klassen, noch keine Produktions-Implementierungen

---

## Historischer Kontext (kurz)

Die Dispatch-Vereinfachung war der dritte Refactoring-Schritt in einer kürzeren Session.
Vorab wurde das provisorische `_delegate` durch eine echte `JAPServer`-Instanz ersetzt,
dann ein erster Dispatcher-Prototyp mit `Build()`-Pattern gebaut, der im Laufe der Diskussion
schrittweise entschärft wurde: `Build()` fiel weg, `RegisterAll` wurde erst mit hartverdrahteten
Membernamen implementiert, dann auf Reflection umgestellt nachdem der logische Designgeruch
(JIP kennt JAP) erkannt wurde.
