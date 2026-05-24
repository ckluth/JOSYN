using JOSYN.Foundation.ResultPattern;

#pragma warning disable IDE0130
namespace JOSYN.Foundation.JIP;
#pragma warning restore IDE0130

/// <summary>
/// Vertragsdefinition für den JIP-Transportschicht-Server.
/// Richtet Named Pipes ein, wartet auf einen Client und verarbeitet Anfragen
/// über einen konfigurierbaren Handler.
/// </summary>
public interface IPipesServer
{
    /// <summary>
    /// Startet den Server-Lifecycle: richtet Request- und Response-Pipe ein,
    /// wartet auf eine Client-Verbindung und verarbeitet Anfragen sequenziell
    /// bis zum Abbruch oder Fehler.
    /// </summary>
    /// <param name="args">
    /// Startkonfiguration: Session-Key, Request-Handler, Timeout und optionaler
    /// Pfad zur Client-Exe.
    /// </param>
    /// <param name="reConnect">
    /// Wenn <see langword="true"/>, wird der Server nach einer ordnungsgemäßen
    /// Verbindungstrennung automatisch neu gestartet. Nicht kombinierbar mit
    /// <see cref="ServerStartArguments.ClientExePath"/>.
    /// </param>
    /// <param name="onReconnect">
    /// Optionaler Callback, der vor jedem Reconnect-Versuch aufgerufen wird —
    /// z. B. für Logging oder Statusbenachrichtigungen.
    /// </param>
    /// <returns>
    /// Erfolgreich, wenn der Server geordnet beendet wurde;
    /// Fehler bei Konfigurationsproblemen oder nicht behebbaren Transportfehlern.
    /// </returns>
    static abstract Task<Result> RunAsync(ServerStartArguments args, bool reConnect = false, Action? onReconnect= null);
}