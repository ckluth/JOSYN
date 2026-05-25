using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

#pragma warning disable IDE0130
namespace JOSYN.Foundation.PropertyBag;
#pragma warning restore IDE0130


/// <summary>
/// Ein <see cref="JsonConverter{T}"/> für <see cref="TimeOnly"/>, der Werte gemäß der aktuellen
/// Thread-Kultur (Standard: <c>de-DE</c>) formatiert und parst.
/// </summary>
internal sealed class CultureAwareTimeOnlyConverter : JsonConverter<TimeOnly>
{
    /// <inheritdoc/>
    public override TimeOnly Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) => TimeOnly.Parse(reader.GetString()!, CultureInfo.CurrentCulture);

    /// <inheritdoc/>
    public override void Write(Utf8JsonWriter writer, TimeOnly value, JsonSerializerOptions options) => writer.WriteStringValue(value.ToString(CultureInfo.CurrentCulture));
}
