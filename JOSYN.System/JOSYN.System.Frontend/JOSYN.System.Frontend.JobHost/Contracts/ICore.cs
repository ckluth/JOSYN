namespace JOSYN.System.Frontend.JobHost;

/// <summary>
/// Vertrag für den Einstiegspunkt einer Job-Exe.
/// </summary>
public interface ICore
{
    /// <summary>
    /// Einstiegspunkt jeder Job-Exe. Liest die IPC-Session-Argumente, verbindet sich mit dem
    /// JAPServer, ruft den Job via Reflection auf und gibt das Ergebnis zurück.
    /// </summary>
    /// <param name="args">Kommandozeilenargumente, die den IPC-Session-Key enthalten.</param>
    /// <returns>
    /// <c>0</c> bei Erfolg; negative Werte kodieren spezifische Fehlerszenarien:
    /// <c>-1</c> = IPC-Verbindung fehlgeschlagen, <c>-2</c> = Job-Aufruf fehlgeschlagen.
    /// </returns>
    public static abstract Task<int> Run(string[] args);
}