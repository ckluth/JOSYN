namespace JOSYN.Core.IPC.JIP;

/// <summary>
/// Vertragsdefinition für eine eingehende JIP-Anfrage.
/// Beschreibt ausschließlich die Datenform (Wire Format) — keine Serialisierungslogik.
/// </summary>
public interface IRequest
{
    /// <summary>
    /// Bezeichner der aufzurufenden Funktion (z. B. "GET-CONFIG").
    /// </summary>
    string What { get; init; }

    /// <summary>
    /// Optionaler Nutzlast-String. Der Applikations-Layer ist für Interpretation und
    /// Serialisierung verantwortlich.
    /// </summary>
    string? Data { get; init; }
}