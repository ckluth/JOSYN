using JOSYN.Foundation.ResultPattern;

#pragma warning disable IDE0130
namespace JOSYN.Foundation.PropertyBag;
#pragma warning restore IDE0130


/// <summary>
/// Repräsentiert eine Methode, die einen serialisierten String in ein flaches String-zu-String-Dictionary parst.
/// </summary>
/// <remarks>
/// In dieser Bibliothek enthaltene Implementierungen:
/// <list type="bullet">
///   <item><see cref="IniDictionarySerializer.DeserializeSingleSection(string)"/> — parst sektionsfreies INI.</item>
///   <item><see cref="JsonDictionarySerializer.Deserialize(string)"/> — parst flaches JSON.</item>
/// </list>
/// Die automatische Formaterkennung (zwischen diesen beiden) ist in
/// <see cref="PropertyBag.Deserialize{TRecord}(string)"/> und seinen Überladungen eingebaut, sodass
/// Aufrufer in der Regel keinen <see cref="StringToDictionarySerializer"/> manuell auswählen müssen.
/// </remarks>
/// <param name="str">Der zu parsende serialisierte String.</param>
/// <returns>
/// Ein <see cref="Result{T}"/> mit dem geparsten flachen Dictionary bei Erfolg, oder ein Fehler, wenn das Parsing fehlschlägt.
/// </returns>
public delegate Result<Dictionary<string, string>> StringToDictionarySerializer(string str);
