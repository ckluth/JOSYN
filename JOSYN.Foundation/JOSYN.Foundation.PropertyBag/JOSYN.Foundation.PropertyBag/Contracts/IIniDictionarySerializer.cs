using JOSYN.Foundation.ResultPattern;

#pragma warning disable IDE0130
namespace JOSYN.Foundation.PropertyBag;
#pragma warning restore IDE0130

/// <summary>
/// Serializes and deserializes INI-format data to and from <c>Dictionary</c> representations.
/// </summary>
/// <remarks>
/// Supports both sectioned INI (<c>[SectionName]</c> headers followed by <c>Key=Value</c> lines)
/// and sectionless INI (plain <c>Key=Value</c> lines with no section header).
/// <para>
/// Values are stored verbatim — no trimming is applied to the right-hand side of <c>=</c>.
/// A hand-crafted INI entry such as <c>Key= value</c> will capture the leading space as part
/// of the value. The caller is responsible for the exact content on both sides of <c>=</c>.
/// </para>
/// <para>
/// Lines starting with <c>;</c> and blank lines are treated as comments or whitespace and ignored
/// during deserialization.
/// </para>
/// <para>
/// All operations return <see cref="Result"/> or <see cref="Result{T}"/> — no exceptions propagate
/// up the call stack.
/// </para>
/// </remarks>
public interface IIniDictionarySerializer
{
    /// <summary>
    /// Serializes a multi-section INI dictionary to a string.
    /// </summary>
    /// <remarks>
    /// If the dictionary contains exactly one entry whose key is <see cref="string.Empty"/>, the
    /// output is written without a section header (sectionless INI). Otherwise each entry key
    /// becomes a <c>[SectionName]</c> header followed by its key-value pairs, with a blank line
    /// between sections.
    /// </remarks>
    /// <param name="data">
    /// A dictionary mapping section names to their key-value pairs. Use an empty string as the
    /// section key to produce a sectionless INI document.
    /// </param>
    /// <returns>
    /// A <see cref="Result{T}"/> containing the INI-formatted string on success, or an error if
    /// serialization fails.
    /// </returns>
    static abstract Result<string> Serialize(Dictionary<string, Dictionary<string, string>> data);

    /// <summary>
    /// Serializes a flat key-value dictionary to a sectionless INI string.
    /// </summary>
    /// <remarks>
    /// Each entry in <paramref name="data"/> becomes a <c>Key=Value</c> line. No section header
    /// is written. To include section headers, use
    /// <see cref="IIniDictionarySerializer.Serialize(Dictionary{string,Dictionary{string,string}})"/>.
    /// </remarks>
    /// <param name="data">The key-value pairs to serialize.</param>
    /// <returns>
    /// A <see cref="Result{T}"/> containing the sectionless INI-formatted string on success, or
    /// an error if serialization fails.
    /// </returns>
    static abstract Result<string> Serialize(Dictionary<string, string> data);

    /// <summary>
    /// Parses a sectionless INI string into a flat key-value dictionary.
    /// </summary>
    /// <remarks>
    /// This is a convenience wrapper around
    /// <see cref="IIniDictionarySerializer.Deserialize(string)"/> that enforces a single-section
    /// constraint. Use it when the input is known to be a plain <c>Key=Value</c> document without
    /// section headers — for example, when deserializing output produced by
    /// <see cref="IIniDictionarySerializer.Serialize(Dictionary{string,string})"/>.
    /// </remarks>
    /// <param name="raw">
    /// A sectionless INI string. May contain comment lines (starting with <c>;</c>) and blank lines.
    /// </param>
    /// <returns>
    /// A <see cref="Result{T}"/> containing the parsed flat dictionary on success, or an error if
    /// the input is empty, contains zero parseable sections, contains more than one section, or
    /// parsing fails.
    /// </returns>
    static abstract Result<Dictionary<string, string>> DeserializeSingleSection(string raw);

    /// <summary>
    /// Parses an INI string into a nested dictionary keyed by section name.
    /// </summary>
    /// <remarks>
    /// Lines before the first section header are collected under an empty-string section key.
    /// Duplicate keys within the same section cause an error. Keys across different sections may
    /// be repeated without conflict.
    /// </remarks>
    /// <param name="raw">The INI-formatted string to parse.</param>
    /// <returns>
    /// A <see cref="Result{T}"/> containing the parsed nested dictionary on success, or an error
    /// if a duplicate key is encountered or parsing fails.
    /// </returns>
    static abstract Result<Dictionary<string, Dictionary<string, string>>> Deserialize(string raw);
}
