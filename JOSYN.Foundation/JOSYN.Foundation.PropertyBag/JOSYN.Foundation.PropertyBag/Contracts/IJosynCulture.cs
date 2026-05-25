using System.Globalization;

#pragma warning disable IDE0130
namespace JOSYN.Foundation.PropertyBag;
#pragma warning restore IDE0130

/// <summary>
/// Vertragsdefinition für die kanonische JOSYN-Prozesskultur.
/// Legt die Kultur fest, die für alle Serialisierungen (PropertyBag INI/JSON,
/// Zahlen, Datumsangaben) im gesamten JOSYN-Ökosystem verwendet wird.
/// </summary>
/// <remarks>
/// Die Kultur ist zur Kompilierzeit fest verdrahtet — niemals als Laufzeitkonfiguration änderbar,
/// da eine Kulturdiskrepanz zwischen Schreiber und Leser zu stiller Datenkorrumpierung führt.
/// </remarks>
public interface IJosynCulture
{
    /// <summary>
    /// Die kanonische Kultur aller JOSYN-Prozesse. Aktuell <c>de-DE</c>.
    /// </summary>
    static abstract CultureInfo Default { get; }
}
