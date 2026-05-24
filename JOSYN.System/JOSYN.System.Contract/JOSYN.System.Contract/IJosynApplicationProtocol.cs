using JOSYN.Foundation.ResultPattern;

namespace JOSYN.System.Contract;

/// <summary>
/// Vertragsdefinition für das JOSYN Application Protocol (JAP).
/// Beschreibt die Kommunikation zwischen <b>JobHost</b> (Frontend) und
/// <b>JAPServer</b> (Backend) auf Applikationsebene — unabhängig vom
/// Transportmechanismus.
/// <para>
/// Der Aufrufer (<c>JobHost</c>) ruft <see cref="GetRawArguments"/> ab,
/// um den Job-Auftrag zu empfangen, führt den Job aus und übermittelt
/// das Ergebnis über <see cref="PutRawResult"/>.
/// </para>
/// </summary>
public interface IJosynApplicationProtocol
{
    /// <summary>
    /// Ruft die serialisierten Job-Argumente als rohen String ab.
    /// Der Aufrufer ist für die Deserialisierung verantwortlich.
    /// </summary>
    /// <returns>
    /// Serialisierter Argumente-String bei Erfolg;
    /// Fehler, wenn keine Argumente verfügbar sind oder der Transport fehlschlägt.
    /// </returns>
    Task<Result<string>> GetRawArguments();

    /// <summary>
    /// Übermittelt das Job-Ergebnis als serialisierten String.
    /// Der Aufrufer ist für die Serialisierung verantwortlich.
    /// </summary>
    /// <param name="result">Serialisiertes Job-Ergebnis.</param>
    /// <returns>
    /// Erfolgreich, wenn das Ergebnis übermittelt wurde;
    /// Fehler, wenn der Transport fehlschlägt.
    /// </returns>
    Task<Result> PutRawResult(string result);
}
