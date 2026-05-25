using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

#pragma warning disable IDE0130
namespace JOSYN.Foundation.ResultPattern;
#pragma warning restore IDE0130

/// <summary>
/// Vertrag für ein void-Ergebnis. Implementiert durch <see cref="Result"/>.
/// </summary>
public interface IResult<out TSelf> where TSelf : IResult<TSelf>
{
    /// <summary>
    /// <see langword="true"/>, wenn die Operation erfolgreich war.
    /// Ist der Wert <see langword="false"/>, ist <see cref="ErrorMessage"/> garantiert nicht null.
    /// </summary>
    [MemberNotNullWhen(false, nameof(ErrorMessage))]
    bool Succeeded { get; }

    /// <summary>
    /// Die Fehlermeldung. Nur gesetzt, wenn <see cref="Succeeded"/> <see langword="false"/> ist.
    /// </summary>
    string? ErrorMessage { get; }

    /// <summary>
    /// Die auslösende Ausnahme, falls vorhanden.
    /// </summary>
    Exception? Exception { get; }

    /// <summary>
    /// Die durch <see cref="Propagate"/> aufgebaute Aufrufkette. Bei einem neu erstellten Fehler leer.
    /// </summary>
    IReadOnlyList<CallerInfo> Callers { get; }

    /// <summary>
    /// Lesbare Darstellung von <see cref="Callers"/> für Logging oder Ausgabe.
    /// </summary>
    string CallStackAsString { get; }

    /// <summary>
    /// Ein erfolgreiches Ergebnis.
    /// </summary>
    static abstract TSelf Success { get; }

    /// <summary>
    /// Erstellt ein Fehlerergebnis mit einer Fehlermeldung.
    /// Optional kann eine <paramref name="exception"/> angefügt werden.
    /// </summary>
    static abstract TSelf Fail(
        string error,
        Exception? exception = null,
        [CallerMemberName] string internal_ignore_methodName = "",
        [CallerFilePath] string internal_ignore_filePath = "",
        [CallerLineNumber] int internal_ignore_lineNumber = 0);

    /// <summary>
    /// Erstellt ein Fehlerergebnis aus einer Ausnahme.
    /// </summary>
    static abstract TSelf Fail(
        Exception exception,
        [CallerMemberName] string internal_ignore_methodName = "",
        [CallerFilePath] string internal_ignore_filePath = "",
        [CallerLineNumber] int internal_ignore_lineNumber = 0);

    /// <summary>
    /// Hängt den aktuellen Aufrufer an die Weiterleitungskette an und gibt den Fehler unverändert zurück.
    /// Immer hinter <c>if (!result.Succeeded)</c> aufrufen.
    /// </summary>
    static abstract Result Propagate(
        Result result,
        [CallerMemberName] string internal_ignore_callermembername = "",
        [CallerFilePath] string internal_ignore_callerfilepath = "",
        [CallerLineNumber] int internal_ignore_callerlinenumber = 0);

    /// <summary>
    /// Konvertiert dieses Fehlerergebnis in ein typisiertes <see cref="Result{TValue}"/>.
    /// Nur auf einem Fehlerergebnis aufrufen — bei Erfolg entsteht ein stiller Fehler.
    /// </summary>
    Result<TValue> ToResult<TValue>();

    /// <summary>
    /// Erstellt einen <see cref="Error"/>-Wert, der implizit in <see cref="Result"/> oder <see cref="Result{T}"/> konvertiert.
    /// Idiomatisch für die Rückgabe von Fehlern: <c>return Result.Error("Fehler aufgetreten");</c>
    /// </summary>
    static abstract Error Error(string error, Exception? exception = null);

    /// <summary>
    /// In catch-Blöcken verwenden: <c>catch (Exception ex) { return ex; }</c>
    /// </summary>
    static abstract implicit operator TSelf(Exception exception);

    /// <summary>
    /// Ermöglicht die direkte Rückgabe eines <see cref="Error"/>-Werts aus einer Methode.
    /// </summary>
    static abstract implicit operator TSelf(Error error);

    /// <summary>
    /// Ermöglicht die Syntax <c>return Result.Success;</c>.
    /// </summary>
    static abstract implicit operator TSelf(ResultSuccess _);
}
