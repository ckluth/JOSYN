using System.Reflection;
using JOSYN.Foundation.ResultPattern;

#pragma warning disable IDE0130
namespace JOSYN.Foundation.PropertyBag;
#pragma warning restore IDE0130

/// <summary>
/// Serialisiert und deserialisiert C#-<c>record class</c>-Typen in und aus zeichenkettenbasierten Formaten.
/// </summary>
/// <remarks>
/// Unterstützte Formate sind sektionsfreies INI (<c>Schlüssel=Wert</c>-Zeilen) und JSON. Bei der
/// Deserialisierung wird das Format automatisch erkannt: Beginnt das erste Nicht-Leerzeichen-Zeichen
/// mit <c>{</c>, wird JSON angenommen; andernfalls INI.
/// <para>
/// Es werden ausschließlich <c>record class</c>-Typen akzeptiert (zur Laufzeit über die vom Compiler
/// erzeugte Methode <c>&lt;Clone&gt;$</c> erkannt). Eigenschaftstypen müssen zur Menge der
/// unterstützten primitiven und bekannten BCL-Typen gehören.
/// </para>
/// <para>
/// Alle Operationen geben <see cref="Result"/> oder <see cref="Result{T}"/> zurück — Ausnahmen werden
/// nicht weitergegeben.
/// </para>
/// </remarks>
public interface IPropertyBag
{
    /// <summary>
    /// Serialisiert eine <c>record class</c>-Instanz in einen sektionsfreien INI-String im Standardformat.
    /// </summary>
    /// <remarks>
    /// Überladung mit <see cref="IniDictionarySerializer"/> als Serialisierer.
    /// Für ein bestimmtes Ausgabeformat stattdessen
    /// <see cref="IPropertyBag.Serialize{TRecord}(TRecord, DictionaryToStringSerializer)"/> verwenden.
    /// </remarks>
    /// <typeparam name="TRecord">Der zu serialisierende Record-Typ. Muss eine <c>record class</c> sein.</typeparam>
    /// <param name="record">Die zu serialisierende Record-Instanz.</param>
    /// <returns>
    /// Ein <see cref="Result{T}"/> mit dem serialisierten INI-String bei Erfolg, oder ein Fehler, wenn der
    /// Typ keine <c>record class</c> ist, nicht unterstützte Eigenschaftstypen enthält oder die Serialisierung fehlschlägt.
    /// </returns>
    static abstract Result<string> Serialize<TRecord>(TRecord record)
        where TRecord : class;


    /// <summary>
    /// Serialisiert eine als <see cref="object"/> übergebene <c>record class</c>-Instanz in einen
    /// sektionsfreien INI-String im Standardformat.
    /// </summary>
    /// <remarks>
    /// Überladung mit <see cref="IniDictionarySerializer"/> als Serialisierer.
    /// Diese Überladung verwenden, wenn der konkrete Record-Typ erst zur Laufzeit bekannt ist. Für zur
    /// Compilezeit bekannte Typen <see cref="IPropertyBag.Serialize{TRecord}(TRecord)"/> bevorzugen.
    /// </remarks>
    /// <param name="record">Die zu serialisierende Record-Instanz.</param>
    /// <param name="recordType">Der exakte <see cref="Type"/> des Records. Muss einer <c>record class</c> entsprechen.</param>
    /// <returns>
    /// Ein <see cref="Result{T}"/> mit dem serialisierten INI-String bei Erfolg, oder ein Fehler, wenn
    /// <paramref name="recordType"/> keine <c>record class</c> ist, nicht unterstützte Eigenschaftstypen
    /// enthält oder die Serialisierung fehlschlägt.
    /// </returns>
    static abstract Result<string> Serialize(object record, Type recordType);


    /// <summary>
    /// Serialisiert eine <c>record class</c>-Instanz mithilfe des angegebenen Format-Serialisierers in einen String.
    /// </summary>
    /// <typeparam name="TRecord">
    /// Der zu serialisierende Record-Typ. Muss eine <c>record class</c> sein.
    /// </typeparam>
    /// <param name="record">Die zu serialisierende Record-Instanz.</param>
    /// <param name="serializeToString">
    /// Ein <see cref="DictionaryToStringSerializer"/>-Delegat, der das intermediäre flache
    /// <c>Dictionary&lt;string, string&gt;</c> in den finalen String konvertiert.
    /// Für INI <see cref="IniDictionarySerializer.Serialize(Dictionary{string,string})"/>,
    /// für JSON <see cref="JsonDictionarySerializer.Serialize{T}(T)"/> verwenden.
    /// </param>
    /// <returns>
    /// Ein <see cref="Result{T}"/> mit dem serialisierten String bei Erfolg, oder ein Fehler, wenn der
    /// Typ keine <c>record class</c> ist, nicht unterstützte Eigenschaftstypen enthält oder die Serialisierung fehlschlägt.
    /// </returns>
    static abstract Result<string> Serialize<TRecord>(TRecord record, DictionaryToStringSerializer serializeToString)
        where TRecord : class;

    /// <summary>
    /// Serialisiert eine als <see cref="object"/> übergebene <c>record class</c>-Instanz mithilfe des
    /// angegebenen Format-Serialisierers in einen String.
    /// </summary>
    /// <remarks>
    /// Diese Überladung verwenden, wenn der konkrete Record-Typ erst zur Laufzeit bekannt ist. Für zur
    /// Compilezeit bekannte Typen <see cref="IPropertyBag.Serialize{TRecord}(TRecord, DictionaryToStringSerializer)"/> bevorzugen.
    /// </remarks>
    /// <param name="record">Die zu serialisierende Record-Instanz.</param>
    /// <param name="recordType">
    /// Der exakte <see cref="Type"/> des Records. Muss einer <c>record class</c> entsprechen.
    /// </param>
    /// <param name="serializeToString">
    /// Ein <see cref="DictionaryToStringSerializer"/>-Delegat, der das intermediäre flache
    /// <c>Dictionary&lt;string, string&gt;</c> in den finalen String konvertiert.
    /// </param>
    /// <returns>
    /// Ein <see cref="Result{T}"/> mit dem serialisierten String bei Erfolg, oder ein Fehler, wenn
    /// <paramref name="recordType"/> keine <c>record class</c> ist, nicht unterstützte Eigenschaftstypen
    /// enthält oder die Serialisierung fehlschlägt.
    /// </returns>
    static abstract Result<string> Serialize(object record, Type recordType, DictionaryToStringSerializer serializeToString);

    /// <summary>
    /// Erkennt das Zeichenkettenformat (INI oder JSON) automatisch und deserialisiert die Eingabe in
    /// eine Instanz von <paramref name="recordType"/>, zurückgegeben als <see cref="object"/>.
    /// </summary>
    /// <remarks>
    /// Diese Überladung verwenden, wenn der Zieltyp erst zur Laufzeit bekannt ist. Für zur Compilezeit
    /// bekannte Typen <see cref="IPropertyBag.Deserialize{TRecord}(string)"/> bevorzugen.
    /// </remarks>
    /// <param name="raw">
    /// Der zu parsende serialisierte String. Muss im sektionsfreien INI- oder JSON-Format vorliegen.
    /// </param>
    /// <param name="recordType">Der Ziel-<c>record class</c>-Typ, in den deserialisiert werden soll.</param>
    /// <returns>
    /// Ein <see cref="Result{T}"/> mit dem deserialisierten Record als <see cref="object"/> bei Erfolg,
    /// oder ein Fehler, wenn Formaterkennung, Parsing, Typvalidierung oder Eigenschaftskonvertierung fehlschlägt.
    /// </returns>
    static abstract Result<object> Deserialize(string raw, Type recordType);

    /// <summary>
    /// Erkennt das Zeichenkettenformat (INI oder JSON) automatisch und deserialisiert die Eingabe in
    /// eine typisierte <typeparamref name="TRecord"/>-Instanz.
    /// </summary>
    /// <typeparam name="TRecord">
    /// Der Ziel-<c>record class</c>-Typ. Muss einen parameterlosen Konstruktor besitzen (alle vom
    /// Compiler erzeugten Record-Klassen erfüllen diese Anforderung).
    /// </typeparam>
    /// <param name="raw">
    /// Der zu parsende serialisierte String. Muss im sektionsfreien INI- oder JSON-Format vorliegen.
    /// </param>
    /// <returns>
    /// Ein <see cref="Result{T}"/> mit dem deserialisierten <typeparamref name="TRecord"/> bei Erfolg,
    /// oder ein Fehler, wenn Formaterkennung, Parsing, Typvalidierung oder Eigenschaftskonvertierung fehlschlägt.
    /// </returns>
    static abstract Result<TRecord> Deserialize<TRecord>(string raw)
        where TRecord : class;

    /// <summary>
    /// Erkennt das Zeichenkettenformat (INI oder JSON) automatisch und deserialisiert die Eingabe in
    /// ein Array konvertierter Methodenaufruf-Argumente.
    /// </summary>
    /// <remarks>
    /// Schlüssel in den serialisierten Daten werden am ersten Zeichen case-insensitiv gegen Parameternamen
    /// abgeglichen. Nullable-Parameter, die in den Daten fehlen, werden stillschweigend auf
    /// <see langword="null"/> gesetzt. Nicht-nullable Parameter, die fehlen, führen zu einem Fehler.
    /// <para>
    /// Das zurückgegebene Array ist positionell auf <paramref name="parameters"/> ausgerichtet und kann
    /// direkt an <see cref="System.Reflection.MethodBase.Invoke(object?, object?[])"/> übergeben werden.
    /// </para>
    /// </remarks>
    /// <param name="raw">
    /// Der zu parsende serialisierte String. Muss im sektionsfreien INI- oder JSON-Format vorliegen.
    /// </param>
    /// <param name="parameters">
    /// Die Methodenparameter-Deskriptoren, gegen die die geparsten Schlüssel-Wert-Paare abgeglichen werden.
    /// </param>
    /// <returns>
    /// Ein <see cref="Result{T}"/> mit einem <see cref="object"/>[] konvertierter Argumentwerte bei Erfolg,
    /// oder ein Fehler, wenn ein erforderliches Argument fehlt oder eine Typkonvertierung fehlschlägt.
    /// </returns>
    static abstract Result<object[]> Deserialize(string raw, ParameterInfo[] parameters);
}
