using JOSYN.Foundation.ResultPattern;

#pragma warning disable IDE0130
namespace JOSYN.Foundation.PropertyBag;
#pragma warning restore IDE0130

/// <summary>
/// Repräsentiert eine Methode, die ein flaches String-zu-String-Dictionary in einen serialisierten String konvertiert.
/// </summary>
/// <remarks>
/// In dieser Bibliothek enthaltene Implementierungen:
/// <list type="bullet">
///   <item><see cref="IniDictionarySerializer.Serialize(Dictionary{string,string})"/> — erzeugt sektionsfreies INI.</item>
///   <item><see cref="JsonDictionarySerializer.Serialize{T}(T)"/> — erzeugt eingerücktes JSON.</item>
/// </list>
/// Eine Implementierung an <see cref="PropertyBag.Serialize{TRecord}(TRecord, DictionaryToStringSerializer)"/>
/// oder <see cref="PropertyBag.Serialize(object, Type, DictionaryToStringSerializer)"/> übergeben, um das Ausgabeformat zu wählen.
/// </remarks>
/// <param name="data">Das zu serialisierende flache Schlüssel-Wert-Dictionary.</param>
/// <returns>
/// Ein <see cref="Result{T}"/> mit dem serialisierten String bei Erfolg, oder ein Fehler, wenn die Serialisierung fehlschlägt.
/// </returns>
public delegate Result<string> DictionaryToStringSerializer(Dictionary<string, string> data);
