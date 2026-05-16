using System;
using System.ComponentModel;

using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

#pragma warning disable IDE0130
namespace JOSYN.Core.ResultPattern;
#pragma warning restore IDE0130

public sealed record Result : IResult<Result>
{

    [MemberNotNullWhen(false, nameof(ErrorMessage))]
    public bool Succeeded => ErrorMessage == null;
    public string? ErrorMessage { get; init; }
    public Exception? Exception { get; init; }
    private Result(string? error = null, Exception? exception = null)
    {
        ErrorMessage = error;
        Exception = exception;
    }
    public static Result Success => default(ResultSuccess);

    public IReadOnlyList<CallerInfo> Callers { get; init; } = [];

    public static implicit operator Result(ResultSuccess _) => new();

    public static implicit operator Result(Exception exception)
    {
        var frame = new StackFrame(1);
        var method = frame.GetMethod();
        var caller = new CallerInfo
        {
            MethodName = method?.Name ?? "",
            ClassName = method?.DeclaringType?.Name ?? "",
        };
        return new Result(ResultHelper.FormatExceptionMessage(exception), exception) with { Callers = [caller] };
    }

    public static implicit operator Result(Failure failure)
    {
        var frame = new StackFrame(1);
        var method = frame.GetMethod();
        var caller = new CallerInfo
        {
            MethodName = method?.Name ?? "",
            ClassName = method?.DeclaringType?.Name ?? "",
            FilePath = "",
            LineNumber = 0,
        };
        return new Result(failure.ErrorMessage, failure.Exception) with { Callers = [caller] };
    }
    
    public static Result Fail(string error, Exception? exception = null, [CallerMemberName] string internal_ignore_callermembername = "", [CallerFilePath] string internal_ignore_callerfilepath = "", [CallerLineNumber] int internal_ignore_callerlinenumber = 0)
    {
        var caller = ResultHelper.CreateCallerInfo(internal_ignore_callermembername, internal_ignore_callerfilepath, internal_ignore_callerlinenumber, new StackFrame(1).GetMethod()?.DeclaringType?.Name ?? "");
        return new Result(error, exception) with { Callers = [caller] };
    }

    [EditorBrowsable(EditorBrowsableState.Never)]    
    public static Result Fail(Exception exception, [CallerMemberName] string internal_ignore_callermembername = "", [CallerFilePath] string internal_ignore_callerfilepath = "", [CallerLineNumber] int internal_ignore_callerlinenumber = 0)
    {
        if (internal_ignore_callermembername == null) throw new ArgumentNullException(nameof(internal_ignore_callermembername));
        var className = new StackFrame(1).GetMethod()?.DeclaringType?.Name ?? "";
        var caller = ResultHelper.CreateCallerInfo(internal_ignore_callermembername, internal_ignore_callerfilepath, internal_ignore_callerlinenumber , className);
        return new Result(ResultHelper.FormatExceptionMessage(exception), exception) with { Callers = [caller] };
    }
    public static Failure Failure(string error, Exception? exception = null) => new(error, exception);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public static Result Propagate(Result result, [CallerMemberName] string internal_ignore_callermembername = "", [CallerFilePath] string internal_ignore_callerfilepath = "", [CallerLineNumber] int internal_ignore_callerlinenumber = 0)
    {
        if (result.Succeeded) return result;
        var className = new StackFrame(1).GetMethod()?.DeclaringType?.Name ?? "";
        var caller = ResultHelper.CreateCallerInfo(internal_ignore_callermembername, internal_ignore_callerfilepath, internal_ignore_callerlinenumber, className);
        return result with { Callers = [.. result.Callers, caller] };
    }

    public string CallStackToString() => ResultHelper.CallStackToString(Callers);

    public Result<TValue> ToResult<TValue>()
    {
        if (Succeeded) return Result<TValue>.FailSilent("Kein Wert vorhanden");
        return Result<TValue>.FailSilent(ErrorMessage!, Exception) with { Callers = Callers };
    }

    internal static Result FailSilent(string error, Exception? exception = null) => new(error, exception);
}