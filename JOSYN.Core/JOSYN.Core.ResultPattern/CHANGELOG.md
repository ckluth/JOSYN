# Changelog

Alle relevanten Änderungen an diesem Paket werden hier dokumentiert.  
Format orientiert sich an [Keep a Changelog](https://keepachangelog.com/de/1.0.0/).

---

## [1.0.0-preview01] — 2026-05-20

Erste stabile Kandidatenversion. Das Paket gilt als produktionsreif für den internen
Einsatz; die Preview-Kennzeichnung spiegelt den noch offenen Abnahme-Prozess wider.

### Hinzugefügt

- `Result` (void) und `Result<T>` (generisch) mit vollständiger Implementierung
  des Result-Patterns
- Automatischer Callstack-Aufbau über `Propagate()`
- `Error`-Hilfstyp für idiomatische Fehlerrückgabe (`return Result.Error("...")`)
- Implizite Konvertierungen: `Exception → Result`, `Error → Result`, `T → Result<T>`
- `ToResult()` / `ToResult<T>()` für typübergreifende Fehlerweiterleitung
- Vollständige XML-Dokumentation aller öffentlichen Member (IntelliSense)
- 113 Unit-Tests (NUnit 4.x), alle grün
- `README.md` mit Schnellstart, Kernkonzepten und Referenztabelle
- Build-Skripte unter `.local-build\`
- Nullable Reference Types durchgängig aktiviert
