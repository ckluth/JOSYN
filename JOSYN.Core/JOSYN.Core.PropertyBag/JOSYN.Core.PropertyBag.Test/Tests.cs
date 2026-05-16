using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using NUnit.Framework;

namespace JOSYN.Core.PropertyBag.Test;

public record MyArguments
{
    public required string Name { get; init; }
    public int Count { get; init; }
    public DateOnly Date { get; init; }
}

[TestFixture]
internal class Tests
{
    [Test]
    public void Serialize_Back_Forth_String_Record_Ini_Json_Success()
    {
        const string name = "Hello JOSYN";
        const int count = 9;
        var date = DateOnly.Parse("04.11.1966", new CultureInfo("de-DE"));
        
        var inicontent = $"""
                             Name={name}
                             Count={count}
                             Date={date:dd.MM.yyyy}
                             """;
        
        // Record aus Ini-String
        var getRec = PropertyBag.Deserialize<MyArguments>(inicontent);
        if (!getRec.Succeeded)
        {
            Console.WriteLine(getRec.ErrorMessage);
            Console.WriteLine(getRec.CallStackToString());
            Console.WriteLine(getRec.Exception);
            Assert.Fail($"PropertyBag.Deserialize<MyArguments>(inicontent) failed: {getRec.ErrorMessage}");
        }
        
        var rec = getRec.Value!;
        Assert.That(rec.Count, Is.EqualTo(count));
        Assert.That(rec.Name, Is.EqualTo(name));
        Assert.That(rec.Date, Is.EqualTo(date));
        
        // Ini-String aus Value
        var str = PropertyBag.Serialize(rec, IniDictionarySerializer.Serialize);
        if (!str.Succeeded) 
        {
            Console.WriteLine(str.ErrorMessage);
            Console.WriteLine(str.CallStackToString());
            Console.WriteLine(str.Exception);
            Assert.Fail($"PropertyBag.Serialize<MyArguments>(rec) failed: {str.ErrorMessage}");
        }
        Assert.That(str.Value!.StartsWith($"Name={name}"));
        
        Console.WriteLine("[Record to Ini-String]");
        Console.WriteLine(str.Value!.Trim());
        Console.WriteLine();
        
        // Json-String aus Value
        var getJsonStr = PropertyBag.Serialize(rec, JsonDictionarySerializer.Serialize);
        if (!getJsonStr.Succeeded)
        {
            Console.WriteLine(getJsonStr.ErrorMessage);
            Console.WriteLine(getJsonStr.CallStackToString());
            Console.WriteLine(getJsonStr.Exception);
            Assert.Fail($"PropertyBag.Serialize(rec, JsonDictionarySerializer.Serialize) failed: {getJsonStr.ErrorMessage}");
        }
        Assert.That(getJsonStr.Value!.Contains($"\"Name\": \"{name}\""));
        
        Console.WriteLine("[Record to Json-String]");
        Console.WriteLine(getJsonStr.Value);
        Console.WriteLine();
        
        // Value aus Json-String
        var getRec2 = PropertyBag.Deserialize<MyArguments>(getJsonStr.Value);
        if (!getRec2.Succeeded)
        {
            Console.WriteLine(getRec2.ErrorMessage);
            Console.WriteLine(getRec2.CallStackToString());
            Console.WriteLine(getRec2.Exception);
            Assert.Fail($"PropertyBag.Deserialize<MyArguments>(getJsonStr.Value) failed: {getRec2.ErrorMessage}");
        }
        
        var rec2 = getRec2.Value!;
        Assert.That(rec2.Count, Is.EqualTo(count));
        Assert.That(rec2.Name, Is.EqualTo(name));
        Assert.That(rec2.Date, Is.EqualTo(date));

        Console.WriteLine("[Json-String to Record]");
        Console.WriteLine(getRec2.Value);
    }

}

