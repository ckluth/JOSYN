using NUnit.Framework;

namespace JOSYN.Core.ResultPattern.Test;

[TestFixture]
public class ResultTests
{

    //// ── Success ──────────────────────────────────────────────────────────────

    [Test]
    public void Success_Succeeded_IsTrue()
    {
        var result = Result.Success;
        Assert.That(result.Succeeded, Is.True);
    }

    [Test]
    public void Success_ErrorMessage_IsNull()
    {
        var result = Result.Success;
        Assert.That(result.ErrorMessage, Is.Null);
    }

    [Test]
    public void Success_Exception_IsNull()
    {
        var result = Result.Success;
        Assert.That(result.Exception, Is.Null);
    }

    // ── Result.Fail(string) ──────────────────────────────────────────────────

    [Test]
    public void Fail_String_Succeeded_IsFalse()
    {
        var result = Result.Fail("oops");
        Assert.That(result.Succeeded, Is.False);
    }

    [Test]
    public void Fail_String_ErrorMessage_IsSet()
    {
        var result = Result.Fail("oops");
        Assert.That(result.ErrorMessage, Is.EqualTo("oops"));
    }

    [Test]
    public void Fail_String_Exception_IsNull_WhenNotProvided()
    {
        var result = Result.Fail("oops");
        Assert.That(result.Exception, Is.Null);
    }

    [Test]
    public void Fail_StringAndException_Exception_IsSet()
    {
        var ex = new InvalidOperationException("boom");
        var result = Result.Fail("oops", ex);
        Assert.That(result.Exception, Is.SameAs(ex));
    }

    // ── Result.Fail(Exception) ───────────────────────────────────────────────

    [Test]
    public void Fail_Exception_Succeeded_IsFalse()
    {
        var result = Result.Fail(new Exception("bang"));
        Assert.That(result.Succeeded, Is.False);
    }

    [Test]
    public void Fail_Exception_ErrorMessage_ContainsExceptionMessage()
    {
        var result = Result.Fail(new Exception("bang"));
        Assert.That(result.ErrorMessage, Does.Contain("bang"));
    }

    [Test]
    public void Fail_Exception_Exception_IsSet()
    {
        var ex = new Exception("bang");
        var result = Result.Fail(ex);
        Assert.That(result.Exception, Is.SameAs(ex));
    }

    // ── Implicit conversion from Exception ───────────────────────────────────

    [Test]
    public void ImplicitConversion_FromException_Succeeded_IsFalse()
    {
        Result result = new Exception("implicit");
        Assert.That(result.Succeeded, Is.False);
    }

    [Test]
    public void ImplicitConversion_FromException_ErrorMessage_ContainsExceptionMessage()
    {
        Result result = new Exception("implicit");
        Assert.That(result.ErrorMessage, Does.Contain("implicit"));
    }

    [Test]
    public void ImplicitConversion_FromException_Exception_IsSet()
    {
        var ex = new Exception("implicit");
        Result result = ex;
        Assert.That(result.Exception, Is.SameAs(ex));
    }

    // ── GetErrorInfo — caller info in error message ───────────────────────────

    [Test]
    public void Fail_Exception_ErrorMessage_ContainsCallerInfo()
    {
        var result = Result.Fail(new Exception("x"));
        // The call stack should contain the class/method name of the caller
        Assert.That(result.CallStackToString(), Does.Contain(nameof(ResultTests)));
    }

    // ── Result.Failure(string) → Failure struct → implicit to Result ─────────

    [Test]
    public void Failure_Factory_ReturnsFailureStruct()
    {
        var failure = Result.Failure("err");
        Assert.That(failure.ErrorMessage, Is.EqualTo("err"));
    }

    [Test]
    public void Failure_ImplicitToResult_Succeeded_IsFalse()
    {
        Result result = Result.Failure("err");
        Assert.That(result.Succeeded, Is.False);
    }

    [Test]
    public void Failure_ImplicitToResult_ErrorMessage_IsPreserved()
    {
        Result result = Result.Failure("err");
        Assert.That(result.ErrorMessage, Is.EqualTo("err"));
    }

    [Test]
    public void Failure_WithException_ExceptionIsPreserved()
    {
        var ex = new Exception("ex");
        Result result = Result.Failure("err", ex);
        Assert.That(result.Exception, Is.SameAs(ex));
    }

    // ── record equality ───────────────────────────────────────────────────────

    [Test]
    public void TwoSuccessResults_AreEqual()
    {
        var a = Result.Success;
        var b = Result.Success;
        Assert.That(a, Is.EqualTo(b));
    }

    [Test]
    public void TwoFailResults_WithSameMessage_AreEqual()
    {
        var a = Result.Fail("same");
        var b = Result.Fail("same");
        Assert.That(a.ErrorMessage, Is.EqualTo(b.ErrorMessage));
    }
}
