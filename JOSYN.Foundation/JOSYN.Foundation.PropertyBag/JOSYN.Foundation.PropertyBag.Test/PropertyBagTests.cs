using NUnit.Framework;

namespace JOSYN.Foundation.PropertyBag.Test;

#region Shared test types

public record SimpleRecord
{
    public required string Name { get; init; }
    public int Count { get; init; }
}

// Primary-constructor (positional) style — no parameterless ctor
public record PositionalRecord(string Title, int Value);
public record PositionalNullableRecord(string Name, int? Count);

public record NullablePropertiesRecord
{
    public required string RequiredName { get; init; }
    public string? OptionalName { get; init; }
    public int? OptionalCount { get; init; }
}

public record UnsupportedTypeRecord
{
    public List<string> Items { get; init; } = [];
}

public enum Color { Red, Green, Blue }

public record EnumRecord
{
    public Color Favorite { get; init; }
}

public class PlainClass
{
    public string Name { get; set; } = "";
}

public record DateTimeOffsetRecord
{
    public DateTimeOffset Timestamp { get; init; }
}

#endregion

[TestFixture]
internal class PropertyBagTests
{
    // ── Serialize<TRecord> ──────────────────────────────────────────────────

    [Test]
    public void Serialize_Record_WithIniSerializer_ProducesKeyValueLines()
    {
        var record = new SimpleRecord { Name = "JOSYN", Count = 42 };

        var result = PropertyBag.Serialize(record, IniDictionarySerializer.Serialize);

        Assert.That(result.Succeeded, Is.True);
        Assert.That(result.Value, Does.Contain("Name=JOSYN"));
        Assert.That(result.Value, Does.Contain("Count=42"));
    }

    [Test]
    public void Serialize_Record_WithJsonSerializer_ProducesJsonObject()
    {
        var record = new SimpleRecord { Name = "JOSYN", Count = 42 };

        var result = PropertyBag.Serialize(record, JsonDictionarySerializer.Serialize);

        Assert.That(result.Succeeded, Is.True);
        Assert.That(result.Value, Does.Contain("\"Name\""));
        Assert.That(result.Value, Does.Contain("\"JOSYN\""));
    }

    [Test]
    public void Serialize_NonRecordClass_ReturnsFailureWithTypeName()
    {
        var plain = new PlainClass { Name = "test" };

        var result = PropertyBag.Serialize(plain, IniDictionarySerializer.Serialize);

        Assert.That(result.Succeeded, Is.False);
        Assert.That(result.ErrorMessage, Does.Contain("PlainClass"));
    }

    [Test]
    public void Serialize_RecordWithUnsupportedPropertyType_ReturnsFailure()
    {
        var record = new UnsupportedTypeRecord { Items = ["a"] };

        var result = PropertyBag.Serialize(record, IniDictionarySerializer.Serialize);

        Assert.That(result.Succeeded, Is.False);
        Assert.That(result.ErrorMessage, Does.Contain("UnsupportedTypeRecord"));
    }

    // ── Serialize(object, Type, ...) non-generic overload ──────────────────

    [Test]
    public void Serialize_NonGenericOverload_ProducesCorrectOutput()
    {
        var record = new SimpleRecord { Name = "Generic", Count = 7 };

        var result = PropertyBag.Serialize(record, typeof(SimpleRecord), IniDictionarySerializer.Serialize);

        Assert.That(result.Succeeded, Is.True);
        Assert.That(result.Value, Does.Contain("Name=Generic"));
    }

    [Test]
    public void Serialize_NonGenericOverload_NonRecordType_ReturnsFailure()
    {
        var plain = new PlainClass { Name = "test" };

        var result = PropertyBag.Serialize(plain, typeof(PlainClass), IniDictionarySerializer.Serialize);

        Assert.That(result.Succeeded, Is.False);
    }

    // ── Deserialize<TRecord> with format auto-detection ─────────────────────

    [Test]
    public void Deserialize_IniInput_AutoDetectsFormatAndReturnsRecord()
    {
        const string ini = "Name=Hello\nCount=3";

        var result = PropertyBag.Deserialize<SimpleRecord>(ini);

        Assert.That(result.Succeeded, Is.True);
        Assert.That(result.Value!.Name, Is.EqualTo("Hello"));
        Assert.That(result.Value.Count, Is.EqualTo(3));
    }

    [Test]
    public void Deserialize_JsonInput_AutoDetectsFormatAndReturnsRecord()
    {
        const string json = """{"Name": "World", "Count": "5"}""";

        var result = PropertyBag.Deserialize<SimpleRecord>(json);

        Assert.That(result.Succeeded, Is.True);
        Assert.That(result.Value!.Name, Is.EqualTo("World"));
        Assert.That(result.Value.Count, Is.EqualTo(5));
    }

    [Test]
    public void Deserialize_NonRecordType_ReturnsFailureWithTypeName()
    {
        const string ini = "Name=test";

        var result = PropertyBag.Deserialize(ini, typeof(PlainClass));

        Assert.That(result.Succeeded, Is.False);
        Assert.That(result.ErrorMessage, Does.Contain("PlainClass"));
    }

    [Test]
    public void Deserialize_MissingNonNullableProperty_ReturnsFailure()
    {
        const string ini = "Count=5";  // Name is required but absent

        var result = PropertyBag.Deserialize<SimpleRecord>(ini);

        Assert.That(result.Succeeded, Is.False);
        Assert.That(result.ErrorMessage, Is.Not.Null.And.Not.Empty);
    }

    [Test]
    public void Deserialize_InvalidValueForPropertyType_FailureIsPropagatedWithMessage()
    {
        // Regression for bug fixed in session 0002: generic Deserialize<TRecord> was
        // returning Succeeded=true with a null Value when DeserializeFromDictionary failed.
        const string ini = "Name=Hello\nCount=not-a-number";

        var result = PropertyBag.Deserialize<SimpleRecord>(ini);

        Assert.That(result.Succeeded, Is.False);
        Assert.That(result.ErrorMessage, Is.Not.Null.And.Not.Empty);
    }

    // ── Enum support ────────────────────────────────────────────────────────

    [Test]
    public void Serialize_EnumProperty_SerializesAsName()
    {
        var record = new EnumRecord { Favorite = Color.Green };

        var result = PropertyBag.Serialize(record, IniDictionarySerializer.Serialize);

        Assert.That(result.Succeeded, Is.True);
        Assert.That(result.Value, Does.Contain("Favorite=Green"));
    }

    [Test]
    public void Deserialize_EnumProperty_ParsedCaseInsensitively()
    {
        const string ini = "Favorite=green";

        var result = PropertyBag.Deserialize<EnumRecord>(ini);

        Assert.That(result.Succeeded, Is.True);
        Assert.That(result.Value!.Favorite, Is.EqualTo(Color.Green));
    }

    // ── Nullable property handling ──────────────────────────────────────────

    [Test]
    public void Deserialize_NullableProperty_MissingKeyIsAllowed()
    {
        const string ini = "RequiredName=Present";

        var result = PropertyBag.Deserialize<NullablePropertiesRecord>(ini);

        Assert.That(result.Succeeded, Is.True);
        Assert.That(result.Value!.OptionalName, Is.Null);
        Assert.That(result.Value.OptionalCount, Is.Null);
    }

    [Test]
    public void Deserialize_NullableProperty_EmptyValueDeserializesAsNull()
    {
        const string ini = "RequiredName=Present\nOptionalName=\nOptionalCount=";

        var result = PropertyBag.Deserialize<NullablePropertiesRecord>(ini);

        Assert.That(result.Succeeded, Is.True);
        Assert.That(result.Value!.OptionalName, Is.Null);
        Assert.That(result.Value.OptionalCount, Is.Null);
    }

    // ── Round-trips ─────────────────────────────────────────────────────────

    [Test]
    public void Serialize_ThenDeserialize_IniRoundTrip_PreservesValues()
    {
        var original = new SimpleRecord { Name = "RoundTrip", Count = 99 };

        var serialized = PropertyBag.Serialize(original, IniDictionarySerializer.Serialize);
        Assert.That(serialized.Succeeded, Is.True);

        var result = PropertyBag.Deserialize<SimpleRecord>(serialized.Value!);

        Assert.That(result.Succeeded, Is.True);
        Assert.That(result.Value!.Name, Is.EqualTo(original.Name));
        Assert.That(result.Value.Count, Is.EqualTo(original.Count));
    }

    [Test]
    public void Serialize_ThenDeserialize_JsonRoundTrip_PreservesValues()
    {
        var original = new SimpleRecord { Name = "RoundTrip", Count = 99 };

        var serialized = PropertyBag.Serialize(original, JsonDictionarySerializer.Serialize);
        Assert.That(serialized.Succeeded, Is.True);

        var result = PropertyBag.Deserialize<SimpleRecord>(serialized.Value!);

        Assert.That(result.Succeeded, Is.True);
        Assert.That(result.Value!.Name, Is.EqualTo(original.Name));
        Assert.That(result.Value.Count, Is.EqualTo(original.Count));
    }

    [Test]
    public void Serialize_ThenDeserialize_NullableNull_RoundTrip_PreservesNull()
    {
        var original = new NullablePropertiesRecord { RequiredName = "R", OptionalName = null, OptionalCount = null };

        var serialized = PropertyBag.Serialize(original, IniDictionarySerializer.Serialize);
        Assert.That(serialized.Succeeded, Is.True);

        var result = PropertyBag.Deserialize<NullablePropertiesRecord>(serialized.Value!);

        Assert.That(result.Succeeded, Is.True);
        Assert.That(result.Value!.OptionalName, Is.Null);
        Assert.That(result.Value.OptionalCount, Is.Null);
    }

    // ── Primary-constructor (positional) records ────────────────────────────

    [Test]
    public void Serialize_PositionalRecord_ProducesKeyValueLines()
    {
        var record = new PositionalRecord("Copilot", 7);

        var result = PropertyBag.Serialize(record, IniDictionarySerializer.Serialize);

        Assert.That(result.Succeeded, Is.True);
        Assert.That(result.Value, Does.Contain("Title=Copilot"));
        Assert.That(result.Value, Does.Contain("Value=7"));
    }

    [Test]
    public void Deserialize_PositionalRecord_IniRoundTrip_PreservesValues()
    {
        var original = new PositionalRecord("RoundTrip", 42);

        var serialized = PropertyBag.Serialize(original, IniDictionarySerializer.Serialize);
        Assert.That(serialized.Succeeded, Is.True);

        var result = PropertyBag.Deserialize<PositionalRecord>(serialized.Value!);

        Assert.That(result.Succeeded, Is.True);
        Assert.That(result.Value!.Title, Is.EqualTo("RoundTrip"));
        Assert.That(result.Value.Value, Is.EqualTo(42));
    }

    [Test]
    public void Deserialize_PositionalRecord_JsonRoundTrip_PreservesValues()
    {
        var original = new PositionalRecord("JSON", 99);

        var serialized = PropertyBag.Serialize(original, JsonDictionarySerializer.Serialize);
        Assert.That(serialized.Succeeded, Is.True);

        var result = PropertyBag.Deserialize<PositionalRecord>(serialized.Value!);

        Assert.That(result.Succeeded, Is.True);
        Assert.That(result.Value!.Title, Is.EqualTo("JSON"));
        Assert.That(result.Value.Value, Is.EqualTo(99));
    }

    [Test]
    public void Deserialize_PositionalNullableRecord_NullableParamMissingKey_IsAllowed()
    {
        const string ini = "Name=Present";

        var result = PropertyBag.Deserialize<PositionalNullableRecord>(ini);

        Assert.That(result.Succeeded, Is.True);
        Assert.That(result.Value!.Name, Is.EqualTo("Present"));
        Assert.That(result.Value.Count, Is.Null);
    }

    // ── DateTimeOffset support ──────────────────────────────────────────────

    [Test]
    public void Serialize_ThenDeserialize_DateTimeOffset_IniRoundTrip_PreservesValue()
    {
        var ts = new DateTimeOffset(2026, 5, 21, 20, 0, 0, TimeSpan.FromHours(2));
        var original = new DateTimeOffsetRecord { Timestamp = ts };

        var serialized = PropertyBag.Serialize(original, IniDictionarySerializer.Serialize);
        Assert.That(serialized.Succeeded, Is.True);

        var result = PropertyBag.Deserialize<DateTimeOffsetRecord>(serialized.Value!);

        Assert.That(result.Succeeded, Is.True);
        Assert.That(result.Value!.Timestamp, Is.EqualTo(ts));
    }
}
