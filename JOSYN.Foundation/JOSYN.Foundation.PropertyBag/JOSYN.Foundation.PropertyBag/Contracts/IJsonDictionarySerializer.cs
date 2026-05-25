using JOSYN.Foundation.ResultPattern;

#pragma warning disable IDE0130
namespace JOSYN.Foundation.PropertyBag;
#pragma warning restore IDE0130

/// <summary>
/// Serialisiert und deserialisiert JSON-Daten in und aus <c>Dictionary&lt;string, string&gt;</c>-Darstellungen.
/// </summary>
/// <remarks>
/// Erzeugt eingerücktes JSON mit Enum-Werten als Strings. Kulturabhängige
/// <see cref="System.Text.Json.Serialization.JsonConverter{T}"/>-Instanzen werden für
/// <see cref="System.DateTime"/>, <see cref="System.DateOnly"/>, <see cref="System.TimeOnly"/> und
/// <see cref="decimal"/> angewendet, sodass Werte gemäß der aktuellen Thread-Kultur
/// (Standard: <c>de-DE</c>) formatiert und geparst werden.
/// <para>
/// Alle Operationen geben <see cref="Result"/> oder <see cref="Result{T}"/> zurück — Ausnahmen werden
/// nicht weitergegeben.
/// </para>
/// </remarks>
public interface IJsonDictionarySerializer
{
    /// <summary>
    /// Serialisiert einen beliebigen Wert mithilfe kulturabhängiger Konverter in einen eingerückten JSON-String.
    /// </summary>
    /// <typeparam name="T">Der Typ des zu serialisierenden Werts.</typeparam>
    /// <param name="obj">Der zu serialisierende Wert.</param>
    /// <returns>
    /// Ein <see cref="Result{T}"/> mit dem JSON-String bei Erfolg, oder ein Fehler, wenn die
    /// Serialisierung fehlschlägt.
    /// </returns>
    static abstract Result<string> Serialize<T>(T obj);

    /// <summary>
    /// Parst einen JSON-String in ein flaches <c>Dictionary&lt;string, string&gt;</c>.
    /// </summary>
    /// <remarks>
    /// Das JSON muss ein flaches Objekt darstellen, bei dem jeder Wert ein JSON-String ist, z.&#160;B.
    /// <c>{"Key": "Value"}</c>. Verschachtelte Objekte oder Nicht-String-Werte werden nicht unterstützt
    /// und führen zu einem Deserialisierungsfehler.
    /// </remarks>
    /// <param name="raw">Der zu parsende JSON-String.</param>
    /// <returns>
    /// Ein <see cref="Result{T}"/> mit dem geparsten Dictionary bei Erfolg, oder ein Fehler, wenn das
    /// JSON fehlerhaft ist oder nicht als String-zu-String-Dictionary deserialisiert werden kann.
    /// </returns>
    static abstract Result<Dictionary<string, string>> Deserialize(string raw);
}
