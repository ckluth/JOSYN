using JOSYN.Foundation.ResultPattern;

#pragma warning disable IDE0130
namespace JOSYN.Foundation.JIP;
#pragma warning restore IDE0130

/// <summary>
/// Implementierungs-Helfer für JIP-Clients.
/// Kapselt die vollständige Send/Receive-Pipeline: Request-Erstellung, Transport,
/// Response-Parsing und Konvertierung in <see cref="Result{TValue}"/> mit <c>string?</c>.
/// <para>
/// Der Applikations-Layer ist für die Interpretation und Deserialisierung des
/// zurückgegebenen Strings verantwortlich.
/// </para>
/// </summary>
public static class JipClient
{
    /// <summary>
    /// Sendet eine Anfrage und gibt das Ergebnis als <see cref="Result{TValue}"/> mit
    /// optionalem String-Payload zurück.
    /// </summary>
    /// <param name="pipes">Verbindung zum Server.</param>
    /// <param name="what">Bezeichner der aufzurufenden Funktion.</param>
    /// <param name="data">Optionaler Nutzlast-String.</param>
    public static async Task<Result<string?>> SendAsync(ClientPipes pipes, string what, string? data = null)
    {
        var request = new Request { What = what, Data = data };
        var getRaw  = await PipesClient.SendRequestAsync(request.ToString(), pipes);
        if (!getRaw.Succeeded) return getRaw.ToResult<string?>();
        var parseResponse = JipProtocol.ParseResponse(getRaw.Value);
        if (!parseResponse.Succeeded) return parseResponse.ToResult<string?>();
        return JipProtocol.ToResult(parseResponse.Value);
    }
}

