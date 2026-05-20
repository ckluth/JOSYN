#pragma warning disable IDE0130
namespace JOSYN.Core.IPC.JIP;
#pragma warning restore IDE0130

/// <summary>
/// Beschreibt die Fehlerarten einer JIP-Antwort.
/// </summary>
public enum ResponseStatus
{
    /// <summary>
    /// Die Funktion wurde erfolgreich ausgeführt.
    /// </summary>
    Success = 0,

    /// <summary>
    /// Ein technischer Fehler hat die Ausführung verhindert (z. B. Serialisierungsfehler, Exception).
    /// </summary>
    TechnicalFailure = 1,

    /// <summary>
    /// Ein fachlicher Fehler ist aufgetreten (z. B. ungültige Eingabe, Regel verletzt).
    /// </summary>
    LogicalFailure = 2,
}