using System.Reflection;
using System.Text;
using JOSYN.Foundation.ResultPattern;

namespace JOSYN.System.Shared.Log;

/// <summary>
/// Prozess-lokaler Datei-Logger für JOSYN-EXE-Prozesse.
/// Schreibt Einträge nach <c>%TEMP%\JOSYN\&lt;ProcessName&gt;\&lt;yyyy-MM-dd&gt;.log</c>.
/// Wenn <see cref="EnableConsoleOutput"/> gesetzt ist, wird zusätzlich auf die Konsole
/// geschrieben — typischerweise aktiviert der Aufrufer dieses Flag im DEBUG-Build.
/// Schreibfehler werden stillschweigend ignoriert — der Logger darf
/// den Host-Prozess niemals zum Absturz bringen.
/// </summary>
public static class LocalLog
{
    /// <summary>
    /// Name des aktuellen Entry-Assembly (ohne Erweiterung) — kann als Sender-Parameter
    /// an die Überladungen mit Unterordner übergeben werden.
    /// </summary>
    public static readonly string ProcessName =
        Path.GetFileNameWithoutExtension(Assembly.GetEntryAssembly()?.Location ?? "unknown");

    private static readonly string LogDirectory =
        Path.Combine(Path.GetTempPath(), "JOSYN", ProcessName);

    /// <summary>
    /// Steuert, ob Log-Einträge zusätzlich auf die Konsole geschrieben werden.
    /// Der Aufrufer setzt dieses Flag typischerweise im eigenen <c>#if DEBUG</c>-Block.
    /// </summary>
    public static bool EnableConsoleOutput { get; set; } = false;

    /// <summary>Schreibt einen Fehlereintrag.</summary>
    public static void Error(string message, string? callStack = null, string? exceptionDetails = null)
    {
        var entry = FormatEntry("ERROR", message, callStack, exceptionDetails);
        WriteToFile(LogDirectory, entry);
        if (EnableConsoleOutput)
            WriteToConsole(entry, ConsoleColor.Red);
    }

    /// <summary>Schreibt einen Fehlereintrag aus einem <see cref="Result"/>.</summary>
    public static void Error(Result result) =>
        Error(result.ErrorMessage ?? string.Empty, result.CallStackAsString, result.Exception?.ToString());

    /// <summary>Schreibt einen Fehlereintrag unter dem angegebenen Sender-Unterordner.</summary>
    public static void Error(string sender, string message, string? callStack = null, string? exceptionDetails = null)
    {
        var entry = FormatEntry("ERROR", message, callStack, exceptionDetails);
        WriteToFile(Path.Combine(LogDirectory, sender), entry);
        if (EnableConsoleOutput)
            WriteToConsole(entry, ConsoleColor.Red);
    }

    /// <summary>Schreibt einen Fehlereintrag aus einem <see cref="Result"/> unter dem angegebenen Sender-Unterordner.</summary>
    public static void Error(string sender, Result result) =>
        Error(sender, result.ErrorMessage ?? string.Empty, result.CallStackAsString, result.Exception?.ToString());

    /// <summary>Schreibt einen Info-Eintrag.</summary>
    public static void Info(string message)
    {
        var entry = FormatEntry("INFO", message);
        WriteToFile(LogDirectory, entry);
        if (EnableConsoleOutput)
            WriteToConsole(entry, ConsoleColor.Gray);
    }

    /// <summary>Schreibt einen Info-Eintrag unter dem angegebenen Sender-Unterordner.</summary>
    public static void Info(string sender, string message)
    {
        var entry = FormatEntry("INFO", message);
        WriteToFile(Path.Combine(LogDirectory, sender), entry);
        if (EnableConsoleOutput)
            WriteToConsole(entry, ConsoleColor.Gray);
    }

    // -------------------------------------------------------------------------

    private static string FormatEntry(string level, string message, string? callStack = null, string? exceptionDetails = null)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"[{DateTimeOffset.Now:yyyy-MM-dd HH:mm:ss zzz}] [{level}]");
        sb.AppendLine(message);
        if (!string.IsNullOrEmpty(callStack))
        {
            sb.AppendLine("--- CallStack ---");
            sb.AppendLine(callStack);
        }
        if (!string.IsNullOrEmpty(exceptionDetails))
        {
            sb.AppendLine("--- Exception ---");
            sb.AppendLine(exceptionDetails);
        }
        sb.AppendLine(new string('-', 80));
        return sb.ToString();
    }

    private static void WriteToFile(string directory, string entry)
    {
        try
        {
            Directory.CreateDirectory(directory);
            var path = Path.Combine(directory, $"{DateTimeOffset.Now:yyyy-MM-dd}.log");
            File.AppendAllText(path, entry, Encoding.UTF8);
        }
        catch
        {
            // Schreibfehler dürfen den Host-Prozess nicht gefährden.
        }
    }

    private static void WriteToConsole(string entry, ConsoleColor color)
    {
        Console.ForegroundColor = color;
        Console.Write(entry);
        Console.ResetColor();
    }
}
