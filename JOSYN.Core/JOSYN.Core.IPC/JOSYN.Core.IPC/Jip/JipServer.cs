#pragma warning disable IDE0130
namespace JOSYN.Core.IPC.JIP;
#pragma warning restore IDE0130

using JOSYN.Core.ResultPattern;

/// <summary>
/// Implementierungs-Helfer für JIP-Server.
/// Kapselt das Request-Parsing, die Response-Serialisierung und das Fehler-Wrapping,
/// sodass der Handler-Code ausschließlich <see cref="Result{TValue}"/> mit <c>string?</c>
/// zurückgibt — ohne Kenntnis des Wire-Formats.
/// </summary>
public static class JipServer
{
    /// <summary>
    /// Kapselt einen synchronen Request-Handler in die vom <see cref="PipesServer"/> erwartete
    /// <c>Func&lt;string, Task&lt;string&gt;&gt;</c>-Signatur.
    /// Parse-Fehler werden als Fehler-Response zurückgegeben.
    /// </summary>
    /// <param name="handler">
    /// Empfängt das geparste <see cref="Request"/> und liefert <see cref="Result{TValue}"/>
    /// mit optionalem String-Payload.
    /// </param>
    public static Func<string, Task<string>> WrapHandler(Func<Request, Result<string?>> handler)
    {
        return requestStr =>
        {
            var parseResult = JipProtocol.ParseRequest(requestStr);
            var response = parseResult.Succeeded
                ? JipProtocol.ToResponse(handler(parseResult.Value))
                : JipProtocol.ToResponse(Result<string?>.Fail($"Ungültige JIP-Anfrage: {parseResult.ErrorMessage}"));
            return Task.FromResult(response.ToString());
        };
    }

    /// <summary>
    /// Kapselt einen asynchronen Request-Handler in die vom <see cref="PipesServer"/> erwartete
    /// <c>Func&lt;string, Task&lt;string&gt;&gt;</c>-Signatur.
    /// Parse-Fehler werden als Fehler-Response zurückgegeben.
    /// </summary>
    /// <param name="handler">
    /// Empfängt das geparste <see cref="Request"/> und liefert asynchron <see cref="Result{TValue}"/>
    /// mit optionalem String-Payload.
    /// </param>
    public static Func<string, Task<string>> WrapHandler(Func<Request, Task<Result<string?>>> handler)
    {
        return async requestStr =>
        {
            var parseResult = JipProtocol.ParseRequest(requestStr);
            var response = parseResult.Succeeded
                ? JipProtocol.ToResponse(await handler(parseResult.Value))
                : JipProtocol.ToResponse(Result<string?>.Fail($"Ungültige JIP-Anfrage: {parseResult.ErrorMessage}"));
            return response.ToString();
        };
    }
}

