using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

#pragma warning disable IDE0130
namespace JOSYN.Foundation.PropertyBag;
#pragma warning restore IDE0130

/// <summary>
/// Ein <see cref="JsonConverter{T}"/> für <see cref="decimal"/>, der Werte gemäß der aktuellen
/// Thread-Kultur (Standard: <c>de-DE</c>) formatiert und parst, unter Beibehaltung des
/// kulturspezifischen Dezimaltrennzeichens (z.&#160;B. <c>,</c> statt <c>.</c>).
/// </summary>
internal sealed class CultureAwareDecimalConverter : JsonConverter<decimal>
{
    /// <inheritdoc/>
    public override decimal Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) => decimal.Parse(reader.GetString()!, CultureInfo.CurrentCulture);

    /// <inheritdoc/>
    public override void Write(Utf8JsonWriter writer, decimal value, JsonSerializerOptions options) => writer.WriteStringValue(value.ToString(CultureInfo.CurrentCulture));
}
