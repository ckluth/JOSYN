using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

#pragma warning disable IDE0130
namespace JOSYN.Core.ResultPattern;
#pragma warning restore IDE0130

/// <summary>
/// TODO
/// </summary>    
public interface IResult<TSelf, TValue> where TSelf : IResult<TSelf, TValue>
{
    /// <summary>
    /// TODO
    /// </summary>        
    [MemberNotNullWhen(false, nameof(ErrorMessage))]
    [MemberNotNullWhen(true, nameof(Value))]
    bool Succeeded { get; }

    /// <summary>
    /// TODO
    /// </summary>        
    TValue? Value { get; }

    /// <summary>
    /// TODO
    /// </summary>        
    string? ErrorMessage { get; }

    /// <summary>
    /// TODO
    /// </summary>        
    Exception? Exception { get; }

    /// <summary>
    /// TODO
    /// </summary>        
    IReadOnlyList<CallerInfo> Callers { get; }

    /// <summary>
    /// TODO
    /// </summary>        
    static abstract TSelf Success(TValue value);

    /// <summary>
    /// TODO
    /// </summary>            
    public Result ToResult();

    /// <summary>
    /// TODO
    /// </summary>                
    public Result<TOther> ToResult<TOther>();
    
    /// <summary>
    /// TODO
    /// </summary>                    
    public string CallStackAsString { get; }

    /// <summary>
    /// TODO
    /// </summary>    
    static abstract TSelf Fail(
        string error,
        Exception? exception = null,
        [CallerMemberName] string internal_ignore_methodName = "",
        [CallerFilePath] string internal_ignore_filePath = "",
        [CallerLineNumber] int internal_ignore_lineNumber = 0);

    /// <summary>
    /// TODO
    /// </summary>    
    static abstract TSelf Fail(
        Exception exception,
        [CallerMemberName] string internal_ignore_methodName = "",
        [CallerFilePath] string internal_ignore_filePath = "",
        [CallerLineNumber] int internal_ignore_lineNumber = 0);

    /// <summary>
    /// TODO
    /// </summary>    
    static abstract TSelf Propagate(
        TSelf result,
        [CallerMemberName] string internal_ignore_callermembername = "",
        [CallerFilePath] string internal_ignore_callerfilepath = "",
        [CallerLineNumber] int internal_ignore_callerlinenumber = 0);

    // implicit operators

    /// <summary>
    /// TODO
    /// </summary>    
    static abstract implicit operator TSelf(TValue value);

    /// <summary>
    /// TODO
    /// </summary>    
    static abstract implicit operator TSelf(Exception exception);

    /// <summary>
    /// TODO
    /// </summary>    
    static abstract implicit operator TSelf(Error error);
}