using JOSYN.Core.ResultPattern;

namespace JOSYN.Core.IPC.JIP;

/// <summary>
/// Vertragsdefinition für den JIP-Konventions-Layer.
/// Trennt den Protokoll-Vertrag (<see cref="Request"/>, <see cref="Response"/>) von der
/// Implementierungsebene (<see cref="Result"/>, <see cref="Result{TValue}"/>).
/// </summary>
public interface IJipProtocol
{
    // -------------------------------------------------------------------------
    // Parsing (Robustheit: Exceptions → Result)
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
    // Server-Seite: Result → Response
    // -------------------------------------------------------------------------

    /// <summary>
    /// Erzeugt eine <see cref="Response"/> aus einem <see cref="Result"/> (ohne Rückgabewert).
    /// Erfolg → <see cref="ResponseStatus.Success"/>.
    /// Fehler → <see cref="ResponseStatus.TechnicalFailure"/> mit Fehlermeldung.
    /// </summary>
    static abstract Response ToResponse(Result result);

    /// <summary>
    /// Erzeugt eine <see cref="Response"/> aus einem <see cref="Result{TValue}"/>.
    /// Erfolg → <see cref="ResponseStatus.Success"/> mit serialisiertem Wert in <see cref="Response.Data"/>.
    /// Fehler → <see cref="ResponseStatus.TechnicalFailure"/> mit Fehlermeldung.
    /// </summary>
    static abstract Response ToResponse<T>(Result<T> result, Func<T, string> serializeData);

    /// <summary>
    /// Erzeugt eine <see cref="Response"/> mit <see cref="ResponseStatus.LogicalFailure"/>
    /// für fachliche Fehler, die kein <see cref="Result"/> als Ursache haben.
    /// </summary>
    static abstract Response ToLogicalFailureResponse(string message);

    // -------------------------------------------------------------------------
    // Client-Seite: Response → Result
    // -------------------------------------------------------------------------

    /// <summary>
    /// Konvertiert eine <see cref="Response"/> in ein <see cref="Result"/> (ohne Rückgabewert).
    /// <see cref="ResponseStatus.Success"/> → <see cref="Result.Success"/>.
    /// Fehler-Status → <c>Result.Fail(…)</c> mit der Fehlermeldung der Antwort.
    /// </summary>
    static abstract Result ToResult(Response response);

    /// <summary>
    /// Konvertiert eine <see cref="Response"/> in ein <see cref="Result{T}"/> mit deserialisiertem Wert.
    /// Fehler-Status → <c>Result&lt;T&gt;.Fail(…)</c>.
    /// Kein <see cref="Response.Data"/> bei Erfolg → <c>Result&lt;T&gt;.Fail(…)</c>.
    /// </summary>
    static abstract Result<T> ToResult<T>(Response response, Func<string, T> deserializeData);
}
