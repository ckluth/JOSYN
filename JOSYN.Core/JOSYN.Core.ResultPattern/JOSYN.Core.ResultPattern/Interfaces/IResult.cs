using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

#pragma warning disable IDE0130
namespace JOSYN.Core.ResultPattern;
#pragma warning restore IDE0130

/// <summary>
/// IResult&lt;T&gt; ist die Basisschnittstelle für das Result-Pattern, die die grundlegenden Eigenschaften und Methoden definiert, die von allen Result-Typen implementiert werden müssen. Sie ermöglicht es, den Erfolg oder Fehler einer Operation zu repräsentieren und enthält Informationen über Fehlertexte, Ausnahmen und den Callstack der Aufrufkette.
/// </summary>                
public interface IResult<out TSelf> where TSelf : IResult<TSelf>
{
    /// <summary>
    /// Ist true, wenn kein Fehlertext vorhanden ist, sonst false.
    /// </summary>                
    [MemberNotNullWhen(false, nameof(ErrorMessage))]
    bool Succeeded { get; }
    
    /// <summary>
    /// Entält den Fehlertext.
    /// </summary>            
    string? ErrorMessage { get; }

    /// <summary>
    /// Entält optional die Exception, die zum Fehler geführt hat.
    /// </summary>        
    Exception? Exception { get; }

    /// <summary>
    /// Entält den Callstack der ganzen Aufrufkette, wenn mit Propagate() gearbeitet wurde.
    /// </summary>    
    IReadOnlyList<CallerInfo> Callers { get; }
    
    /// <summary>
    /// TODO
    /// </summary>
    string CallStackAsString { get; }

    /// <summary>
    /// Gibt ein Result ohne Fehlerinformationen zurück.
    /// </summary>    
    static abstract TSelf Success { get; }

    /// <summary>
    /// Gibt ein Result mit Fehlerinformationen zurück.
    /// </summary>
    static abstract TSelf Fail(
        string error,
        Exception? exception = null,
        [CallerMemberName] string internal_ignore_methodName = "",
        [CallerFilePath] string internal_ignore_filePath = "",
        [CallerLineNumber] int internal_ignore_lineNumber = 0);

    /// <summary>
    /// Gibt ein Result mit Fehlerinformationen zurück.
    /// </summary>
    static abstract TSelf Fail(
        Exception exception,
        [CallerMemberName] string internal_ignore_methodName = "",
        [CallerFilePath] string internal_ignore_filePath = "",
        [CallerLineNumber] int internal_ignore_lineNumber = 0);

    /// <summary>
    /// Propagiert ein Result oder ein ein Result&lt;T&gt;mit dem Callstack der ganzen Aufrufkette.
    /// </summary>    
    static abstract Result Propagate(
        Result result,
        [CallerMemberName] string internal_ignore_callermembername = "",
        [CallerFilePath] string internal_ignore_callerfilepath = "",
        [CallerLineNumber] int internal_ignore_callerlinenumber = 0);
    
    /// <summary>
    /// TODO
    /// </summary>    
    Result<TValue> ToResult<TValue>();

    /// <summary>
    /// Gibt ein Error-Objekt zurück, das implicit in ein Result umgewandelt wird.
    /// </summary>
    static abstract Error Error(string error, Exception? exception = null);

    // implcit operatoren für Exception und Error

    /// <summary>
    /// Wandelt implict eine Exception in ein Result um, wobei die Fehlermeldung aus der Exception.Message stammt.
    /// </summary>    
    static abstract implicit operator TSelf(Exception exception);

    /// <summary>
    /// Wandelt implict ein Error-Objekt in ein Result um.
    /// </summary>        
    static abstract implicit operator TSelf(Error error);

    /// <summary>
    /// TODO
    /// </summary>
    static abstract implicit operator TSelf(ResultSuccess _);
}