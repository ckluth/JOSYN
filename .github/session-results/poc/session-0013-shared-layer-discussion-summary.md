# Session 0013 — JOSYN.System.Shared: Diskussion und Umsetzung

**Story:** poc  
**Session:** 0013  
**Type:** summary  
**Date:** 2026-05-25  

---

## Ausgangslage

Zwei offene Wunden im PoC:

1. **`FakeCore.cs` (Frontend/JobHost):** Platzhalter-Klasse, die schon die richtige semantische
   Unterscheidung kannte (Pipe-Fehler → nur lokal; Job-Fehler → lokal + remote), aber beide
   Pfade mit identischem `Console.WriteLine` implementiert hatte. Kein echter Log, kein echter
   Remote-Report.

2. **`#region Console Helpers` in `Host.cs` (Backend/JAPServer):** Spiegelbildliches Problem.
   `PrintHandlerError`, `TerminateWithSuccess`, `LogErrorResult`, `LogError` — alles
   Konsolen-Farbhacks als Platzhalter für einen echten Mechanismus.

**Kernfrage:** Wohin gehört der gemeinsame Logger, wenn er irgendwann um Rotation/Cleanup
erweitert wird?

---

## Architektur-Diskussion: Wohin mit dem Logger?

### Verworfen: `JOSYN.Foundation.Logging`
Foundation-Pakete haben keine JOSYN-Domain-Semantik. Ein EXE-Prozess-Logger, der
Fehlerpfade mit dem JAPServer-Protokoll kennt, ist kein Foundation-Primitive.
Foundation soll stabil bleiben.

### Verworfen: Logging in `JOSYN.System.Contract`
`Contract` hat einen präzisen semantischen Wert — API-Definition zwischen zwei Prozessen.
Utility-Code darin würde den Namen zur Lüge machen.

### Entschieden: Neue Schicht `JOSYN.System.Shared`

`JOSYN.System.Shared` als dritte Gruppierungs-Schicht neben `Frontend` und `Backend`.
Dieselbe Strukturregel wie die anderen beiden: reine Namespace-Container-Schicht, konkrete
Projekte eine Ebene tiefer mit vollqualifiziertem Namen.

```
JOSYN.System/
  JOSYN.System.Frontend/        ← Gruppierungsschicht
    JOSYN.System.Frontend.JobHost/
  JOSYN.System.Backend/         ← Gruppierungsschicht
    JOSYN.System.Backend.JAPServer/
  JOSYN.System.Shared/          ← NEU: Gruppierungsschicht
    JOSYN.System.Shared.Contract/  ← Nachfolger von JOSYN.System.Contract
    JOSYN.System.Shared.Log/       ← NEU
```

**Begründung:** `Shared` trifft semantisch exakt den Inhalt — Dinge, die beide EXE-Prozesse
gemeinsam nutzen. Es skaliert: wenn ein dritter gemeinsamer Aspekt (Config, Metrics, …)
hinzukommt, bekommt er einfach einen weiteren Platz unter `Shared`.

**Jetzt der richtige Moment:** Umbenennung `Contract` → `Shared.Contract` kostet im PoC
null (zwei referenzierende Projekte, keine externen Konsumenten). Nach dem Freeze wäre es
eine Breaking-Change-Migration.

---

## Abhängigkeitsgraph (unverändert sauber)

```
JOSYN.Foundation.*
       ↓
JOSYN.System.Shared.*    (Shared.Contract + Shared.Log)
       ↓
JOSYN.System.Frontend.*  /  JOSYN.System.Backend.*
```

---

## Was umgesetzt wurde

### 1. `JOSYN.System.Shared` — Scaffold
- `JOSYN.System.Shared.slnx` (enthält beide Projekte)
- Standard-Scaffold: `Directory.Build.props`, `nuget.config`, `.local-build/`-Skripte
- `pack.cmd` packt beide Projekte in einem Schritt

### 2. `JOSYN.System.Shared.Contract` (Nachfolger von `JOSYN.System.Contract`)
- Namespace: `JOSYN.System.Shared.Contract`
- `IJosynApplicationProtocol`: **dritte Methode `PutError(string serializedError)` ergänzt**
- Neuer `ErrorReport`-Record:
  ```csharp
  record ErrorReport(string Message, string? CallStack, string? ExceptionDetails, DateTimeOffset OccurredAt);
  ```
  Wird via PropertyBag (INI) serialisiert, bevor er über `PutError` übertragen wird.
- README vollständig neu geschrieben (Fehlerrouting-Prinzip dokumentiert)

### 3. `JOSYN.System.Shared.Log`
- `public static class LocalLog`
- Log-Pfad: `%TEMP%\JOSYN\<ProcessName>\<yyyy-MM-dd>.log`
- Sofort-Flush (append), kein Puffer — Crash-Sicherheit
- Im Debug-Build: zusätzliche Konsolenausgabe (farbig)
- Schreibfehler werden stillschweigend ignoriert (Logger darf Host nie zum Absturz bringen)
- API:
  ```csharp
  LocalLog.Error(string message, string? callStack = null, string? exceptionDetails = null);
  LocalLog.Error(Result result);   // Convenience-Überladung
  LocalLog.Info(string message);
  ```

### 4. Backend — `Host.cs` und `JAPServer.cs`
- `#region Console Helpers` vollständig eliminiert
- `Host.cs`: alle Log-Aufrufe verwenden jetzt `LocalLog`
- `JAPServer.cs`: `PutError` implementiert — loggt via `LocalLog.Error`
- NuGet-Refs aktualisiert: `JOSYN.System.Contract` → `Shared.Contract` + `Shared.Log`

### 5. Frontend — `Core.cs`, `JAPClient.cs`, `FakeCore.cs`
- `FakeCore.cs` **gelöscht**
- `Core.cs` neu: explizites Fehler-Routing:
  ```
  Pipe-Fehler          →  LocalLog.Error(...)                        (nur lokal)
  Job-Fehler           →  LocalLog.Error(...) + japClient.PutError() (lokal + remote)
  PutError fehlgeschl. →  LocalLog.Error(...) Fallback               (nur lokal)
  ```
  Serialisierung des `ErrorReport` via PropertyBag (schon verfügbar in Frontend)
- `JAPClient.cs`: `PutError` JIP-Aufruf implementiert (analog zu `PutRawResult`)
- NuGet-Refs aktualisiert: `JOSYN.System.Contract` → `Shared.Contract` + `Shared.Log`

### 6. Root `build-all.cmd`
- Schritt 4 ersetzt: `JOSYN.System.Contract` → `JOSYN.System.Shared` (baut beide Projekte,
  packt Contract + Log getrennt)
- Zähler-Text angepasst: "6 Sub-Repos / 7 NuGet-Pakete"

### 7. `.gitignore`
- Exception für `JOSYN.System.Shared.Log/` ergänzt: `*.log`-Pattern in der
  Visual-Studio-Gitignore trifft auf Windows den Ordnernamen (case-insensitive)

---

## Offene Punkte (nach PoC-Freeze)

| # | Thema |
|---|---|
| 1 | `JOSYN.System.Contract` (alt) ist noch im Repo — kein Code mehr referenziert es; Tombstone oder löschen? |
| 2 | `LocalLog`: Rotation/Cleanup-Mechanismus (tägliche Dateien vorhanden, aber kein Aufräumen) |
| 3 | `LocalLog`: Konfigurierbare Log-Pfade |
| 4 | `JAPServer.PutError`: aktuell nur LocalLog — in einer späteren Phase: speichern + verarbeiten |
| 5 | `ErrorReport`-Deserialisierung im Backend (aktuell verbatim geloggt, nicht strukturiert ausgewertet) |
| 6 | `FakeReadArgumentsFromFile` in `JAPServer` ist weiterhin Fake — bewusst offen für PoC |

---

*Session 0013 — 2026-05-25 — poc/evolution*
