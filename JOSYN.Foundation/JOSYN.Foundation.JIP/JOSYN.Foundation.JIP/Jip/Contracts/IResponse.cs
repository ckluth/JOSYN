using System.Diagnostics.CodeAnalysis;

namespace JOSYN.Foundation.JIP;

/// <summary>
/// Vertragsdefinition für eine ausgehende JIP-Antwort.
/// Beschreibt ausschließlich die Datenform (Wire Format) — keine Serialisierungslogik.
/// </summary>
public interface IResponse
{
    /// <summary>
    /// <see langword="true"/> bei Erfolg. Wenn <see langword="false"/>, ist
    /// <see cref="Error"/> garantiert gesetzt.
    /// </summary>
    [MemberNotNullWhen(false, nameof(Error))]
    bool Succeeded { get; init; }

    /// <summary>
    /// Fehlermeldung; nur gesetzt wenn <see cref="Succeeded"/> <see langword="false"/> ist.
    /// </summary>
    string? Error { get; init; }

    /// <summary>
    /// Optionaler Nutzlast-String. Der Applikations-Layer ist für Interpretation und
    /// Deserialisierung verantwortlich.
    /// </summary>
    string? Data { get; init; }
}