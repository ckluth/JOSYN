# Session 0007 — JOSYN.Foundation.JIP: Migration von JOSYN.Core.IPC

**Date:** 2026-05-24
**Type:** generation

---

## Goal

Dritter und letzter Schritt der Core→Foundation-Migration:
`JOSYN.Core.IPC` → `JOSYN.Foundation.JIP`

Damit verschwindet `JOSYN.Core` vollständig aus dem Repository.

---

## Changes Made

### Neue Zielstruktur

```
JOSYN.Foundation/
├── JOSYN.Foundation.ResultPattern/   ✅ (previous sessions)
├── JOSYN.Foundation.PropertyBag/     ✅ (previous sessions)
└── JOSYN.Foundation.JIP/             ✅ (this session)
```

### Gelöscht

- `JOSYN.Core\JOSYN.Core.IPC\` — vollständiger Ordner inkl. aller Unterprojekte
- `JOSYN.Core\` — nun leerer Wurzelordner (nur noch README.md) → gelöscht

### Neue Dateien / Ordner (unter `JOSYN.Foundation\JOSYN.Foundation.JIP\`)

Alle Projektordner umbenannt:

| Alt | Neu |
|-----|-----|
| `JOSYN.Core.IPC\` | `JOSYN.Foundation.JIP\` |
| `JOSYN.Core.IPC.Demo.ClientExe\` | `JOSYN.Foundation.JIP.Demo.ClientExe\` |
| `JOSYN.Core.IPC.Demo.ServerExe\` | `JOSYN.Foundation.JIP.Demo.ServerExe\` |
| `JOSYN.Core.IPC.Demo.ServerExe.Test\` | `JOSYN.Foundation.JIP.Demo.ServerExe.Test\` |

Alle Projektdateien umbenannt entsprechend (`.slnx`, `.csproj`).

### Namespace-Mapping

| Alt | Neu |
|-----|-----|
| `JOSYN.Core.IPC` | `JOSYN.Foundation.JIP` |
| `JOSYN.Core.IPC.JIP` | `JOSYN.Foundation.JIP` |
| `JOSYN.Core.IPC.Demo` | `JOSYN.Foundation.JIP.Demo` |
| `JOSYN.Core.IPC.Demo.ServerExe` | `JOSYN.Foundation.JIP.Demo.ServerExe` |
| `JOSYN.Core.IPC.Demo.ServerExe.Test` | `JOSYN.Foundation.JIP.Demo.ServerExe.Test` |

Beide alten Namespaces (`JOSYN.Core.IPC` für die Transport-Schicht und `JOSYN.Core.IPC.JIP`
für die JIP-Konventions-Schicht) wurden auf den gemeinsamen Root `JOSYN.Foundation.JIP` geflacht.
Das gesamte Paket *ist* JIP — eine Sub-Namespace-Trennung ist nicht mehr notwendig.

### `.csproj`-Änderungen (Hauptbibliothek)

- `AssemblyName`: `JOSYN.Core.IPC` → `JOSYN.Foundation.JIP`
- `RootNamespace`: `JOSYN.Core.IPC` → `JOSYN.Foundation.JIP`
- `PackageId`: `JOSYN.Core.IPC` → `JOSYN.Foundation.JIP`
- `PackageReference`: `JOSYN.Core.ResultPattern` → `JOSYN.Foundation.ResultPattern`

### Externe Konsumenten aktualisiert

| Datei | Änderung |
|-------|---------|
| `JOSYN.System.JapServer.Server.csproj` | `JOSYN.Core.IPC` → `JOSYN.Foundation.JIP`, `JOSYN.Core.ResultPattern` → `JOSYN.Foundation.ResultPattern` |
| `JOSYN.System.JapServer.Server\Program.cs` | `using JOSYN.Core.IPC` + `using JOSYN.Core.IPC.JIP` → `using JOSYN.Foundation.JIP` |
| `JOSYN.System.JapServer.Server\JAPServer.cs` | `using JOSYN.Core.ResultPattern` → `using JOSYN.Foundation.ResultPattern` |
| `JOSYN.System.JAPServer\.local-build\pack.cmd` | Projektname korrigiert |
| `JOSYN.JobHost.csproj` | `JOSYN.Core.IPC/PropertyBag/ResultPattern` → Foundation-Varianten |
| `JOSYN.JobHost\JAPClient.cs` | using-Direktiven aktualisiert |
| `JOSYN.JobHost\JobInvoker.cs` | using-Direktiven aktualisiert |
| `JOSYN.JobHost\FakeCore.cs` | using-Direktiven aktualisiert |
| `JOSYN.JAP.Protocol.csproj` | `JOSYN.Core.ResultPattern` → `JOSYN.Foundation.ResultPattern` |
| `JOSYN.JAP.Protocol\IJosynApplicationProtocol.cs` | using-Direktive aktualisiert |

---

## Build Result

`dotnet build JOSYN.Foundation.JIP.slnx --configuration Release` → **Erfolgreich** (0 Fehler, 0 Warnungen)
`dotnet test JOSYN.Foundation.JIP.slnx --configuration Release` → **Bestanden** (1/1 Test)

---

## Historischer Kontext

`JOSYN.Core` war ein logisches Multi-Repo mit drei Bausteinen:
- `JOSYN.Core.ResultPattern` → `JOSYN.Foundation.ResultPattern` (Session zuvor)
- `JOSYN.Core.PropertyBag` → `JOSYN.Foundation.PropertyBag` (Session zuvor)
- `JOSYN.Core.IPC` → `JOSYN.Foundation.JIP` (diese Session)

Mit dieser Session ist die Migration vollständig. `JOSYN.Core` existiert nicht mehr.
