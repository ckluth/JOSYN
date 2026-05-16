using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

#pragma warning disable IDE0130
namespace JOSYN.Core.ResultPattern;
#pragma warning restore IDE0130

public interface IResult<out TSelf, TValue> where TSelf : IResult<TSelf, TValue>
{    
    [MemberNotNullWhen(false, nameof(ErrorMessage))]
    [MemberNotNullWhen(true, nameof(Value))]
    bool Succeeded { get; }
	
    TValue? Value { get; }	
	
    string? ErrorMessage { get; }	
	
    Exception? Exception { get; }	
	
    IReadOnlyList<CallerInfo> Callers { get; }

    static abstract TSelf Success(TValue value);

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

    static abstract implicit operator TSelf(TValue value);
    static abstract implicit operator TSelf(Exception exception);
    static abstract implicit operator TSelf(Failure failure);
}