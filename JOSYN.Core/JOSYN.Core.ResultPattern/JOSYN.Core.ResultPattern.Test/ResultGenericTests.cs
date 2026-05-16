using NUnit.Framework;

namespace JOSYN.Core.ResultPattern.Test;

[TestFixture]
public class ResultGenericTests
{

    // ── Success via implicit conversion ──────────────────────────────────────

    [Test]
    public void ImplicitConversion_FromValue_Succeeded_IsTrue()
    {
        Result<int> result = 42;
        Assert.That(result.Succeeded, Is.True);
    }

    [Test]
    public void ImplicitConversion_FromValue_Value_IsSet()
    {
        Result<int> result = 42;
        Assert.That(result.Value, Is.EqualTo(42));
    }

    [Test]
    public void ImplicitConversion_FromValue_ErrorMessage_IsNull()
    {
        Result<int> result = 42;
        Assert.That(result.ErrorMessage, Is.Null);
    }

    [Test]
    public void ImplicitConversion_FromValue_Exception_IsNull()
    {
        Result<int> result = 42;
        Assert.That(result.Exception, Is.Null);
    }

    [Test]
    public void ImplicitConversion_FromReferenceType_Value_IsSet()
    {
        Result<string> result = "hello";
        Assert.That(result.Value, Is.EqualTo("hello"));
    }

    // ── Result<T>.Success(value) ─────────────────────────────────────────────

    [Test]
    public void Success_Succeeded_IsTrue()
    {
        var result = Result<string>.Success("ok");
        Assert.That(result.Succeeded, Is.True);
    }

    [Test]
    public void Success_Value_IsSet()
    {
        var result = Result<string>.Success("ok");
        Assert.That(result.Value, Is.EqualTo("ok"));
    }

    // ── Result<T>.Fail(string) ───────────────────────────────────────────────

    [Test]
    public void Fail_String_Succeeded_IsFalse()
    {
        var result = Result<int>.Fail("bad");
        Assert.That(result.Succeeded, Is.False);
    }

    [Test]
    public void Fail_String_ErrorMessage_IsSet()
    {
        var result = Result<int>.Fail("bad");
        Assert.That(result.ErrorMessage, Is.EqualTo("bad"));
    }

    [Test]
    public void Fail_String_Value_IsDefault()
    {
        var result = Result<int>.Fail("bad");
        Assert.That(result.Value, Is.EqualTo(0));
    }

    [Test]
    public void Fail_String_Value_IsNull_ForReferenceType()
    {
        var result = Result<string>.Fail("bad");
        Assert.That(result.Value, Is.Null);
    }

    [Test]
    public void Fail_StringAndException_Exception_IsSet()
    {
        var ex = new InvalidOperationException("boom");
        var result = Result<int>.Fail("bad", ex);
        Assert.That(result.Exception, Is.SameAs(ex));
    }

    // ── Result<T>.Fail(Exception) ────────────────────────────────────────────

    [Test]
    public void Fail_Exception_Succeeded_IsFalse()
    {
        var result = Result<int>.Fail(new Exception("bang"));
        Assert.That(result.Succeeded, Is.False);
    }

    [Test]
    public void Fail_Exception_ErrorMessage_ContainsExceptionMessage()
    {
        var result = Result<int>.Fail(new Exception("bang"));
        Assert.That(result.ErrorMessage, Does.Contain("bang"));
    }

    [Test]
    public void Fail_Exception_Exception_IsSet()
    {
        var ex = new Exception("bang");
        var result = Result<int>.Fail(ex);
        Assert.That(result.Exception, Is.SameAs(ex));
    }

    [Test]
    public void Fail_Exception_ErrorMessage_ContainsCallerInfo()
    {
        var result = Result<int>.Fail(new Exception("x"));
        Assert.That(result.CallStackToString(), Does.Contain(nameof(ResultGenericTests)));
    }

    // ── Implicit conversion from Exception ───────────────────────────────────

    [Test]
    public void ImplicitConversion_FromException_Succeeded_IsFalse()
    {
        Result<int> result = new Exception("implicit");
        Assert.That(result.Succeeded, Is.False);
    }

    [Test]
    public void ImplicitConversion_FromException_Value_IsDefault()
    {
        Result<int> result = new Exception("implicit");
        Assert.That(result.Value, Is.EqualTo(0));
    }

    [Test]
    public void ImplicitConversion_FromException_ErrorMessage_ContainsExceptionMessage()
    {
        Result<int> result = new Exception("implicit");
        Assert.That(result.ErrorMessage, Does.Contain("implicit"));
    }

    [Test]
    public void ImplicitConversion_FromException_Exception_IsSet()
    {
        var ex = new Exception("implicit");
        Result<int> result = ex;
        Assert.That(result.Exception, Is.SameAs(ex));
    }

    [Test]
    public void ImplicitConversion_FromException_ErrorMessage_ContainsCallerInfo()
    {
        Result<int> result = new Exception("x");
        Assert.That(result.CallStackToString(), Does.Contain(nameof(ResultGenericTests)));
    }

    // ── Implicit conversion from Failure ─────────────────────────────────────

    [Test]
    public void ImplicitConversion_FromFailure_Succeeded_IsFalse()
    {
        Failure failure = Result.Failure("err");
        Result<int> result = failure;
        Assert.That(result.Succeeded, Is.False);
    }

    [Test]
    public void ImplicitConversion_FromFailure_ErrorMessage_IsPreserved()
    {
        Result<int> result = Result.Failure("err");
        Assert.That(result.ErrorMessage, Is.EqualTo("err"));
    }

    [Test]
    public void ImplicitConversion_FromFailure_Value_IsDefault()
    {
        Result<int> result = Result.Failure("err");
        Assert.That(result.Value, Is.EqualTo(0));
    }

    [Test]
    public void ImplicitConversion_FromFailure_WithException_ExceptionIsPreserved()
    {
        var ex = new Exception("ex");
        Result<int> result = Result.Failure("err", ex);
        Assert.That(result.Exception, Is.SameAs(ex));
    }

    // ── Failure struct ────────────────────────────────────────────────────────

    [Test]
    public void Failure_ImplicitConversion_FromString()
    {
        Failure failure = "something went wrong";
        Assert.That(failure.ErrorMessage, Is.EqualTo("something went wrong"));
        Assert.That(failure.Exception, Is.Null);
    }

    [Test]
    public void Failure_ImplicitConversion_FromException()
    {
        var ex = new InvalidOperationException("oops");
        Failure failure = ex;
        Assert.That(failure.ErrorMessage, Is.EqualTo("oops"));
        Assert.That(failure.Exception, Is.SameAs(ex));
    }

    // ── record equality ───────────────────────────────────────────────────────

    [Test]
    public void TwoSuccessResults_WithSameValue_AreEqual()
    {
        Result<int> a = 7;
        Result<int> b = 7;
        Assert.That(a, Is.EqualTo(b));
    }

    [Test]
    public void TwoFailResults_WithSameMessage_AreEqual()
    {
        var a = Result<int>.Fail("same");
        var b = Result<int>.Fail("same");
        Assert.That(a.ErrorMessage, Is.EqualTo(b.ErrorMessage));
    }

    [Test]
    public void SuccessResult_AndFailResult_AreNotEqual()
    {
        Result<int> success = 1;
        var fail = Result<int>.Fail("err");
        Assert.That(success, Is.Not.EqualTo(fail));
    }

    // ── Typical usage pattern (propagating failures) ──────────────────────────

    [Test]
    public void TypicalPattern_PropagateFailure()
    {
        static Result<int> Inner() => Result<int>.Fail("inner failed");

        static Result<string> Outer()
        {
            var r = Inner();
            if (!r.Succeeded)
                return Result.Failure(r.ErrorMessage, r.Exception);
            return r.Value.ToString();
        }

        var result = Outer();
        Assert.That(result.Succeeded, Is.False);
        Assert.That(result.ErrorMessage, Is.EqualTo("inner failed"));
    }

    [Test]
    public void TypicalPattern_HappyPath()
    {
        var result = Parse("42");
        Assert.That(result.Succeeded, Is.True);
        Assert.That(result.Value, Is.EqualTo(42));
        return;

        static Result<int> Parse(string s) => int.TryParse(s, out var n) ? n : Result.Failure($"Not a number: {s}");
    }

    [Test]
    public void TypicalPattern_FailurePath()
    {
        var result = Parse("abc");
        Assert.That(result.Succeeded, Is.False);
        Assert.That(result.ErrorMessage, Does.Contain("abc"));
        return;

        static Result<int> Parse(string s) => int.TryParse(s, out var n) ? n : Result.Failure($"Not a number: {s}");
    }
}
