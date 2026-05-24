using JOSYN.Foundation.ResultPattern;

#pragma warning disable IDE0130
namespace JOSYN.Foundation.JIP;
#pragma warning restore IDE0130

/// <summary>
/// Vertragsdefinition für den JIP-Transportschicht-Client.
/// Stellt eine Verbindung zu einem laufenden Server her und sendet Anfragen
/// über das Length-Prefix-Protokoll (<c>int32</c> + Bytes, Little-Endian).
/// </summary>
public interface IPipesClient
{
    /// <summary>
    /// Verbindet sich mit dem laufenden JIP-Server über zwei Named Pipes,
    /// die aus dem <paramref name="sessionKey"/> abgeleitet werden
    /// (Request-Pipe und Response-Pipe). Versucht die Verbindung mit
    /// exponentiellem Backoff bis zu fünf Mal.
    /// </summary>
    /// <param name="sessionKey">
    /// Eindeutiger Session-Schlüssel; muss mit dem des Servers übereinstimmen.
    /// </param>
    /// <returns>
    /// <see cref="ClientPipes"/>-Handle bei Erfolg;
    /// Fehler, wenn der Server nicht innerhalb aller Wiederholungsversuche erreichbar war.
    /// </returns>
    static abstract Task<Result<ClientPipes>> ConnectAsync(Guid sessionKey);

    /// <summary>
    /// Sendet eine binäre Anfrage an den Server und gibt die rohe binäre Antwort zurück.
    /// Schützt gegen parallele Aufrufe über einen Busy-Guard (Single-in-Flight-Protokoll).
    /// </summary>
    /// <param name="requestBytes">Rohe Anfrage-Bytes.</param>
    /// <param name="pipes">Verbindungs-Handle aus <see cref="ConnectAsync"/>.</param>
    /// <returns>
    /// Binäre Antwort-Bytes bei Erfolg;
    /// Fehler, wenn der Server einen Fehler-Magic-Token zurückgibt, der Client bereits
    /// beschäftigt ist oder der Transport fehlschlägt.
    /// </returns>
    static abstract Task<Result<byte[]>> SendRequestAsync(byte[] requestBytes, ClientPipes pipes);

    /// <summary>
    /// Sendet eine UTF-8-kodierte String-Anfrage an den Server und gibt die
    /// Antwort als String zurück.
    /// </summary>
    /// <param name="request">Anfrage-String (UTF-8).</param>
    /// <param name="pipes">Verbindungs-Handle aus <see cref="ConnectAsync"/>.</param>
    /// <returns>
    /// Antwort-String bei Erfolg;
    /// Fehler, wenn der Server einen Fehler-Magic-Token zurückgibt oder der Transport fehlschlägt.
    /// </returns>
    static abstract Task<Result<string>> SendRequestAsync(string request, ClientPipes pipes);

    /// <summary>
    /// Trennt die Verbindung zum Server und gibt die Pipe-Ressourcen frei.
    /// Sendet optional einen Shutdown-Magic-Token, bevor die Pipes geschlossen werden.
    /// </summary>
    /// <param name="pipes">Verbindungs-Handle, das getrennt werden soll.</param>
    /// <param name="sendShutdownRequest">
    /// Wenn <see langword="true"/>, sendet den <see cref="IPipesProtocol.MagicShutdownToken"/>,
    /// bevor die Verbindung getrennt wird — signalisiert dem Server einen geordneten Abbruch.
    /// </param>
    /// <returns>Erfolgreich, wenn die Verbindung ordnungsgemäß getrennt wurde.</returns>
    static abstract Task<Result> DisconnectAsync(ClientPipes pipes, bool sendShutdownRequest = false);
}