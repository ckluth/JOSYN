using System.Diagnostics;

#pragma warning disable IDE0130
namespace JOSYN.Core.ResultPattern;
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
            : string.Join("\n  at ", callers.Select(c => c.ToString()));

    internal static string FormatExceptionMessage(Exception exception) => $"Ausnahmefehler: {exception.Message}";
}