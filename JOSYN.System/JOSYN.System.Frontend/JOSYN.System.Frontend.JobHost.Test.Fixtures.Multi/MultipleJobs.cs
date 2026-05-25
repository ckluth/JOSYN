using JOSYN.System.Frontend.JobHost.Attributes;

namespace JOSYN.System.Frontend.JobHost.Test.Fixtures.Multi;

/// <summary>
/// Test fixture: assembly with two <see cref="JobEntryPointAttribute"/> methods.
/// Used by unit tests that verify the "multiple entry points found" error path.
/// </summary>
public static class JobAlpha
{
    [JobEntryPoint]
    public static void Run() { }
}

public static class JobBeta
{
    [JobEntryPoint]
    public static void Run() { }
}
