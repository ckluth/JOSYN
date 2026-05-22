using JOSYN.Core.ResultPattern;

namespace JOSYN.Core.IPC.JIP;

/// <summary>
/// Vertragsdefinition für den JIP-Konventions-Layer.
/// Trennt das Wire Format (<see cref="Request"/>, <see cref="Response"/>) von der
/// Implementierungsebene (<see cref="Result{TValue}"/> mit <c>string?</c>).
/// </summary>
public interface IJipProtocol
{
    // -------------------------------------------------------------------------
    // Parsing (Exceptions → Result)
    // -------------------------------------------------------------------------

    /// <summary>
    /// Deserialisiert einen rohen JSON-String in ein <see cref="Request"/>-Objekt.
    /// </summary>
    static abstract Result<Request> ParseRequest(string raw);

    /// <summary>
    /// Deserialisiert einen rohen JSON-String in ein <see cref="Response"/>-Objekt.
    /// </summary>
    static abstract Result<Response> ParseResponse(string raw);

    // -------------------------------------------------------------------------
    // Server-Seite: Result<string?> → Response
    // -------------------------------------------------------------------------

    /// <summary>
    /// Erzeugt eine <see cref="Response"/> aus einem <see cref="Result{TValue}"/> mit
    /// optionalem String-Payload. Der Applikations-Layer ist für die Serialisierung des
    /// Wertes in diesen String verantwortlich.
    /// </summary>
    static abstract Response ToResponse(Result<string?> result);

    // -------------------------------------------------------------------------
    // Client-Seite: Response → Result<string?>
    // -------------------------------------------------------------------------

    /// <summary>
    /// Konvertiert eine <see cref="Response"/> in ein <see cref="Result{TValue}"/> mit
    /// optionalem String-Payload. Der Applikations-Layer ist für die Deserialisierung
    /// des Wertes verantwortlich.
    /// </summary>
    static abstract Result<string?> ToResult(Response response);
}
