# JOSYN.System.Contract

**JOSYN.System.Contract** definiert den Applikations-Vertrag zwischen den beiden
JOSYN-Systemkomponenten: dem **JobHost** (Frontend) und dem **JAPServer** (Backend).

Dieser Vertrag ist transportunabhängig — er beschreibt ausschließlich, *was* ausgetauscht
wird, nicht *wie* (das ist Aufgabe von `JOSYN.Foundation.JIP`).

---

## Überblick

Das JOSYN Application Protocol (JAP) definiert einen minimalen, rohen Datenaustausch:

```
JobHost                              JAPServer
   │── GetRawArguments() ──────────►  │  (Job-Auftrag abholen)
   │◄── PutRawResult(result) ─────────│  (Ergebnis zurückmelden)
```

Der Vertrag ist bewusst auf zwei Methoden reduziert. Serialisierung und Deserialisierung
liegen im Verantwortungsbereich der jeweiligen Implementierung.

---

## Schnellstart

```csharp
// Implementierung im JobHost (Frontend)
public class MyJobHostProtocol : IJosynApplicationProtocol
{
    public async Task<Result<string>> GetRawArguments()
    {
        // Argumente aus dem Transport-Layer abrufen
        var result = await JipClient.SendAsync(pipes, "GET-ARGS");
        if (!result.Succeeded) return Result<string>.Propagate(result.ToResult<string>());
        return result.Value ?? string.Empty;
    }

    public async Task<Result> PutRawResult(string result)
    {
        // Ergebnis über den Transport-Layer zurückmelden
        var response = await JipClient.SendAsync(pipes, "PUT-RESULT", result);
        if (!response.Succeeded) return Result.Propagate(response.ToResult());
        return Result.Success;
    }
}
```

---

## Vertrag

### `IJosynApplicationProtocol`

| Methode | Beschreibung |
|---|---|
| `GetRawArguments()` | Ruft serialisierte Job-Argumente als String ab |
| `PutRawResult(string)` | Übermittelt das serialisierte Job-Ergebnis |

---

## Für Maintainer

### Voraussetzungen

.NET 10 SDK, C# (latest)

### Bauen, Testen, Packen

Ausführen aus dem Repo-Wurzelverzeichnis (`JOSYN.System.Contract\`):

```
.local-build\build.cmd          # Release-Build
.local-build\build.cmd Debug    # Debug-Build
.local-build\pack.cmd           # NuGet-Paket erzeugen → ..\..\Local Packages\
```

### Abhängigkeiten

- `JOSYN.Foundation.ResultPattern` (lokaler NuGet-Feed)

### Referenced by

- `JOSYN.System.Frontend` (JobHost)
- `JOSYN.System.Backend` (JAPServer)

### Projektstruktur

```
JOSYN.System.Contract\
└── IJosynApplicationProtocol.cs    # JAP-Vertrag
```

---

*JOSYN.System.Contract — © 2026 HAEVG AG — MIT License*
