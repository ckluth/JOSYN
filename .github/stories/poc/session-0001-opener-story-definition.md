# session opener

## topic

poc

## short-description

story-definition

## basic instructions

- read, analyze and understand this opener.
- do not generate code or similar artefacts.
- create a precise summary of your understanding.
- for the human: the summary should allow me to check whether our shared understanding is sufficient for further progress or if I need to refine it.
- for the AI: the summary should enable you to develop a plan for a step-by-step implementation in future sessions.
- keep in mind: we will proceed in separate steps in several sessions. Each step shall be consolidated before proceeding.

## session content

### high-flight-goals  (not for this session, but for the story)

- Der JOSYN-POC soll zu einem Reifegrad entwickelt werden, dass er zu seinem eigentlichen Verwendungszweck (siehe "poc-purpose") eingesetzt werden kann.
- Eine sehr gute Abschlussdokumentation soll erstellt werden, damit die nächsten Schritte klar definiert sind.
- Logischer Abschluss: das gesamte Meta-Repo "JOSYN" soll "logisch archiviert" werden können, d.h. einen "frozen state" erreichen.

### poc-purpose

- Grundlage für show-and-tell mit Kollegen
- Demonstration der Kern-Ideen
- Dokumentierte Basis für die weitere Evolution / für nächste Milestones
- Nicht nur Wegwerfcode: Bereitstellung von "reifen Implementierungen" einzelner Building-Blocks für die Weiterentwicklung.

### my ideas

JOSYN soll in vier "logische Aspekte" unterteilt werden:

1. JOSYN.Foundation
2. JOSYN.Contract
3. JOSYN.Frontend
4. JOSYN.Backend


#### JOSYN.Foundation

- JOSYN.Foundation hat keine Dependencies zu den anderen Aspekten, sondern stellt die "Basis" dar.
- JOSYN.Foundation ist nicht "JOSYN-spezifisch", sondern könnte auch in anderen Projekten eingesetzt werden.
- JOSYN.Foundation ist immer abwärtskompatibel
- JOSYN.Foundation soll rock-stable bleiben; im Idealfall für immer eine "1.0.0"

- JOSYN.Foundation hat verschiedene "Builing-Blocks", die in eigenen Repos leben und eine eigene Verionierung haben könnnen.
- Es gibt beschriebene Abhängigkeiten zwischen den Building-Blocks.
- Jeder "Builing-Block" generiert als Artefact ein eigenes sauberes, robustes Nuget-Package. 


#### JOSYN.Foundation Building-Blocks  

Es gibt genau drei Building-Blocks in JOSYN.Foundation - mit unterschieldichem Gewicht und Charakter

1. JOSYN.Foundation.Resultpattern
2. JOSYN.Foundation.PropertyBag
3. JOSYN.Foundation.JIP


##### JOSYN.Foundation.Resultpattern

- Das JOSYN.Foundation.Resultpattern ist vollständig autark.
- Es wird "überall referenziert".
- Es muss extrem "non-volatile" sein.
- Soll als Nuget-Paket eine hohe Reife erreichen. (Readme, Meta-Informationen, XML-Kommentare, ...)

Aktuelle Impelemtierung: "JOSYN.Core.ResultPattern"

##### JOSYN.Foundation.PropertyBag

- Referenziert nur das ResultPattern
- Ist eher leichtgewichtig
- Soll auch "non-volatile" sein
- Kann prinzipiell von überall referenziert werden.
- Hat keinen Contract-Charakter, sondern ist eher ein "Utility"
- Wird nur gebraucht außerhalb von JOSYN.Foundation; in den Implementierungen von JAP-Server und JAP-Client. (siehe dort)
- Soll als Nuget-Paket eine hohe Reife erreichen. (Readme, Meta-Informationen, XML-Kommentare, ...)

Aktuelle Implementierung: "JOSYN.Core.PropertyBag"

##### JOSYN.Foundation.JIP

- Das ist der "größte Brocken" in der "JOSYN.Foundation"
- Aktuelle Implementierung: "JOSYN.Core.IPC"
- Soll in Zukunft unter "JIP" "firmieren". 
- Referenziert nur das ResultPattern
- Ist der schwergweichtigste Member der Foundation. 
- Soll auch "non-volatile" sein 
- Kann prinzipiell von überall referenziert werden.
- Soll als Nuget-Paket eine hohe Reife erreichen. (Readme, Meta-Informationen, XML-Kommentare, ...)

Soll weiterhin die beiden Demo-Projekte (Client und Server) enthalten, die aber nie "public" sind, sonder nur für Dokumentationszweck haben.


#### JOSYN.Contract

- Ein kleinerer Part im gesamten POC.
- Definiert das "JAP" als Contract. Das High-Level-JIP-Protocl für das spätere JOSYN-System.
- Enthält hier nur Demo-Members. Das reicht auch für den POC.
- Aktuell in JOSYN.JAP zu finden.
- Hier gefällt mir nur das gesamte Naming nicht (Namespace/Artifact/...) und die Position in der Gesamtstruktur.
- Inhaltlich können wir das erstmal eins zu eins übernehmen.
- Außerdem ist es noch sehr "dirty" für öffemtliches NugetPaket...
- Soll als Nuget-Paket eine hohe Reife erreichen. (Readme, Meta-Informationen, XML-Kommentare, ...)

### JOSYN.Frontend

Aktuelle Implementierung: "JOSYN.JobHost"

Referenziert:
	- JOSYN.Foundation
	- JOSYN.JAP

Soll im POC vom Charakter her nur eine JAP-Client-Implementierung darstellen. 

Diese Projekt enthält:  
- JOSYN.JobHost => soll in Zukunft das zentrale NuGet-Paket für die Job-ENtwicklung darstellen.
	- Keep the purpose: größtmögliche Entkopplung von Logik und Dependencies zum Backend (daher: JIP)

- Eine Demo-Job-Implementierung

### JOSYN.Backend

Aktuelle Implementierung: JOSYN.System.JAPServer

Soll im POC vom Charakter her nur eine Implementierung eines JAP-Servers darstellen. 

Stellt eine erste DUMMY-Implementierung fpr JAP bereit.

Ist aktuell weitgehend eine Kopie des DemoServers aud dem IPC-Project.

Referenziert:
	- JOSYN.Foundation
	- JOSYN.JAP

## desired session artifacts

no extra artifacts

## session result

a good summary of this session, that allows an easy evaluation and proceeding in the next sessions




