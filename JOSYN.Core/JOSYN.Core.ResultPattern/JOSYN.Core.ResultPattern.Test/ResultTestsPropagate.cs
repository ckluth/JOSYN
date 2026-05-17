using NUnit.Framework;

namespace JOSYN.Core.ResultPattern.Test;

[TestFixture]
public class ResultTestsPropagate
{
    [Test]
    public void Propagate_ShouldAccumulateCallStack_AcrossTypes()
    {
        // Act
        var result = LoadUserDisplayName();

        // Assert
        Console.WriteLine("Succeeded:");
        Console.WriteLine(result.Succeeded);
        Console.WriteLine();
        Console.WriteLine("[ErrorMessage]");
        Console.WriteLine(result.ErrorMessage);
        Console.WriteLine();
        Console.WriteLine("[Callstack]");
        Console.WriteLine(result.CallStackAsString);
        if (result.Exception == null) return;
        Console.WriteLine();
        Console.WriteLine("[Exception]");
        Console.WriteLine(result.Exception.ToString());

        Assert.That(result.Callers.Count, Is.EqualTo(4));
    }
    
    // ── Hilfsmethoden ─────────────────────────────────────────────────────────
    private Result LoadUserDisplayName()
    {
        var result = ParseUserAge();
        if (!result.Succeeded) return Result.Propagate(result.ToResult());
        return Result.Success;
    }

    private Result<int> ParseUserAge()
    {
        var result = ReadUserName();
        if (!result.Succeeded) return Result<int>.Propagate(result.ToResult<int>());
        return int.Parse(result.Value);
    }

    private Result<string> ReadUserName()
    {
        var result = LoadUserRecord();
        if (!result.Succeeded) return Result<string>.Propagate(result.ToResult<string>());
        return result.Value.ToString();
    }

    private Result<Guid> LoadUserRecord()
    {
        try
        {
            throw new InvalidOperationException("Datenbankverbindung fehlgeschlagen.");
        }
        catch (Exception ex)
        {
            //return Result<Guid>.Fail(ex);
            return ex;
        }
    }


    [Test]
    public void Propagate_ShouldAccumulateCallStack()
    {
        // Act
        var result = DoA();

        // Assert
        Console.WriteLine("Succeeded:");
        Console.WriteLine(result.Succeeded);
        Console.WriteLine();
        Console.WriteLine("[ErrorMessage]");
        Console.WriteLine(result.ErrorMessage);
        Console.WriteLine();
        Console.WriteLine("[Callstack]");
        Console.WriteLine(result.CallStackAsString);
        if (result.Exception == null) return;
        Console.WriteLine();
        Console.WriteLine("[Exception]");
        Console.WriteLine(result.Exception.ToString());

        Assert.That(result.Callers.Count, Is.EqualTo(4));
    }

    // ── Hilfsmethoden ─────────────────────────────────────────────────────────

    private Result DoA()
    {
        var result = DoB();
        if (!result.Succeeded) return Result.Propagate(result);
        return Result.Success;
    }

    private Result DoB()
    {
        var result = DoC();
        if (!result.Succeeded) return Result.Propagate(result);
        return Result.Success;
    }

    private Result DoC()
    {
        var result = DoD();
        if (!result.Succeeded) return Result.Propagate(result);
        return Result.Success;
    }

    private Result DoD()
    {
        try
        {
            throw new InvalidOperationException("Etwas ist schiefgelaufen.");
        }
        catch (Exception ex)
        {
            return ex; // Result.Fail(ex);
        }
    }

    [Test]
    public void Propagate_Generic_ShouldAccumulateCallStack()
    {
        // Act
        var result = LoadUserDisplayName2();

        // Assert
        Console.WriteLine("Succeeded:");
        Console.WriteLine(result.Succeeded);
        Console.WriteLine();
        Console.WriteLine("[ErrorMessage]");
        Console.WriteLine(result.ErrorMessage);
        Console.WriteLine();
        Console.WriteLine("[Callstack]");
        Console.WriteLine(result.CallStackAsString);
        if (result.Exception == null) return;
        Console.WriteLine();
        Console.WriteLine("[Exception]");
        Console.WriteLine(result.Exception.ToString());

        Assert.That(result.Callers.Count, Is.EqualTo(4));
    }

    // ── Hilfsmethoden ─────────────────────────────────────────────────────────

    private Result<string> LoadUserDisplayName2()
    {
        var result = ParseUserAge2();
        if (!result.Succeeded) return Result<string>.Propagate(result.ToResult<string>());
        return result.Value.ToString();
    }

    private Result<int> ParseUserAge2()
    {
        var result = ReadUserName2();
        if (!result.Succeeded) return Result<int>.Propagate(result.ToResult<int>());
        return int.Parse(result.Value);
    }

    private Result<string> ReadUserName2()
    {
        var result = LoadUserRecord2();
        if (!result.Succeeded) return Result<string>.Propagate(result.ToResult<string>());
        return result.Value.ToString();
    }

    private Result<Guid> LoadUserRecord2()
    {
        try
        {
            throw new InvalidOperationException("Datenbankverbindung fehlgeschlagen.");
        }
        catch (Exception ex)
        {
            return ex; //Result<Guid>.Fail(ex);
        }
    }
}