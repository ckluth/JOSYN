using JOSYN.Foundation.ResultPattern;

#pragma warning disable IDE0130
namespace JOSYN.Foundation.JIP;
#pragma warning restore IDE0130

/// <summary>
/// Vertragsdefinition für den JIP-Konventions-Server-Helfer.
/// Kapselt Request-Parsing, Response-Serialisierung und Fehler-Wrapping,
/// sodass Handler-Code ausschließlich mit <see cref="Result{TValue}"/> arbeitet —
/// ohne Kenntnis des Wire-Formats.
/// </summary>
public interface IJipServer
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
    static abstract Func<string, Task<string>> WrapHandler(Func<Request, Result<string?>> handler);

    /// <summary>
    /// Kapselt einen asynchronen Request-Handler in die vom <see cref="PipesServer"/> erwartete
    /// <c>Func&lt;string, Task&lt;string&gt;&gt;</c>-Signatur.
    /// Parse-Fehler werden als Fehler-Response zurückgegeben.
    /// </summary>
    /// <param name="handler">
    /// Empfängt das geparste <see cref="Request"/> und liefert asynchron <see cref="Result{TValue}"/>
    /// mit optionalem String-Payload.
    /// </param>
    static abstract Func<string, Task<string>> WrapHandler(Func<Request, Task<Result<string?>>> handler);
}
