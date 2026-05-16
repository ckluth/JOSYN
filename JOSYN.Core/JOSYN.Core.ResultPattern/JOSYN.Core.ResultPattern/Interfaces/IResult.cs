using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

#pragma warning disable IDE0130
namespace JOSYN.Core.ResultPattern;
#pragma warning restore IDE0130

public interface IResult<out TSelf> where TSelf : IResult<TSelf>
{
    [MemberNotNullWhen(false, nameof(ErrorMessage))]
    bool Succeeded { get; }
	
    string? ErrorMessage { get; }
	
    Exception? Exception { get; }
	
    IReadOnlyList<CallerInfo> Callers { get; }

    static abstract TSelf Success { get; }

    static abstract TSelf Fail(
        string error,
        Exception? exception = null,
        [CallerMemberName] string methodName = "",
        [CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = 0);

    static abstract TSelf Fail(
        Exception exception,
        [CallerMemberName] string methodName = "",
        [CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = 0);

    static abstract Failure Failure(string error, Exception? exception = null);

    static abstract implicit operator TSelf(Exception exception);
    static abstract implicit operator TSelf(Failure failure);
}