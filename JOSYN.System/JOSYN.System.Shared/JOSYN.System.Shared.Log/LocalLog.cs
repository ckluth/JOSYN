using System.Diagnostics;
using System.Text;
using JOSYN.Foundation.ResultPattern;

namespace JOSYN.System.Shared.Log;

/// <summary>
/// Prozess-lokaler Datei-Logger für JOSYN-EXE-Prozesse.
/// Schreibt Einträge nach <c>%TEMP%\JOSYN\&lt;ProcessName&gt;\&lt;yyyy-MM-dd&gt;.log</c>.
/// Im Debug-Build wird zusätzlich auf die Konsole geschrieben.
/// Schreibfehler werden stillschweigend ignoriert — der Logger darf
/// den Host-Prozess niemals zum Absturz bringen.
/// </summary>
public static class LocalLog
{
    private static readonly string LogDirectory =
        Path.Combine(Path.GetTempPath(), "JOSYN", Process.GetCurrentProcess().ProcessName);

    /// <summary>Schreibt einen Fehlereintrag.</summary>
    public static void Error(string message, string? callStack = null, string? exceptionDetails = null)
    {
        var entry = FormatEntry("ERROR", message, callStack, exceptionDetails);
        WriteToFile(entry);
#if DEBUG
        WriteToConsole(entry, ConsoleColor.Red);
#endif
    }

    /// <summary>Schreibt einen Fehlereintrag aus einem <see cref="Result"/>.</summary>
    public static void Error(Result result) =>
        Error(result.ErrorMessage ?? string.Empty, result.CallStackAsString, result.Exception?.ToString());

    /// <summary>Schreibt einen Info-Eintrag.</summary>
    public static void Info(string message)
    {
        var entry = FormatEntry("INFO", message);
        WriteToFile(entry);
#if DEBUG
        WriteToConsole(entry, ConsoleColor.Gray);
#endif
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

    private static void WriteToFile(string entry)
    {
        try
        {
            Directory.CreateDirectory(LogDirectory);
            var path = Path.Combine(LogDirectory, $"{DateTimeOffset.Now:yyyy-MM-dd}.log");
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
