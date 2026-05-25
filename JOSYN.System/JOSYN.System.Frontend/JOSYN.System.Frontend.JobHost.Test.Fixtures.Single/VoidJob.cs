using JOSYN.System.Frontend.JobHost.Attributes;

namespace JOSYN.System.Frontend.JobHost.Test.Fixtures.Single;

/// <summary>
/// Test fixture: assembly with exactly one <see cref="JobEntryPointAttribute"/> method.
/// Used by unit tests that verify the "single entry point, void, no parameters" path.
/// </summary>
public static class VoidJob
{
    [JobEntryPoint]
    public static void Run() { }
}
