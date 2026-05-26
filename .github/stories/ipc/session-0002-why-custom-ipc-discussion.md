# IPC Discussion — Session 002: Warum JOSYN.Core.IPC?

> **Scope:** Technische Begründung für die Eigenentwicklung — Alternativen-Analyse und Alleinstellungsmerkmal.  
> Kein Code wurde während dieser Diskussion verändert.

---

## Die naheliegende Frage

Named-Pipe-basierte IPC mit typed method dispatch ist keine neue Idee. .NET hat reife Lösungen. Warum also eine eigene Library?

---

## Was es gibt — und warum es nicht passt

### StreamJsonRpc (Microsoft)

Der engste Treffer. Wird intern in Visual Studio und Roslyn eingesetzt — typed method calls over named pipes, async, proxy-generation via interfaces, battle-tested. Technisch exzellent.

**Aber:** Exception-basiert. Jeder Aufruf kann werfen. Für JOSYN bedeutet das: an der Grenze zur Library entsteht zwingend ein `try/catch`-Adapter, der Exceptions in Results übersetzt. Diese Grenzstelle ist eine **architektonische Bruchstelle** — der Callstack ist ab diesem Punkt zerrissen, die Propagation-Kette unterbrochen.

### gRPC (.NET)

Kann lokale IPC. Bringt aber HTTP/2-Transport, Protobuf IDL, Schema-Kompilierung und erhebliches Setup-Overhead mit. Konzeptuell für Netzwerk-RPC designed, nicht für session-isolierte In-Process-Kommunikation.

### CoreWCF

Hat Named-Pipe-Binding. Ist aber WCF — XML/SOAP, DI-heavy, Legacy-Gepäck.

### `System.IO.Pipes` direkt

Exakt das, was JOSYN.Core.IPC intern verwendet — aber nackt. Kein Framing, kein Protokoll, keine Fehlerbehandlung. Man baut dann ohnehin dieselbe Schicht.

---

## Das Alleinstellungsmerkmal

Es geht nicht um den Transport. Named Pipes + Length-Prefix-Framing ist keine hohe Erfindungshöhe. Der Wert liegt an einer anderen Stelle:

**JOSYN.Core.IPC ist Result-Pattern-nativ — ohne Bruchstellen.**

`PipesClient.ConnectAsync` gibt ein `Result` zurück.  
`JipClient.SendAsync` gibt ein `Result` zurück.  
Eine Exception aus dem tiefsten `ReadInt32`-Aufruf propagiert als `Result` mit vollständigem Callstack bis zur Anwendung — ohne dass der Aufrufer wissen muss, wo sie entstanden ist.

Keine andere verfügbare Library liefert das, ohne eine Adapter-Schicht zu erzwingen.

---

## Warum die Adapter-Schicht kein akzeptabler Kompromiss ist

Eine Exception-zu-Result-Übersetzung ist nicht nur Boilerplate — sie **zerstört Information**:

- Der `CallerInfo`-Callstack von JOSYN endet am Adapter. Was darunter passiert ist, ist unsichtbar.
- `Result.Propagate()` kann nicht über die Grenze hinweg akkumulieren.
- Das Tooling (`result.CallStackAsString`) zeigt nur die halbe Wahrheit.

Der Aufrufer sieht: *"Irgendwo ist etwas schiefgelaufen."* JOSYN.Core.IPC zeigt: *"In `PipesClient.ConnectAsync`, aufgerufen aus `JipClient.SendCoreAsync`, aufgerufen aus `JipClient.SendAsync` — hier ist was schiefgelaufen, und hier ist der vollständige Weg dorthin."*

---

## Das Gesamtpaket

Weil alle Schichten dieselbe Sprache sprechen — `Result`, `Propagate`, keine Exceptions — kann `JipClient.SendAsync` so schlank sein wie es ist:

```csharp
var config = await JipClient.SendAsync<string>(pipes, "GET-CONFIG", deserialize);
```

Keine Adapter, keine doppelte Fehlerbehandlung, keine `try/catch`-Grenzen zwischen den Schichten. Das ist die Konsequenz der Kohärenz — nicht eine separate Leistung, sondern das **Ergebnis eines konsistenten Fundaments**.

---

## Fazit

JOSYN.Core.IPC ist gerechtfertigt — nicht wegen der Erfindungshöhe des Transports, sondern weil es das einzige Paket ist, das Named-Pipe-IPC **vollständig im JOSYN-Ökosystem** verankert. Wer StreamJsonRpc nimmt, bekommt eine technisch überlegene Library — und bezahlt mit einer permanenten Bruchstelle in seiner Fehlerbehandlungs-Architektur.
