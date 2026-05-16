using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

#pragma warning disable IDE0130
namespace JOSYN.Core.ResultPattern;
#pragma warning restore IDE0130

public sealed record Result<TValue> : IResult<Result<TValue>, TValue>
{
    [MemberNotNullWhen(true, nameof(Value))]
    public TValue? Value { get; init; }

    [MemberNotNullWhen(false, nameof(ErrorMessage))]
    [MemberNotNullWhen(true, nameof(Value))]
    public bool Succeeded => ErrorMessage == null;

    public string? ErrorMessage { get; init; }
    public Exception? Exception { get; init; }
    public IReadOnlyList<CallerInfo> Callers { get; init; } = [];

    private Result(TValue? value = default, string? error = null, Exception? exception = null)
    {
        Value = value;
        ErrorMessage = error;
        Exception = exception;
    }
    
    public static Result<TValue> Success(TValue value) => new(value);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public static Result<TValue> Fail(string error, Exception? exception = null, [CallerMemberName] string internal_ignore_callermembername = "", [CallerFilePath] string internal_ignore_callerfilepath = "", [CallerLineNumber] int internal_ignore_callerlinenumber = 0)
    {
        var caller = ResultHelper.CreateCallerInfo(internal_ignore_callermembername, internal_ignore_callerfilepath, internal_ignore_callerlinenumber, new StackFrame(1).GetMethod()?.DeclaringType?.Name ?? "");
        return new Result<TValue>(default, error, exception) with { Callers = [caller] };
    }

    [EditorBrowsable(EditorBrowsableState.Never)]
    public static Result<TValue> Fail(Exception exception, [CallerMemberName] string internal_ignore_callermembername = "", [CallerFilePath] string internal_ignore_callerfilepath = "", [CallerLineNumber] int internal_ignore_callerlinenumber = 0)
    {
        var className = new StackFrame(1).GetMethod()?.DeclaringType?.Name ?? "";
        var caller = ResultHelper.CreateCallerInfo(internal_ignore_callermembername, internal_ignore_callerfilepath, internal_ignore_callerlinenumber, className);
        return new Result<TValue>(default, ResultHelper.FormatExceptionMessage(exception), exception) with { Callers = [caller] };
    }

    public static implicit operator Result<TValue>(TValue value) => new(value);

    public static implicit operator Result<TValue>(Exception exception)
    {
        var frame = new StackFrame(1);
        var method = frame.GetMethod();
        var caller = new CallerInfo
        {
            MethodName = method?.Name ?? "",
            ClassName = method?.DeclaringType?.Name ?? "",
        };
        return new Result<TValue>(default, ResultHelper.FormatExceptionMessage(exception), exception) with { Callers = [caller] };
    }

    public static implicit operator Result<TValue>(Failure failure)
    {
        var frame = new StackFrame(1);
        var method = frame.GetMethod();
        var caller = new CallerInfo
        {
            MethodName = method?.Name ?? "",
            ClassName = method?.DeclaringType?.Name ?? "",
        };
        return new Result<TValue>(default, failure.ErrorMessage, failure.Exception) with { Callers = [caller] };
    }

    [EditorBrowsable(EditorBrowsableState.Never)]
    public static Result<TValue> Propagate(Result<TValue> result, [CallerMemberName] string internal_ignore_callermembername = "", [CallerFilePath] string internal_ignore_callerfilepath = "", [CallerLineNumber] int internal_ignore_callerlinenumber = 0)
    {
        if (result.Succeeded) return result;
        var className = new StackFrame(1).GetMethod()?.DeclaringType?.Name ?? "";
        var caller = ResultHelper.CreateCallerInfo(internal_ignore_callermembername, internal_ignore_callerfilepath, internal_ignore_callerlinenumber, className);
        return result with { Callers = [.. result.Callers, caller] };
    }

    public Result ToResult() 
    { 
        if (Succeeded) return Result.Success; return Result.FailSilent(ErrorMessage!, Exception) with { Callers = Callers };
    }

    public Result<TOther> ToResult<TOther>()
    {
        if (Succeeded) return Result<TOther>.FailSilent("Kein Wert vorhanden");
        return Result<TOther>.FailSilent(ErrorMessage!, Exception) with { Callers = Callers };
    }
    
    public string CallStackToString() => ResultHelper.CallStackToString(Callers);

    internal static Result<TValue> FailSilent(string error, Exception? exception = null) => new(default, error, exception);
}