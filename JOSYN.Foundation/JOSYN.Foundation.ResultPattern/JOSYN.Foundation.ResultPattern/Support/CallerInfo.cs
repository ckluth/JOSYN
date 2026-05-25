#pragma warning disable IDE0130
namespace JOSYN.Foundation.ResultPattern;
#pragma warning restore IDE0130

/// <summary>
/// Repräsentiert einen einzelnen Eintrag in der Weiterleitungskette eines Ergebnisses.
/// Wird automatisch befüllt — manuelle Erzeugung ist nicht vorgesehen.
/// </summary>
public sealed record CallerInfo
{
    /// <summary>
    /// Methodenname.
    /// </summary>
    public string MethodName { get; init; } = "";

    /// <summary>
    /// Name des deklarierenden Typs.
    /// </summary>
    public string ClassName { get; init; } = "";

    /// <summary>
    /// Pfad zur Quelldatei. Leer, wenn keine PDB-Datei verfügbar ist.
    /// </summary>
    public string FilePath { get; init; } = "";

    /// <summary>
    /// Zeilennummer. Null, wenn keine PDB-Datei verfügbar ist.
    /// </summary>
    public int LineNumber { get; init; }

    /// <inheritdoc />
    public override string ToString()
    {
        if (string.IsNullOrEmpty(FilePath) || LineNumber == 0) return $"{ClassName}.{MethodName}()";
        return $"{ClassName}.{MethodName}() in {Path.GetFileName(FilePath)}:{LineNumber}";
    }
}
