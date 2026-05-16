using System.Globalization;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Unicode;

using JOSYN.Core.ResultPattern;

#pragma warning disable IDE0130
namespace JOSYN.Core.PropertyBag;
#pragma warning restore IDE0130

public static class JsonDictionarySerializer
{
    static JsonDictionarySerializer()
    {
        var culture = new CultureInfo("de-DE");
        CultureInfo.DefaultThreadCurrentCulture = culture;
        CultureInfo.DefaultThreadCurrentUICulture = culture;
    }

    public static Result<string> Serialize<T>(T obj)
    {
        try
        {
            return JsonSerializer.Serialize(obj, CreateCultureAwareOptions());
        }
        catch (Exception ex) { return ex; }
    }

    public static Result<Dictionary<string, string>> Deserialize(string raw)
    {
        try
        {
            var result = Deserialize<Dictionary<string, string>>(raw);
            if (!result.Succeeded)
                return Result<Dictionary<string, string>>.Propagate(result);
            
            return result.Value;
        }
        catch (Exception ex) { return ex; }
    }

    private static Result<T> Deserialize<T>(string json)
    {
        try
        {
            var result = JsonSerializer.Deserialize<T>(json, CreateCultureAwareOptions());
            return result == null ? Result<T>.Fail("JsonSerializer.Deserialize<T> returned null.") : Result<T>.Success(result);
        }
        catch (Exception ex) { return ex; }
    }

    #region private

    private static readonly JsonSerializerOptions options = new()
    {
        WriteIndented = true,
        Converters = { new JsonStringEnumConverter() },
        Encoder = JavaScriptEncoder.Create(UnicodeRanges.All)
    };


    private static JsonSerializerOptions CreateCultureAwareOptions()
    {
        var cultureAwareOptions = new JsonSerializerOptions(options);
        cultureAwareOptions.Converters.Add(new CultureAwareDateTimeConverter());
        cultureAwareOptions.Converters.Add(new CultureAwareDecimalConverter());
        cultureAwareOptions.Converters.Add(new CultureAwareDateOnlyConverter());
        cultureAwareOptions.Converters.Add(new CultureAwareTimeOnlyConverter());
        return cultureAwareOptions;
    }
    #endregion
}

