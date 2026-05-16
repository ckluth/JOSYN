#pragma warning disable IDE0130
namespace JOSYN.Core.ResultPattern;
#pragma warning restore IDE0130

public record CallerInfo
{
    public string MethodName { get; init; } = "";
    public string ClassName { get; init; } = "";
    public string FilePath { get; init; } = "";
    public int LineNumber { get; init; }
    public override string ToString()
    {
        if (string.IsNullOrEmpty(FilePath) || LineNumber == 0) return $"{ClassName}.{MethodName}()";
        return $"{ClassName}.{MethodName}() in {Path.GetFileName(FilePath)}:{LineNumber}";
    }
}