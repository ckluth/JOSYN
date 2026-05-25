using JOSYN.Foundation.ResultPattern;

namespace JOSYN.System.Shared.Log;

/// <summary>
/// Vertragsdefinition für den prozess-lokalen Datei-Logger.
/// Schreibt Einträge nach <c>&lt;LogDirectory&gt;\&lt;yyyy-MM-dd&gt;.log</c>.
/// Überladungen mit <c>causer</c>-Parameter schreiben in einen gleichnamigen Unterordner.
/// </summary>
public interface ILocalLog
{
    /// <summary>
    /// Wurzelpfad für alle Log-Dateien dieses Prozesses.
    /// Muss vor dem ersten Log-Aufruf gesetzt werden, falls eine andere Ablage gewünscht ist.
    /// </summary>
    static abstract string LogDirectory { get; set; }

    /// <summary>
    /// Steuert, ob Log-Einträge zusätzlich auf die Konsole geschrieben werden.
    /// </summary>
    static abstract bool EnableConsoleOutput { get; set; }

    /// <summary>Schreibt einen Fehlereintrag.</summary>
    static abstract void Error(string message, string? callStack = null, string? exceptionDetails = null);

    /// <summary>Schreibt einen Fehlereintrag aus einem <see cref="Result"/>.</summary>
    static abstract void Error(Result result);

    /// <summary>Schreibt einen Fehlereintrag in den Unterordner des angegebenen Verursachers.</summary>
    static abstract void Error(string causer, string message, string? callStack = null, string? exceptionDetails = null);

    /// <summary>Schreibt einen Fehlereintrag aus einem <see cref="Result"/> in den Unterordner des angegebenen Verursachers.</summary>
    static abstract void Error(string causer, Result result);

    /// <summary>Schreibt einen Info-Eintrag.</summary>
    static abstract void Info(string message);

    /// <summary>Schreibt einen Info-Eintrag in den Unterordner des angegebenen Verursachers.</summary>
    static abstract void Info(string causer, string message);
}
