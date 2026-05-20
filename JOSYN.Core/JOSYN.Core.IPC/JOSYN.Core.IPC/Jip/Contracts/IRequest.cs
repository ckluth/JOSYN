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
    /// Optionaler Nutzlast-String (z. B. serialisierter Parameter).
    /// </summary>
    string? Data { get; init; }

    /// <summary>
    /// Optionales Key-Value-Dictionary als strukturierte Parameter-Alternative zu <see cref="Data"/>.
    /// </summary>
    Dictionary<string, string>? Dict { get; init; }
}