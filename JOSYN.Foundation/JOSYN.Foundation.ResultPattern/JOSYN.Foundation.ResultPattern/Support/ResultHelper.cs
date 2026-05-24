using System.Diagnostics;

#pragma warning disable IDE0130
namespace JOSYN.Foundation.ResultPattern;
#pragma warning restore IDE0130

internal static class ResultHelper
{
    internal static CallerInfo CreateCallerInfo(string methodName, string filePath, int lineNumber, string className = "")
    {
        return new CallerInfo
        {
            MethodName = methodName,
            ClassName = className,
            FilePath = filePath,
            LineNumber = lineNumber,
        };
    }
    
    internal static string CallStackToString(IReadOnlyList<CallerInfo> callers) =>
        callers.Count == 0
            ? "(kein Callstack)"
            : string.Join("\n", callers.Select(c => $"  at {c}"));

    // "Ausnahmefehler: " prefix is intentionally German — matches the project's runtime error message convention.
    internal static string FormatExceptionMessage(Exception exception) => $"Ausnahmefehler: {exception.Message}";
}
