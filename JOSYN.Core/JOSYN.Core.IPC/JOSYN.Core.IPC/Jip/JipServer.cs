#pragma warning disable IDE0130
namespace JOSYN.Core.IPC.JIP;
#pragma warning restore IDE0130

/// <summary>
/// Implementierungs-Helfer für JIP-Server.
/// Kapselt das Request-Parsing, die Response-Serialisierung und das Fehler-Wrapping,
/// sodass der Handler-Code ausschließlich auf <see cref="Request"/> und <see cref="Response"/>
/// operiert — ohne Kenntnis des Wire-Formats.
/// <para>
/// Steht außerhalb des Protokoll-Vertrags (<see cref="IJipProtocol"/>) und ist
/// ausschließlich für die Implementierung von X-JIP-Server-Protokollen gedacht.
/// </para>
/// </summary>
public static class JipServer
{
    /// <summary>
    /// Kapselt einen synchronen Request-Handler in die vom <see cref="PipesServer"/> erwartete
    /// <c>Func&lt;string, Task&lt;string&gt;&gt;</c>-Signatur.
    /// Parse-Fehler werden als <see cref="ResponseStatus.LogicalFailure"/> zurückgegeben.
    /// </summary>
    /// <param name="handler">
    /// Empfängt das geparste <see cref="Request"/> und liefert die fertige <see cref="Response"/>.
    /// </param>
    public static Func<string, Task<string>> WrapHandler(Func<Request, Response> handler)
    {
        return requestStr =>
        {
            var parseResult = JipProtocol.ParseRequest(requestStr);
            var response = parseResult.Succeeded
                ? handler(parseResult.Value)
                : JipProtocol.ToLogicalFailureResponse($"Ungültige JIP-Anfrage: {parseResult.ErrorMessage}");
            return Task.FromResult(response.ToString());
        };
    }

    /// <summary>
    /// Kapselt einen asynchronen Request-Handler in die vom <see cref="PipesServer"/> erwartete
    /// <c>Func&lt;string, Task&lt;string&gt;&gt;</c>-Signatur.
    /// Parse-Fehler werden als <see cref="ResponseStatus.LogicalFailure"/> zurückgegeben.
    /// </summary>
    /// <param name="handler">
    /// Empfängt das geparste <see cref="Request"/> und liefert asynchron die fertige <see cref="Response"/>.
    /// </param>
    public static Func<string, Task<string>> WrapHandler(Func<Request, Task<Response>> handler)
    {
        return async requestStr =>
        {
            var parseResult = JipProtocol.ParseRequest(requestStr);
            var response = parseResult.Succeeded
                ? await handler(parseResult.Value)
                : JipProtocol.ToLogicalFailureResponse($"Ungültige JIP-Anfrage: {parseResult.ErrorMessage}");
            return response.ToString();
        };
    }
}
