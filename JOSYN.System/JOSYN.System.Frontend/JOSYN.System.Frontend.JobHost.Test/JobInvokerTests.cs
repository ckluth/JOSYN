using NUnit.Framework;
using JOSYN.Foundation.ResultPattern;
using JOSYN.System.Frontend.JobHost.Attributes;
using JOSYN.System.Frontend.JobHost.Test.Fixtures.Single;
using JOSYN.System.Frontend.JobHost.Test.Fixtures.Multi;

namespace JOSYN.System.Frontend.JobHost.Test;

[TestFixture]
public sealed class JobInvokerTests
{
    // ── Entry point discovery ──────────────────────────────────────────────────

    [Test]
    public async Task InvokeJob_NoEntryPoint_Fails()
    {
        // FakeProtocol is in the test assembly; that assembly has no exported [JobEntryPoint] methods.
        var result = await JobInvoker.InvokeJob(new FakeProtocol(), typeof(FakeProtocol));

        Assert.That(result.Succeeded, Is.False);
        Assert.That(result.ErrorMessage, Does.Contain(nameof(JobEntryPointAttribute)));
    }

    [Test]
    public async Task InvokeJob_MultipleEntryPoints_Fails()
    {
        // JobAlpha is in the Fixtures.Multi assembly, which has two [JobEntryPoint] methods.
        var result = await JobInvoker.InvokeJob(new FakeProtocol(), typeof(JobAlpha));

        Assert.That(result.Succeeded, Is.False);
        Assert.That(result.ErrorMessage, Does.Contain("Mehrere"));
    }

    // ── Successful invocation ──────────────────────────────────────────────────

    [Test]
    public async Task InvokeJob_VoidEntryPoint_Succeeds()
    {
        // VoidJob is in the Fixtures.Single assembly: exactly one [JobEntryPoint], void, no parameters.
        var result = await JobInvoker.InvokeJob(new FakeProtocol(), typeof(VoidJob));

        Assert.That(result.Succeeded, Is.True);
    }
}

