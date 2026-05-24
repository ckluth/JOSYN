#pragma warning disable IDE0130
namespace JOSYN.Foundation.ResultPattern;
#pragma warning restore IDE0130

/// <summary>
/// Represents a single frame in a result's propagation chain.
/// Populated automatically — you never create these manually.
/// </summary>
public sealed record CallerInfo
{
    /// <summary>
    /// Method name.
    /// </summary>
    public string MethodName { get; init; } = "";

    /// <summary>
    /// Declaring type name.
    /// </summary>
    public string ClassName { get; init; } = "";

    /// <summary>
    /// Source file path. Empty when no PDB is available.
    /// </summary>
    public string FilePath { get; init; } = "";

    /// <summary>
    /// Line number. Zero when no PDB is available.
    /// </summary>
    public int LineNumber { get; init; }

    /// <inheritdoc />
    public override string ToString()
    {
        if (string.IsNullOrEmpty(FilePath) || LineNumber == 0) return $"{ClassName}.{MethodName}()";
        return $"{ClassName}.{MethodName}() in {Path.GetFileName(FilePath)}:{LineNumber}";
    }
}
