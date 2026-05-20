using System.Text.Json;
using JOSYN.Core.ResultPattern;

namespace JOSYN.Core.IPC.JIP;

/// <summary>
/// Implementierung des JIP-Konventions-Layers.
/// Vermittelt zwischen dem Protokoll-Vertrag (<see cref="Request"/>, <see cref="Response"/>)
/// und der Implementierungsebene (<see cref="Result"/>, <see cref="Result{TValue}"/>).
/// </summary>
public class JipProtocol : IJipProtocol
{
    // -------------------------------------------------------------------------
    // Parsing
    // -------------------------------------------------------------------------

    /// <inheritdoc/>
    public static Result<Request> ParseRequest(string raw)
    {
        try
        {
            var request = JsonSerializer.Deserialize<Request>(raw);
            if (request is null)
                return Result<Request>.Fail("Anfrage konnte nicht deserialisiert werden.");

            return request;
        }
        catch (Exception ex)
        {
            return ex;
        }
    }

    /// <inheritdoc/>
    public static Result<Response> ParseResponse(string raw)
    {
        try
        {
            var response = JsonSerializer.Deserialize<Response>(raw);
            if (response is null)
                return Result<Response>.Fail("Antwort konnte nicht deserialisiert werden.");

            return response;
        }
        catch (Exception ex)
        {
            return ex;
        }
    }

    // -------------------------------------------------------------------------
    // Server-Seite: Result → Response
    // -------------------------------------------------------------------------

    /// <inheritdoc/>
    public static Response ToResponse(Result result)
    {
        if (result.Succeeded)
            return new Response { Status = ResponseStatus.Success };

        return new Response
        {
            Status = ResponseStatus.TechnicalFailure,
            Error  = result.ErrorMessage,
        };
    }

    /// <inheritdoc/>
    public static Response ToResponse<T>(Result<T> result, Func<T, string> serializeData)
    {
        if (!result.Succeeded)
            return new Response
            {
                Status = ResponseStatus.TechnicalFailure,
                Error  = result.ErrorMessage,
            };

        try
        {
            return new Response
            {
                Status = ResponseStatus.Success,
                Data   = serializeData(result.Value),
            };
        }
        catch (Exception ex)
        {
            return new Response
            {
                Status = ResponseStatus.TechnicalFailure,
                Error  = $"Serialisierung des Rückgabewerts fehlgeschlagen: {ex.Message}",
            };
        }
    }

    /// <inheritdoc/>
    public static Response ToLogicalFailureResponse(string message) =>
        new() { Status = ResponseStatus.LogicalFailure, Error = message };

    // -------------------------------------------------------------------------
    // Client-Seite: Response → Result
    // -------------------------------------------------------------------------

    /// <inheritdoc/>
    public static Result ToResult(Response response) => !response.HasError ? Result.Success : Result.Fail(response.Error);

    /// <inheritdoc/>
    public static Result<T> ToResult<T>(Response response, Func<string, T> deserializeData)
    {
        if (response.HasError)
            return Result<T>.Fail(response.Error);

        if (response.Data is null)
            return Result<T>.Fail("Kein Datenwert in der Antwort vorhanden.");

        try
        {
            return deserializeData(response.Data);
        }
        catch (Exception ex)
        {
            return ex;
        }
    }
}
