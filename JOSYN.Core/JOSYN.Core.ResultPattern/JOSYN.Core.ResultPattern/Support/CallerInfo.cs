#pragma warning disable IDE0130
namespace JOSYN.Core.ResultPattern;
#pragma warning restore IDE0130

/// <summary>
/// TODO
/// </summary>
public record CallerInfo
{
    /// <summary>
    /// TODO
    /// </summary>
    public string MethodName { get; init; } = "";

    /// <summary>
    /// TODO
    /// </summary>
    public string ClassName { get; init; } = "";

    /// <summary>
    /// TODO
    /// </summary>
    public string FilePath { get; init; } = "";

    /// <summary>
    /// TODO
    /// </summary>
    public int LineNumber { get; init; }

    /// <summary>
    /// TODO
    /// </summary>
    public override string ToString()
    {
        if (string.IsNullOrEmpty(FilePath) || LineNumber == 0) return $"{ClassName}.{MethodName}()";
        return $"{ClassName}.{MethodName}() in {Path.GetFileName(FilePath)}:{LineNumber}";
    }
}