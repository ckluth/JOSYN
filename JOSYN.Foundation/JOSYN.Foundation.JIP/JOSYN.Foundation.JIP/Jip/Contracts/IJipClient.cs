using JOSYN.Foundation.ResultPattern;

#pragma warning disable IDE0130
namespace JOSYN.Foundation.JIP;
#pragma warning restore IDE0130

/// <summary>
/// Vertragsdefinition für den JIP-Konventions-Client.
/// Kapselt die vollständige Send/Receive-Pipeline: Request-Erstellung, Transport,
/// Response-Parsing und Konvertierung in <see cref="Result{TValue}"/> mit <c>string?</c>.
/// </summary>
public interface IJipClient
{
    /// <summary>
    /// Sendet eine Anfrage und gibt das Ergebnis als <see cref="Result{TValue}"/> mit
    /// optionalem String-Payload zurück.
    /// </summary>
    /// <param name="pipes">Verbindung zum Server.</param>
    /// <param name="what">Bezeichner der aufzurufenden Funktion.</param>
    /// <param name="data">Optionaler Nutzlast-String.</param>
    /// <returns>
    /// Antwort-Payload bei Erfolg; Fehler, wenn Transport, Parsing oder
    /// die Server-seitige Verarbeitung fehlschlägt.
    /// </returns>
    static abstract Task<Result<string?>> SendAsync(ClientPipes pipes, string what, string? data = null);
}
