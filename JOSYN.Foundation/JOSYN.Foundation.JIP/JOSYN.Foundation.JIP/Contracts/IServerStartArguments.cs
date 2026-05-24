namespace JOSYN.Foundation.JIP;

/// <summary>
/// Startkonfiguration für <see cref="PipesServer"/>.
/// Kapselt Handler, Session-Key, Timeout und optionalen Client-Pfad
/// als unveränderlicher Record.
/// </summary>
public interface IServerStartArguments
{
    /// <summary>
    /// Eindeutiger Session-Key, aus dem die Pipe-Namen abgeleitet werden.
    /// Standard: automatisch generierte <see cref="Guid"/>.
    /// </summary>
    Guid SessionKey { get; init; }

    /// <summary>
    /// Optionaler Pfad zur Client-Exe, die der Server beim Start startet.
    /// Wenn <see langword="null"/>, muss der Client eigenständig gestartet worden sein.
    /// </summary>
    string? ClientExePath { get; init; }

    /// <summary>
    /// String-Request-Handler (UTF-8). Genau einer von
    /// <see cref="HandleStringRequest"/> oder <see cref="HandleRawRequest"/> muss gesetzt sein.
    /// </summary>
    Func<string, Task<string>>? HandleStringRequest { get; init; }

    /// <summary>
    /// Binär-Request-Handler. Genau einer von
    /// <see cref="HandleRawRequest"/> oder <see cref="HandleStringRequest"/> muss gesetzt sein.
    /// </summary>
    Func<byte[], Task<byte[]>>? HandleRawRequest { get; init; }
    
    /// <summary>
    /// <see langword="true"/>, wenn <see cref="HandleStringRequest"/> gesetzt ist;
    /// <see langword="false"/>, wenn <see cref="HandleRawRequest"/> verwendet wird.
    /// </summary>
    bool HasStringRequestHandler { get; }

    /// <summary>
    /// Maximale Wartezeit auf eine eingehende Client-Verbindung.
    /// Standard: 10 Sekunden.
    /// </summary>
    TimeSpan ConnectionTimeout { get; init; }

    /// <summary>
    /// Callback für nicht-kritische Fehler im Request-Loop.
    /// Empfängt den Anfrage-String und die aufgetretene Exception zur Fehlerprotokollierung.
    /// </summary>
    Func<string, Exception, Task> HandleErrorNotification { get; init; }

    /// <summary>
    /// Optionaler asynchroner Abbruch-Callback. Wird im Polling-Intervall (100 ms)
    /// abgefragt. Gibt <see langword="true"/> zurück, wenn der Server anhalten soll.
    /// Wenn <see langword="null"/>, läuft der Server bis zum Verbindungsende.
    /// </summary>
    Func<Task<bool>>? IsCancellationRequested { get; init; }

}