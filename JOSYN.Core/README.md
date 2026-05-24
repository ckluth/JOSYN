# JOSYN.Core

- JOSYN.Core ist ein logische Multi-Repo im physischen Multi-Repo JOSYN

## Contains

Enthält die logischen Repos:

- JOSYN.Core.IPC  => JOSYN.Core.JIP
- JOSYN.Core.JAP
- JOSYN.Core.PropertyBag
- JOSYN.Core.ResultPattern
- JOSYN.Core.Helpers (?)

## Purpose


JOSYN.Core erzeugt selbst kein Artefact; die Produkte in diesem JOSYN.Core werden als einzelne NuGet-Pakete bereitgestelt.

JOSYN.Core ist ein *Namespace*

JOSYN.Core beschreibt eine logische Kategorie:

Stellt grundlegende Kern-Funktionalität für JOSYN bereit.

- Volatilität:
	- Bleibt stabil
	- By nature keine Feature-Evolution

- Dependencies: 
	- Kein Dependencies auf JOSYN.System oder JOSYN.JobHost
	- Wird von JOSYN.System und JOSYN.JobHost referneziert.
	
- Versionierungsstrategie:
	- Keine holistische Versionierung
	- Jedes Paket hat seine eigene, unabhängige Versionshisorie (im Idealfall bleiben alle auf 1.0.0)

	


