using System.Reflection;
using JOSYN.Core.ResultPattern;

#pragma warning disable IDE0130
namespace JOSYN.Core.PropertyBag;
#pragma warning restore IDE0130

/// <summary>
/// Serializes and deserializes C# <c>record class</c> types to and from string-based formats.
/// </summary>
/// <remarks>
/// Supported formats are sectionless INI (<c>Key=Value</c> lines) and JSON. When deserializing,
/// the format is auto-detected: a string whose first non-whitespace character is <c>{</c> is treated
/// as JSON; everything else is treated as INI.
/// <para>
/// Only <c>record class</c> types are accepted (detected at runtime via the compiler-generated
/// <c>&lt;Clone&gt;$</c> method). Property types must belong to the set of supported primitive and
/// well-known BCL types.
/// </para>
/// <para>
/// All operations return <see cref="Result"/> or <see cref="Result{T}"/> — no exceptions propagate
/// up the call stack.
/// </para>
/// </remarks>
public interface IPropertyBag
{
    /// <summary>
    /// TODO
    /// </summary>
    /// <typeparam name="TRecord"></typeparam>
    /// <param name="record"></param>
    /// <returns></returns>
    static abstract Result<string> Serialize<TRecord>(TRecord record)
        where TRecord : class;


    /// <summary>
    /// 
    /// </summary>
    /// <param name="record"></param>
    /// <param name="recordType"></param>
    /// <returns></returns>
    static abstract Result<string> Serialize(object record, Type recordType);


    /// <summary>
    /// Serializes a <c>record class</c> instance to a string using the provided format serializer.
    /// </summary>
    /// <typeparam name="TRecord">
    /// The record type to serialize. Must be a <c>record class</c>.
    /// </typeparam>
    /// <param name="record">The record instance to serialize.</param>
    /// <param name="serializeToString">
    /// A <see cref="DictionaryToStringSerializer"/> delegate that converts the intermediate flat
    /// <c>Dictionary&lt;string, string&gt;</c> representation into the final string.
    /// Use <see cref="IniDictionarySerializer.Serialize(Dictionary{string,string})"/> for INI
    /// or <see cref="JsonDictionarySerializer.Serialize{T}(T)"/> for JSON.
    /// </param>
    /// <returns>
    /// A <see cref="Result{T}"/> containing the serialized string on success, or an error if the
    /// type is not a <c>record class</c>, contains unsupported property types, or serialization fails.
    /// </returns>
    static abstract Result<string> Serialize<TRecord>(TRecord record, DictionaryToStringSerializer serializeToString)
        where TRecord : class;

    /// <summary>
    /// Serializes a <c>record class</c> instance supplied as <see cref="object"/> to a string
    /// using the provided format serializer.
    /// </summary>
    /// <remarks>
    /// Use this overload when the concrete record type is only known at runtime. For compile-time
    /// known types, prefer <see cref="IPropertyBag.Serialize{TRecord}(TRecord, DictionaryToStringSerializer)"/>.
    /// </remarks>
    /// <param name="record">The record instance to serialize.</param>
    /// <param name="recordType">
    /// The exact <see cref="Type"/> of the record. Must correspond to a <c>record class</c>.
    /// </param>
    /// <param name="serializeToString">
    /// A <see cref="DictionaryToStringSerializer"/> delegate that converts the intermediate flat
    /// <c>Dictionary&lt;string, string&gt;</c> representation into the final string.
    /// </param>
    /// <returns>
    /// A <see cref="Result{T}"/> containing the serialized string on success, or an error if
    /// <paramref name="recordType"/> is not a <c>record class</c>, contains unsupported property
    /// types, or serialization fails.
    /// </returns>
    static abstract Result<string> Serialize(object record, Type recordType, DictionaryToStringSerializer serializeToString);

    /// <summary>
    /// Auto-detects the string format (INI or JSON) and deserializes the input into an instance
    /// of <paramref name="recordType"/>, returned as <see cref="object"/>.
    /// </summary>
    /// <remarks>
    /// Use this overload when the target type is only known at runtime. For compile-time known
    /// types, prefer <see cref="IPropertyBag.Deserialize{TRecord}(string)"/>.
    /// </remarks>
    /// <param name="raw">
    /// The serialized string to parse. Must be in sectionless INI or JSON format.
    /// </param>
    /// <param name="recordType">The target <c>record class</c> type to deserialize into.</param>
    /// <returns>
    /// A <see cref="Result{T}"/> containing the deserialized record as <see cref="object"/> on
    /// success, or an error if format detection, parsing, type validation, or property conversion fails.
    /// </returns>
    static abstract Result<object> Deserialize(string raw, Type recordType);

    /// <summary>
    /// Auto-detects the string format (INI or JSON) and deserializes the input into a
    /// strongly-typed <typeparamref name="TRecord"/> instance.
    /// </summary>
    /// <typeparam name="TRecord">
    /// The target <c>record class</c> type. Must have a parameterless constructor (all record
    /// classes generated by the compiler satisfy this requirement).
    /// </typeparam>
    /// <param name="raw">
    /// The serialized string to parse. Must be in sectionless INI or JSON format.
    /// </param>
    /// <returns>
    /// A <see cref="Result{T}"/> containing the deserialized <typeparamref name="TRecord"/> on
    /// success, or an error if format detection, parsing, type validation, or property conversion fails.
    /// </returns>
    static abstract Result<TRecord> Deserialize<TRecord>(string raw)
        where TRecord : class;

    /// <summary>
    /// Auto-detects the string format (INI or JSON) and deserializes the input into an array of
    /// converted method invocation arguments.
    /// </summary>
    /// <remarks>
    /// Keys in the serialized data are matched against parameter names case-insensitively on the
    /// first character. Nullable parameters that are absent from the data are silently set to
    /// <see langword="null"/>. Non-nullable parameters that are absent cause an error.
    /// <para>
    /// The returned array is positionally aligned with <paramref name="parameters"/> and can be
    /// passed directly to <see cref="System.Reflection.MethodBase.Invoke(object?, object?[])"/>.
    /// </para>
    /// </remarks>
    /// <param name="raw">
    /// The serialized string to parse. Must be in sectionless INI or JSON format.
    /// </param>
    /// <param name="parameters">
    /// The method parameter descriptors to match against the parsed key-value pairs.
    /// </param>
    /// <returns>
    /// A <see cref="Result{T}"/> containing an <see cref="object"/>[] of converted argument values
    /// on success, or an error if a required argument is missing or a type conversion fails.
    /// </returns>
    static abstract Result<object[]> Deserialize(string raw, ParameterInfo[] parameters);
}
