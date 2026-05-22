using JOSYN.Core.ResultPattern;

#pragma warning disable IDE0130
namespace JOSYN.Core.PropertyBag;
#pragma warning restore IDE0130


/// <summary>
/// Represents a method that parses a serialized string into a flat string-to-string dictionary.
/// </summary>
/// <remarks>
/// Implementations provided by this library:
/// <list type="bullet">
///   <item><see cref="IniDictionarySerializer.DeserializeSingleSection(string)"/> — parses sectionless INI.</item>
///   <item><see cref="JsonDictionarySerializer.Deserialize(string)"/> — parses flat-object JSON.</item>
/// </list>
/// Format auto-detection (choosing between these two) is built into
/// <see cref="PropertyBag.Deserialize{TRecord}(string)"/> and its overloads, so callers
/// typically do not need to pick a <see cref="StringToDictionarySerializer"/> manually.
/// </remarks>
/// <param name="str">The serialized string to parse.</param>
/// <returns>
/// A <see cref="Result{T}"/> containing the parsed flat dictionary on success, or an error if parsing fails.
/// </returns>
public delegate Result<Dictionary<string, string>> StringToDictionarySerializer(string str);