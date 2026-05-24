namespace JOSYN.System.Frontend.JobHost.Attributes;

/// <summary>
/// Deklariert, dass die zugehörige Job-Methode parallel ausgeführt werden darf.
/// Der Parameter <paramref name="isAllowed"/> steuert, ob die parallele Ausführung
/// aktiviert (<c>true</c>, Standard) oder explizit deaktiviert (<c>false</c>) ist.
/// </summary>
[AttributeUsage(AttributeTargets.Method, Inherited = false)]
public sealed class ParallelExecutionAllowedAttribute(bool isAllowed = true) : Attribute
{
    /// <summary>Gibt an, ob parallele Ausführung erlaubt ist.</summary>
    public bool IsAllowed => isAllowed;
}