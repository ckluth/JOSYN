using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

#pragma warning disable IDE0130
namespace JOSYN.Foundation.ResultPattern;
#pragma warning restore IDE0130

/// <summary>
/// Vertrag für ein typisiertes Ergebnis. Implementiert durch <see cref="Result{TValue}"/>.
/// </summary>
public interface IResult<TSelf, TValue> where TSelf : IResult<TSelf, TValue>
{
    /// <summary>
    /// <see langword="true"/>, wenn die Operation erfolgreich war und <see cref="Value"/> gesetzt ist.
    /// Ist der Wert <see langword="false"/>, ist <see cref="ErrorMessage"/> garantiert nicht null.
    /// </summary>
    [MemberNotNullWhen(false, nameof(ErrorMessage))]
    [MemberNotNullWhen(true, nameof(Value))]
    bool Succeeded { get; }

    /// <summary>
    /// Der Ergebniswert. <see langword="null"/> bzw. Standardwert, wenn <see cref="Succeeded"/> <see langword="false"/> ist.
    /// Vor dem Zugriff stets <see cref="Succeeded"/> prüfen.
    /// </summary>
    TValue? Value { get; }

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
    /// Erstellt ein erfolgreiches Ergebnis, das <paramref name="value"/> enthält.
    /// In return-Anweisungen die implizite Konvertierung von <typeparamref name="TValue"/> bevorzugen.
    /// </summary>
    static abstract TSelf Success(TValue value);

    /// <summary>
    /// Entfernt den Wert und gibt ein einfaches <see cref="Result"/> zurück. Fehler und Aufrufkette bleiben erhalten.
    /// </summary>
    Result ToResult();

    /// <summary>
    /// Interpretiert diesen Fehler als <see cref="Result{TOther}"/> neu.
    /// Nur auf einem Fehlerergebnis aufrufen — bei Erfolg entsteht ein stiller Fehler.
    /// </summary>
    Result<TOther> ToResult<TOther>();

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
    static abstract TSelf Propagate(
        TSelf result,
        [CallerMemberName] string internal_ignore_callermembername = "",
        [CallerFilePath] string internal_ignore_callerfilepath = "",
        [CallerLineNumber] int internal_ignore_callerlinenumber = 0);

    /// <summary>
    /// Ermöglicht <c>return myValue;</c> in Methoden, die <see cref="Result{TValue}"/> zurückgeben.
    /// </summary>
    static abstract implicit operator TSelf(TValue value);

    /// <summary>
    /// In catch-Blöcken verwenden: <c>catch (Exception ex) { return ex; }</c>
    /// </summary>
    static abstract implicit operator TSelf(Exception exception);

    /// <summary>
    /// Ermöglicht die direkte Rückgabe eines <see cref="Error"/>-Werts aus einer Methode.
    /// </summary>
    static abstract implicit operator TSelf(Error error);
}
