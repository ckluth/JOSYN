using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

#pragma warning disable IDE0130
namespace JOSYN.Foundation.ResultPattern;
#pragma warning restore IDE0130

/// <summary>
/// Contract for a void result. Implemented by <see cref="Result"/>.
/// </summary>
public interface IResult<out TSelf> where TSelf : IResult<TSelf>
{
    /// <summary>
    /// <see langword="true"/> if the operation succeeded.
    /// When <see langword="false"/>, <see cref="ErrorMessage"/> is guaranteed non-null.
    /// </summary>
    [MemberNotNullWhen(false, nameof(ErrorMessage))]
    bool Succeeded { get; }

    /// <summary>
    /// The error message. Only set when <see cref="Succeeded"/> is <see langword="false"/>.
    /// </summary>
    string? ErrorMessage { get; }

    /// <summary>
    /// The exception that caused the failure, if any.
    /// </summary>
    Exception? Exception { get; }

    /// <summary>
    /// The call chain built up by <see cref="Propagate"/>. Empty on a freshly created failure.
    /// </summary>
    IReadOnlyList<CallerInfo> Callers { get; }

    /// <summary>
    /// Human-readable representation of <see cref="Callers"/> for logging or display.
    /// </summary>
    string CallStackAsString { get; }

    /// <summary>
    /// A succeeded result.
    /// </summary>
    static abstract TSelf Success { get; }

    /// <summary>
    /// Creates a failed result with an error message.
    /// Optionally attach an <paramref name="exception"/> if you have one.
    /// </summary>
    static abstract TSelf Fail(
        string error,
        Exception? exception = null,
        [CallerMemberName] string internal_ignore_methodName = "",
        [CallerFilePath] string internal_ignore_filePath = "",
        [CallerLineNumber] int internal_ignore_lineNumber = 0);

    /// <summary>
    /// Creates a failed result from an exception.
    /// </summary>
    static abstract TSelf Fail(
        Exception exception,
        [CallerMemberName] string internal_ignore_methodName = "",
        [CallerFilePath] string internal_ignore_filePath = "",
        [CallerLineNumber] int internal_ignore_lineNumber = 0);

    /// <summary>
    /// Appends the current caller to the propagation chain and returns the failure unchanged.
    /// Always call this behind <c>if (!result.Succeeded)</c>.
    /// </summary>
    static abstract Result Propagate(
        Result result,
        [CallerMemberName] string internal_ignore_callermembername = "",
        [CallerFilePath] string internal_ignore_callerfilepath = "",
        [CallerLineNumber] int internal_ignore_callerlinenumber = 0);

    /// <summary>
    /// Converts this failed result to a typed <see cref="Result{TValue}"/>.
    /// Only call on a failed result — calling on success yields a silent failure.
    /// </summary>
    Result<TValue> ToResult<TValue>();

    /// <summary>
    /// Creates an <see cref="Error"/> value that implicitly converts to <see cref="Result"/> or <see cref="Result{T}"/>.
    /// Idiomatic for returning failures: <c>return Result.Error("Something went wrong");</c>
    /// </summary>
    static abstract Error Error(string error, Exception? exception = null);

    /// <summary>
    /// Use <c>catch (Exception ex) { return ex; }</c> in catch blocks.
    /// </summary>
    static abstract implicit operator TSelf(Exception exception);

    /// <summary>
    /// Enables returning an <see cref="Error"/> value directly from a method.
    /// </summary>
    static abstract implicit operator TSelf(Error error);

    /// <summary>
    /// Enables <c>return Result.Success;</c> syntax.
    /// </summary>
    static abstract implicit operator TSelf(ResultSuccess _);
}
