# JOSYN.Core.PropertyBag

- PropertyBag ist ein Serializer/Deserializer für ein logisches Dictionary<string,string> das <Name,Value>-Paare darstellt, die Properties eines flachen Records oder Aufrufargumente beschreiben. 
- Aktuell werden die Formate Sectionless-Ini oder Json unterstützt (auto-detect)
- Das Result-Pattern wird durchgängig eingesetzt.

## Artifact

JOSYN.Core.PropertyBag.n.n.n.nupkg

## Supported Types (in ``SupportedPropertyTypes.cs``)

- string
- bool
- char
- byte
- sbyte
- short
- ushort
- int
- uint
- long
- ulong
- float
- double
- decimal
- DateTime
- TimeSpan
- DateOnly
- TimeOnly
- Guid

## Purpose

- Primär für den internen Einsatz in JOSYN

## Referenced by

- JOSYN.JobRunner

## References

- JOSYN.Core.ResultPattern
