using System.Diagnostics.CodeAnalysis;

namespace JOSYN.Core.IPC.JIP;

/// <summary>
/// Vertragsdefinition für eine ausgehende JIP-Antwort.
/// Beschreibt ausschließlich die Datenform (Wire Format) — keine Serialisierungslogik.
/// </summary>
public interface IResponse
{
    /// <summary>
    /// Status der Antwort: Erfolg, fachlicher Fehler oder technischer Fehler.
    /// </summary>
    ResponseStatus Status { get; init; }

    /// <summary>
    /// <see langword="true"/>, wenn <see cref="Status"/> einen Fehler signalisiert
    /// und <see cref="Error"/> gesetzt ist.
    /// </summary>
    [MemberNotNullWhen(true, nameof(Error))]
    bool HasError => Status is ResponseStatus.TechnicalFailure or ResponseStatus.LogicalFailure;

    /// <summary>
    /// Fehlermeldung; nur gesetzt wenn <see cref="HasError"/> <see langword="true"/> ist.
    /// </summary>
    string? Error { get; init; }

    /// <summary>
    /// Optionaler Nutzlast-String (z. B. serialisiertes Ergebnis).
    /// </summary>
    string? Data { get; init; }

    /// <summary>
    /// Optionales Key-Value-Dictionary als strukturierte Ergebnis-Alternative zu <see cref="Data"/>.
    /// </summary>
    Dictionary<string, string>? Dict { get; init; }
}