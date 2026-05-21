using JOSYN.Core.ResultPattern;

#pragma warning disable IDE0130
namespace JOSYN.Core.IPC.JIP;
#pragma warning restore IDE0130

/// <summary>
/// Implementierungs-Helfer für JIP-Clients.
/// Kapselt die vollständige Send/Receive-Pipeline: Request-Erstellung, Transport,
/// Response-Parsing und Result-Konvertierung.
/// <para>
/// Steht außerhalb des Protokoll-Vertrags (<see cref="IJipProtocol"/>) und ist
/// ausschließlich für die Implementierung von X-JIP-Client-Protokollen gedacht.
/// </para>
/// </summary>
public static class JipClient
{
    // -------------------------------------------------------------------------
    // Ohne Payload-Rückgabe
    // -------------------------------------------------------------------------

    /// <summary>
    /// Sendet eine Anfrage ohne Eingabe-Payload und ohne Rückgabewert.
    /// </summary>
    public static async Task<Result> SendAsync(ClientPipes pipes, string what)
    {
        var send = await SendCoreAsync(pipes, new Request { What = what });
        if (!send.Succeeded) return send.ToResult();
        return JipProtocol.ToResult(send.Value);
    }

    /// <summary>
    /// Sendet eine Anfrage mit typisiertem Eingabe-Payload, ohne Rückgabewert.
    /// </summary>
    /// <param name="pipes">Verbindung zum Server.</param>
    /// <param name="what">Bezeichner der aufzurufenden Funktion.</param>
    /// <param name="data">Eingabe-Payload.</param>
    /// <param name="serialize">Serialisiert <typeparamref name="TIn"/> in den Payload-String.</param>
    public static async Task<Result> SendAsync<TIn>(
        ClientPipes pipes, string what,
        TIn data, Func<TIn, string> serialize)
    {
        string serialized;
        try { serialized = serialize(data); }
        catch (Exception ex) { return ex; }

        var send = await SendCoreAsync(pipes, new Request { What = what, Data = serialized });
        if (!send.Succeeded) return send.ToResult();
        return JipProtocol.ToResult(send.Value);
    }

    // -------------------------------------------------------------------------
    // Mit typisiertem Rückgabewert (Data-Feld)
    // -------------------------------------------------------------------------

    /// <summary>
    /// Sendet eine Anfrage ohne Eingabe-Payload, mit typisiertem Rückgabewert aus <see cref="Response.Data"/>.
    /// </summary>
    /// <param name="pipes">Verbindung zum Server.</param>
    /// <param name="what">Bezeichner der aufzurufenden Funktion.</param>
    /// <param name="deserialize">Deserialisiert den Payload-String in <typeparamref name="TOut"/>.</param>
    public static async Task<Result<TOut>> SendAsync<TOut>(
        ClientPipes pipes, string what,
        Func<string, TOut> deserialize)
    {
        var send = await SendCoreAsync(pipes, new Request { What = what });
        if (!send.Succeeded) return send.ToResult<TOut>();
        return JipProtocol.ToResult(send.Value, deserialize);
    }

    /// <summary>
    /// Sendet eine Anfrage mit typisiertem Eingabe-Payload und typisiertem Rückgabewert aus <see cref="Response.Data"/>.
    /// </summary>
    /// <param name="pipes">Verbindung zum Server.</param>
    /// <param name="what">Bezeichner der aufzurufenden Funktion.</param>
    /// <param name="data">Eingabe-Payload.</param>
    /// <param name="serialize">Serialisiert <typeparamref name="TIn"/> in den Payload-String.</param>
    /// <param name="deserialize">Deserialisiert den Payload-String in <typeparamref name="TOut"/>.</param>
    public static async Task<Result<TOut>> SendAsync<TIn, TOut>(
        ClientPipes pipes, string what,
        TIn data, Func<TIn, string> serialize,
        Func<string, TOut> deserialize)
    {
        string serialized;
        try { serialized = serialize(data); }
        catch (Exception ex) { return ex; }

        var send = await SendCoreAsync(pipes, new Request { What = what, Data = serialized });
        if (!send.Succeeded) return send.ToResult<TOut>();
        return JipProtocol.ToResult(send.Value, deserialize);
    }

    // -------------------------------------------------------------------------
    // Mit Dict-Rückgabe
    // -------------------------------------------------------------------------

    /// <summary>
    /// Sendet eine Anfrage ohne Eingabe-Payload, mit Key-Value-Rückgabe aus <see cref="Response.Dict"/>.
    /// </summary>
    public static async Task<Result<Dictionary<string, string>>> SendDictAsync(ClientPipes pipes, string what)
    {
        var send = await SendCoreAsync(pipes, new Request { What = what });
        if (!send.Succeeded) return send.ToResult<Dictionary<string, string>>();

        var response = send.Value;
        if (response.HasError)
            return Result<Dictionary<string, string>>.Fail(response.Error);

        if (response.Dict is null)
            return Result<Dictionary<string, string>>.Fail("Kein Dict-Wert in der Antwort vorhanden.");

        return response.Dict;
    }

    // -------------------------------------------------------------------------
    // Privater Kern
    // -------------------------------------------------------------------------

    private static async Task<Result<Response>> SendCoreAsync(ClientPipes pipes, Request request)
    {
        var getRaw = await PipesClient.SendRequestAsync(request.ToString(), pipes);
        if (!getRaw.Succeeded) return getRaw.ToResult<Response>();
        return JipProtocol.ParseResponse(getRaw.Value);
    }
}
