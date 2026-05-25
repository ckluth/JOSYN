using JOSYN.Foundation.ResultPattern;

#pragma warning disable IDE0130
namespace JOSYN.Foundation.PropertyBag;
#pragma warning restore IDE0130

/// <summary>
/// Serialisiert und deserialisiert INI-formatierte Daten in und aus <c>Dictionary</c>-Darstellungen.
/// </summary>
/// <remarks>
/// Unterstützt sowohl sektioniertes INI (<c>[Sektionsname]</c>-Header gefolgt von <c>Schlüssel=Wert</c>-Zeilen)
/// als auch sektionsfreies INI (einfache <c>Schlüssel=Wert</c>-Zeilen ohne Sektions-Header).
/// <para>
/// Werte werden unverändert gespeichert — auf der rechten Seite des <c>=</c> wird kein Trimming angewendet.
/// Ein manuell erstellter INI-Eintrag wie <c>Key= value</c> erfasst das führende Leerzeichen als Teil des
/// Werts. Der Aufrufer ist für den genauen Inhalt auf beiden Seiten des <c>=</c> verantwortlich.
/// </para>
/// <para>
/// Zeilen, die mit <c>;</c> beginnen, und Leerzeilen werden als Kommentare bzw. Leerzeichen behandelt
/// und bei der Deserialisierung ignoriert.
/// </para>
/// <para>
/// Alle Operationen geben <see cref="Result"/> oder <see cref="Result{T}"/> zurück — Ausnahmen werden
/// nicht weitergegeben.
/// </para>
/// </remarks>
public interface IIniDictionarySerializer
{
    /// <summary>
    /// Serialisiert ein mehrsektioniges INI-Dictionary in einen String.
    /// </summary>
    /// <remarks>
    /// Enthält das Dictionary genau einen Eintrag mit dem Schlüssel <see cref="string.Empty"/>, wird die
    /// Ausgabe ohne Sektions-Header erstellt (sektionsfreies INI). Andernfalls wird jeder Eintragsschlüssel
    /// zu einem <c>[Sektionsname]</c>-Header, gefolgt von seinen Schlüssel-Wert-Paaren, mit einer Leerzeile
    /// zwischen den Sektionen.
    /// </remarks>
    /// <param name="data">
    /// Ein Dictionary, das Sektionsnamen auf ihre Schlüssel-Wert-Paare abbildet. Einen leeren String als
    /// Sektionsschlüssel verwenden, um ein sektionsfreies INI-Dokument zu erzeugen.
    /// </param>
    /// <returns>
    /// Ein <see cref="Result{T}"/> mit dem INI-formatierten String bei Erfolg, oder ein Fehler, wenn die
    /// Serialisierung fehlschlägt.
    /// </returns>
    static abstract Result<string> Serialize(Dictionary<string, Dictionary<string, string>> data);

    /// <summary>
    /// Serialisiert ein flaches Schlüssel-Wert-Dictionary in einen sektionsfreien INI-String.
    /// </summary>
    /// <remarks>
    /// Jeder Eintrag in <paramref name="data"/> wird zu einer <c>Schlüssel=Wert</c>-Zeile. Es wird kein
    /// Sektions-Header geschrieben. Um Sektions-Header einzufügen,
    /// <see cref="IIniDictionarySerializer.Serialize(Dictionary{string,Dictionary{string,string}})"/> verwenden.
    /// </remarks>
    /// <param name="data">Die zu serialisierenden Schlüssel-Wert-Paare.</param>
    /// <returns>
    /// Ein <see cref="Result{T}"/> mit dem sektionsfreien INI-formatierten String bei Erfolg, oder ein
    /// Fehler, wenn die Serialisierung fehlschlägt.
    /// </returns>
    static abstract Result<string> Serialize(Dictionary<string, string> data);

    /// <summary>
    /// Parst einen sektionsfreien INI-String in ein flaches Schlüssel-Wert-Dictionary.
    /// </summary>
    /// <remarks>
    /// Hilfsmethode um <see cref="IIniDictionarySerializer.Deserialize(string)"/>, die eine
    /// Einzel-Sektion-Einschränkung durchsetzt. Verwenden, wenn die Eingabe ein einfaches
    /// <c>Schlüssel=Wert</c>-Dokument ohne Sektions-Header ist — z.&#160;B. beim Deserialisieren von
    /// Ausgaben von <see cref="IIniDictionarySerializer.Serialize(Dictionary{string,string})"/>.
    /// </remarks>
    /// <param name="raw">
    /// Ein sektionsfreier INI-String. Kann Kommentarzeilen (beginnend mit <c>;</c>) und Leerzeilen enthalten.
    /// </param>
    /// <returns>
    /// Ein <see cref="Result{T}"/> mit dem geparsten flachen Dictionary bei Erfolg, oder ein Fehler, wenn
    /// die Eingabe leer ist, keine parsebaren Sektionen enthält, mehr als eine Sektion enthält oder das
    /// Parsing fehlschlägt.
    /// </returns>
    static abstract Result<Dictionary<string, string>> DeserializeSingleSection(string raw);

    /// <summary>
    /// Parst einen INI-String in ein verschachteltes Dictionary, indiziert nach Sektionsname.
    /// </summary>
    /// <remarks>
    /// Zeilen vor dem ersten Sektions-Header werden unter einem leeren Sektionsschlüssel gesammelt.
    /// Doppelte Schlüssel innerhalb derselben Sektion führen zu einem Fehler. Schlüssel in verschiedenen
    /// Sektionen dürfen sich wiederholen.
    /// </remarks>
    /// <param name="raw">Der zu parsende INI-formatierte String.</param>
    /// <returns>
    /// Ein <see cref="Result{T}"/> mit dem geparsten verschachtelten Dictionary bei Erfolg, oder ein
    /// Fehler, wenn ein doppelter Schlüssel gefunden wird oder das Parsing fehlschlägt.
    /// </returns>
    static abstract Result<Dictionary<string, Dictionary<string, string>>> Deserialize(string raw);
}
