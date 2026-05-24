using JOSYN.Foundation.ResultPattern;

#pragma warning disable IDE0130
namespace JOSYN.Foundation.JIP;
#pragma warning restore IDE0130

/// <summary>
/// Vertragsdefinition für das JIP-Wire-Protokoll.
/// Enthält Protokoll-Konstanten, CLI-Hilfsmethoden und die Pipe-Namensableitung.
/// </summary>
public interface IPipesProtocol
{
    /// <summary>
    /// Protokoll-Identifier: Präfix für CLI-Argumente und Session-Key-Übergabe.
    /// Wert: <c>"JOSYN-IPC"</c>.
    /// </summary>
    public const string MagicToken = "JOSYN-IPC";

    /// <summary>
    /// Präfix für Fehlerantworten vom Server.
    /// Wert: <c>"JOSYN-IPC-ERROR"</c>.
    /// </summary>
    public const string MagicErrorToken = $"{MagicToken}-ERROR";

    /// <summary>
    /// Antwortwert, wenn der Client bereits eine ausstehende Anfrage hat
    /// (Single-in-Flight-Schutz über Busy-Guard).
    /// Wert: <c>"JOSYN-IPC-ERROR-BUSY"</c>.
    /// </summary>
    public const string MagicBusyToken = $"{MagicErrorToken}-BUSY";

    /// <summary>
    /// Anfrage- und Bestätigungs-Token für einen geordneten Server-Shutdown,
    /// ausgelöst durch <see cref="IPipesClient.DisconnectAsync"/>.
    /// Wert: <c>"JOSYN-IPC-SHUTDOWN"</c>.
    /// </summary>
    public const string MagicShutdownToken = $"{MagicToken}-SHUTDOWN";

    /// <summary>
    /// Erstellt den CLI-Argument-String, den der Server beim Start an den
    /// Client-Prozess übergibt, um den Session-Key zu übermitteln.
    /// </summary>
    /// <param name="sessionKey">Session-Key als String.</param>
    /// <returns>CLI-Argument im Format <c>"JOSYN-IPC &lt;sessionKey&gt;"</c>.</returns>
    static abstract string CreateClientStartCLIArguments(string sessionKey);

    /// <summary>
    /// Parst den Session-Key aus den CLI-Argumenten, die durch
    /// <see cref="CreateClientStartCLIArguments"/> erzeugt wurden.
    /// </summary>
    /// <param name="args">CLI-Argumente des Prozesses (<c>args</c> aus <c>Main</c>).</param>
    /// <returns>
    /// Geparter Session-Key bei Erfolg;
    /// <see cref="Guid.Empty"/>, wenn die Argumente nicht dem erwarteten Format entsprechen.
    /// </returns>
    static abstract Guid ParseSessionKeyCLIArguments(string[] args);

    /// <summary>
    /// Leitet Request- und Response-Pipe-Namen aus dem Session-Key ab.
    /// </summary>
    /// <param name="sessionKey">Session-Key als String.</param>
    /// <returns>
    /// Tupel mit Request-Pipe-Name (<c>"req-pipe-&lt;key&gt;"</c>)
    /// und Response-Pipe-Name (<c>"res-pipe-&lt;key&gt;"</c>).
    /// </returns>
    static abstract (string requestPipeName, string responsePipeName) DerivePipeNamesFromSessionKey(string sessionKey);
}